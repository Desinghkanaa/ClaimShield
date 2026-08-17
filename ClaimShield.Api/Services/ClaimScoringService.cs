using System.Text.Json;

using ClaimShield.Api.Constants;
using ClaimShield.Api.Data.Context;
using ClaimShield.Api.Interfaces.Services;
using ClaimShield.Api.Models.DTOs.ClaimScoring;
using ClaimShield.Api.Models.Entities;

using Microsoft.EntityFrameworkCore;

namespace ClaimShield.Api.Services
{
    // =============================================================
    // Two-stage, rule-repository-driven scoring engine.
    //
    // Stage 1 (FNOL) runs once at claim registration. Stage 2
    // (Survey) runs on every survey report submission/resubmission,
    // each run superseding the prior one rather than overwriting it.
    // Composite = Stage1 + Stage2 scores, re-banded against the same
    // thresholds, forced Red if either stage had a Hard rule fire.
    // =============================================================

    public class ClaimScoringService : IClaimScoringService
    {
        private readonly ClaimShieldDbContext _context;
        private readonly IAuditLogService _auditLogService;

        public ClaimScoringService(
            ClaimShieldDbContext context,
            IAuditLogService auditLogService)
        {
            _context = context;
            _auditLogService = auditLogService;
        }

        // =========================================================
        // SCORE A STAGE
        // =========================================================

        public async Task<ScoringStageDto> ScoreStageAsync(
            Guid claimId,
            int stage)
        {
            var claim =
                await _context.Claims
                    .FirstOrDefaultAsync(
                        x => x.ClaimId == claimId);

            if (claim == null)
            {
                throw new InvalidOperationException(
                    "Claim not found.");
            }

            var facts =
                await BuildFactsAsync(
                    claim,
                    stage);

            var rules =
                await _context.ScoringRules
                    .Where(
                        x =>
                            x.Stage == stage &&
                            x.IsActive)
                    .OrderBy(x => x.RuleId)
                    .ToListAsync();

            var triggeredRuleIds = new List<string>();
            var reasonLines = new List<string>();
            var versionParts = new List<string>();
            var scoreValue = 0;
            var hardFlagTriggered = false;

            foreach (var rule in rules)
            {
                versionParts.Add(
                    $"{rule.RuleId}:v{rule.Version}");

                if (!facts.TryGetValue(
                        rule.ConditionField,
                        out var factValue))
                {
                    continue;
                }

                if (!EvaluateCondition(
                        factValue,
                        rule.ConditionOperator,
                        rule.ConditionThreshold))
                {
                    continue;
                }

                triggeredRuleIds.Add(rule.RuleId);

                reasonLines.Add(
                    $"Flagged: {rule.Category} — " +
                    $"{rule.ConditionField} {rule.ConditionOperator} {rule.ConditionThreshold}");

                if (rule.Severity == ScoringSeverityConstants.Hard)
                {
                    hardFlagTriggered = true;
                }
                else
                {
                    scoreValue += rule.Points;
                }
            }

            var band =
                await DetermineBandAsync(
                    scoreValue,
                    hardFlagTriggered);

            var priorResults =
                await _context.ClaimScoringResults
                    .Where(
                        x =>
                            x.ClaimId == claimId &&
                            x.Stage == stage &&
                            x.SupersededBy == null)
                    .ToListAsync();

            var newResult = new ClaimScoringResult
            {
                ClaimScoringResultId = Guid.NewGuid(),

                ClaimId = claimId,

                Stage = stage,

                ScoreValue = scoreValue,

                HardFlagTriggered = hardFlagTriggered,

                Band = band,

                TriggeredRuleIds =
                    JsonSerializer.Serialize(triggeredRuleIds),

                ReasonText =
                    reasonLines.Count > 0
                        ? string.Join(" ", reasonLines)
                        : "No rules triggered.",

                RuleSetVersion =
                    versionParts.Count > 0
                        ? string.Join(",", versionParts)
                        : "none",

                ScoredAt = DateTime.UtcNow
            };

            _context.ClaimScoringResults.Add(newResult);

            foreach (var prior in priorResults)
            {
                prior.SupersededBy = newResult.ClaimScoringResultId;
            }

            await _context.SaveChangesAsync();

            await _auditLogService.LogAsync(
                null,
                $"ClaimScoring.{GetStageName(stage)}.Scored",
                "Claim",
                claimId,
                null,
                new
                {
                    newResult.ScoreValue,
                    newResult.HardFlagTriggered,
                    newResult.Band,
                    TriggeredRuleIds = triggeredRuleIds
                });

            return MapStageToDto(newResult);
        }

        // =========================================================
        // INTERNAL (Surveyor / Approver / Admin) VIEW
        // =========================================================

        public async Task<InternalClaimScoringDto?> GetInternalScoringAsync(
            Guid claimId)
        {
            var stage1 =
                await GetLatestNonSupersededAsync(
                    claimId,
                    ScoringStageConstants.Stage1_FNOL);

            var stage2 =
                await GetLatestNonSupersededAsync(
                    claimId,
                    ScoringStageConstants.Stage2_Survey);

            if (stage1 == null && stage2 == null)
            {
                return null;
            }

            var (compositeScore, compositeBand, lastScoredAt) =
                await ComputeCompositeAsync(
                    stage1,
                    stage2);

            var stages = new List<ScoringStageDto>();

            if (stage1 != null)
            {
                stages.Add(MapStageToDto(stage1));
            }

            if (stage2 != null)
            {
                stages.Add(MapStageToDto(stage2));
            }

            return new InternalClaimScoringDto
            {
                ClaimId = claimId,
                CompositeScore = compositeScore,
                CompositeBand = compositeBand,
                CompositeBandName = GetBandName(compositeBand),
                LastScoredAt = lastScoredAt,
                Stages = stages
            };
        }

        // =========================================================
        // CUSTOMER VIEW
        // =========================================================

        public async Task<CustomerClaimScoreDto?> GetCustomerScoringAsync(
            Guid claimId)
        {
            var internalDto =
                await GetInternalScoringAsync(claimId);

            if (internalDto == null)
            {
                return null;
            }

            return new CustomerClaimScoreDto
            {
                ClaimId = internalDto.ClaimId,
                CompositeScore = internalDto.CompositeScore,
                CompositeBand = internalDto.CompositeBand,
                CompositeBandName = internalDto.CompositeBandName,
                LastScoredAt = internalDto.LastScoredAt
            };
        }

        // =========================================================
        // ROLE-GATED ACCESS
        //
        // Single source of truth for who may see what. Customer =
        // band-only, own claims only. Surveyor = full detail, only
        // if assigned. Approver = full detail, only while the claim
        // has an open Surveyor escalation (mirrors ClaimsController.
        // CanAccessClaimAsync's Approver branch exactly). Admin =
        // full detail, any claim. Anyone else, or wrong ownership =
        // not authorized - callers must return 403, never a filtered
        // 200, so claim existence is never leaked either.
        // =========================================================

        public async Task<ScoringAccessResult> GetScoringForUserAsync(
            Guid claimId,
            Guid userId,
            int roleId)
        {
            var claim =
                await _context.Claims
                    .FirstOrDefaultAsync(
                        x => x.ClaimId == claimId);

            if (claim == null)
            {
                return new ScoringAccessResult
                {
                    ClaimFound = false,
                    Authorized = false
                };
            }

            var authorized = false;

            if (roleId == RoleConstants.AdminId)
            {
                authorized = true;
            }
            else if (roleId == RoleConstants.CustomerId)
            {
                var customer =
                    await _context.Customers
                        .FirstOrDefaultAsync(
                            x => x.CustomerId == claim.CustomerId);

                authorized = customer != null && customer.UserId == userId;
            }
            else if (roleId == RoleConstants.SurveyorId)
            {
                authorized =
                    await _context.SurveyAssignments
                        .AnyAsync(
                            x =>
                                x.ClaimId == claimId &&
                                x.SurveyorId == userId);
            }
            else if (roleId == RoleConstants.ApproverId)
            {
                // Status check rather than decision history: a claim
                // approved via repair-estimate approval
                // (IClaimApprovalService) never creates a ClaimDecision
                // row, so history-based checks would wrongly lock the
                // Approver out once repairs are underway. Not
                // IClaimDecisionService directly - that service depends
                // on IClaimScoringService, so injecting it here would be
                // a circular dependency.
                if (claim.StatusId >= ClaimStatusConstants.RepairInProgress)
                {
                    authorized = true;
                }
                else
                {
                    var latestDecision =
                        await _context.ClaimDecisions
                            .Where(x => x.ClaimId == claimId)
                            .OrderByDescending(x => x.DecisionDate)
                            .FirstOrDefaultAsync();

                    authorized =
                        latestDecision != null &&
                        latestDecision.RoleId == RoleConstants.SurveyorId &&
                        claim.StatusId == ClaimStatusConstants.SurveyCompleted;
                }
            }

            if (!authorized)
            {
                return new ScoringAccessResult
                {
                    ClaimFound = true,
                    Authorized = false
                };
            }

            if (roleId == RoleConstants.CustomerId)
            {
                return new ScoringAccessResult
                {
                    ClaimFound = true,
                    Authorized = true,
                    CustomerView = await GetCustomerScoringAsync(claimId)
                };
            }

            return new ScoringAccessResult
            {
                ClaimFound = true,
                Authorized = true,
                InternalView = await GetInternalScoringAsync(claimId)
            };
        }

        // =========================================================
        // FACT CATALOG
        //
        // Only these keys are ever populated - the Admin rule form
        // restricts ConditionField to this same catalog so a rule
        // can never silently reference an unknown field.
        // =========================================================

        private async Task<Dictionary<string, decimal>> BuildFactsAsync(
            Claim claim,
            int stage)
        {
            var facts = new Dictionary<string, decimal>();

            var intimationDelayDays =
                claim.ReportedDate.HasValue
                    ? (decimal)(claim.ReportedDate.Value - claim.IncidentDate).TotalDays
                    : 0m;

            facts["IntimationDelayDays"] = intimationDelayDays;

            facts["EstimatedLossAmount"] = claim.EstimatedLossAmount ?? 0m;

            facts["IsFraudSuspected"] =
                claim.IsFraudSuspected == true ? 1m : 0m;

            var priorClaimsCount =
                await _context.Claims
                    .CountAsync(
                        x =>
                            x.CustomerId == claim.CustomerId &&
                            x.ClaimId != claim.ClaimId);

            facts["PriorClaimsCount"] = priorClaimsCount;

            var policy =
                await _context.Policies
                    .FirstOrDefaultAsync(
                        x => x.PolicyId == claim.PolicyId);

            facts["PolicyCoverageRatio"] =
                policy != null && policy.CoverageAmount > 0
                    ? (claim.EstimatedLossAmount ?? 0m) / policy.CoverageAmount
                    : 0m;

            // Phase 12 (Raise Claim wizard) facts - additive, optional
            // for an Admin to key ScoringRules off. Populated once
            // Step 2 has run; 0/absent before that (e.g. at Stage 1,
            // which fires at claim creation, before Step 2 exists).
            var intake =
                await _context.ClaimIntakes
                    .FirstOrDefaultAsync(x => x.ClaimId == claim.ClaimId);

            facts["DeathOccurred"] =
                intake?.DeathOccurred == true ? 1m : 0m;

            var rcOcrResult =
                await _context.ClaimRcOcrResults
                    .FirstOrDefaultAsync(x => x.ClaimId == claim.ClaimId);

            facts["RcMismatch"] =
                rcOcrResult?.MatchStatus == RcMatchStatusConstants.Mismatched ? 1m : 0m;

            if (stage == ScoringStageConstants.Stage2_Survey)
            {
                var latestSurveyReport =
                    await _context.SurveyReports
                        .Where(x => x.ClaimId == claim.ClaimId)
                        .OrderByDescending(x => x.CreatedDate)
                        .FirstOrDefaultAsync();

                facts["EstimatedRepairCost"] =
                    latestSurveyReport?.EstimatedRepairCost ?? 0m;

                facts["TotalLoss"] =
                    latestSurveyReport?.TotalLoss == true ? 1m : 0m;

                var documentCount =
                    await _context.ClaimDocuments
                        .CountAsync(x => x.ClaimId == claim.ClaimId);

                facts["DocumentCount"] = documentCount;

                facts["RepairToLossRatio"] =
                    claim.EstimatedLossAmount is > 0 &&
                        latestSurveyReport?.EstimatedRepairCost != null
                        ? latestSurveyReport.EstimatedRepairCost.Value /
                            claim.EstimatedLossAmount.Value
                        : 0m;
            }

            return facts;
        }

        // =========================================================
        // HELPERS
        // =========================================================

        private static bool EvaluateCondition(
            decimal factValue,
            string conditionOperator,
            string thresholdText)
        {
            if (!decimal.TryParse(thresholdText, out var threshold))
            {
                return false;
            }

            return conditionOperator switch
            {
                ">" => factValue > threshold,
                ">=" => factValue >= threshold,
                "<" => factValue < threshold,
                "<=" => factValue <= threshold,
                "=" => factValue == threshold,
                "!=" => factValue != threshold,
                _ => false
            };
        }

        private async Task<int> DetermineBandAsync(
            int scoreValue,
            bool hardFlagTriggered)
        {
            if (hardFlagTriggered)
            {
                return ScoringBandConstants.Red;
            }

            var threshold =
                await _context.ScoringThresholds
                    .FirstOrDefaultAsync(x => x.IsActive);

            // No active threshold configured yet - fail-safe to Red,
            // same conservative-default principle as AuthorityLimits
            // having no row for a role.
            if (threshold == null)
            {
                return ScoringBandConstants.Red;
            }

            if (scoreValue >= threshold.RedMin)
            {
                return ScoringBandConstants.Red;
            }

            if (scoreValue >= threshold.AmberMin)
            {
                return ScoringBandConstants.Amber;
            }

            return ScoringBandConstants.Green;
        }

        private async Task<(int Score, int Band, DateTime? LastScoredAt)> ComputeCompositeAsync(
            ClaimScoringResult? stage1,
            ClaimScoringResult? stage2)
        {
            var score =
                (stage1?.ScoreValue ?? 0) +
                (stage2?.ScoreValue ?? 0);

            var hardFlag =
                (stage1?.HardFlagTriggered ?? false) ||
                (stage2?.HardFlagTriggered ?? false);

            var band =
                await DetermineBandAsync(
                    score,
                    hardFlag);

            DateTime? lastScoredAt = null;

            if (stage1 != null &&
                (lastScoredAt == null || stage1.ScoredAt > lastScoredAt))
            {
                lastScoredAt = stage1.ScoredAt;
            }

            if (stage2 != null &&
                (lastScoredAt == null || stage2.ScoredAt > lastScoredAt))
            {
                lastScoredAt = stage2.ScoredAt;
            }

            return (score, band, lastScoredAt);
        }

        private async Task<ClaimScoringResult?> GetLatestNonSupersededAsync(
            Guid claimId,
            int stage)
        {
            return await _context.ClaimScoringResults
                .Where(
                    x =>
                        x.ClaimId == claimId &&
                        x.Stage == stage &&
                        x.SupersededBy == null)
                .OrderByDescending(x => x.ScoredAt)
                .FirstOrDefaultAsync();
        }

        private static ScoringStageDto MapStageToDto(
            ClaimScoringResult result)
        {
            return new ScoringStageDto
            {
                Stage = result.Stage,
                StageName = GetStageName(result.Stage),
                ScoreValue = result.ScoreValue,
                HardFlagTriggered = result.HardFlagTriggered,
                Band = result.Band,
                BandName = GetBandName(result.Band),
                TriggeredRuleIds =
                    JsonSerializer.Deserialize<List<string>>(
                        result.TriggeredRuleIds) ?? new(),
                ReasonText = result.ReasonText,
                RuleSetVersion = result.RuleSetVersion,
                ScoredAt = result.ScoredAt
            };
        }

        private static string GetStageName(
            int stage)
        {
            return stage switch
            {
                ScoringStageConstants.Stage1_FNOL => "Stage1_FNOL",
                ScoringStageConstants.Stage2_Survey => "Stage2_Survey",
                _ => "Unknown"
            };
        }

        private static string GetBandName(
            int band)
        {
            return band switch
            {
                ScoringBandConstants.Green => "Green",
                ScoringBandConstants.Amber => "Amber",
                ScoringBandConstants.Red => "Red",
                _ => "Unknown"
            };
        }
    }
}
