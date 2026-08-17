using System.Text.Json;
using ClaimShield.Api.Constants;
using ClaimShield.Api.Data.Context;
using ClaimShield.Api.Interfaces.Services;
using ClaimShield.Api.Models.DTOs.ClaimDecisions;
using ClaimShield.Api.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace ClaimShield.Api.Services
{
    // =============================================================
    // Maker-checker workflow.
    //
    // Surveyor (maker) records Approve/Review/Deny. Approve
    // auto-finalizes only if the claim amount is within the
    // Surveyor's AuthorityLimits.MaxApprovalAmount AND the current
    // composite scoring band (Phase 9's two-stage rule engine) is
    // Green (missing AuthorityLimits row, missing scoring result, or
    // any Amber/Red band = always escalate). Review/Deny always
    // escalate. There is no dedicated "pending approval" claim
    // status - while escalated, Claim.StatusId simply stays at
    // SurveyCompleted, and the open escalation is derived from
    // "latest decision is Surveyor-authored AND status is still
    // SurveyCompleted."
    //
    // Scoring itself is NOT triggered here - Stage 2 already ran
    // when the survey report was submitted
    // (SurveyReportService.CreateAsync/UpdateAsync). This just reads
    // the current composite result.
    // =============================================================

    public class ClaimDecisionService : IClaimDecisionService
    {
        private readonly ClaimShieldDbContext _context;
        private readonly IClaimScoringService _claimScoringService;
        private readonly IAuditLogService _auditLogService;

        public ClaimDecisionService(
            ClaimShieldDbContext context,
            IClaimScoringService claimScoringService,
            IAuditLogService auditLogService)
        {
            _context = context;
            _claimScoringService = claimScoringService;
            _auditLogService = auditLogService;
        }

        // =========================================================
        // SURVEYOR DECISION
        // =========================================================

        public async Task<ClaimDecisionResult> RecordSurveyorDecisionAsync(
            Guid claimId,
            Guid surveyorId,
            SurveyorDecisionRequest request)
        {
            var claim =
                await _context.Claims
                    .FirstOrDefaultAsync(
                        x => x.ClaimId == claimId);

            if (claim == null)
            {
                return Fail("Claim not found.");
            }

            var isOwner =
                await _context.SurveyAssignments
                    .AnyAsync(
                        x =>
                            x.ClaimId == claimId &&
                            x.SurveyorId == surveyorId);

            if (!isOwner)
            {
                return Fail(
                    "You are not assigned to survey this claim.");
            }

            if (claim.StatusId != ClaimStatusConstants.SurveyCompleted)
            {
                return Fail(
                    "A decision can only be recorded once the survey report has been completed.");
            }

            var alreadyDecided =
                await _context.ClaimDecisions
                    .AnyAsync(
                        x => x.ClaimId == claimId);

            if (alreadyDecided)
            {
                return Fail(
                    "A decision has already been recorded for this claim.");
            }

            var composite =
                await _claimScoringService.GetInternalScoringAsync(
                    claimId);

            var latestSurveyReport =
                await _context.SurveyReports
                    .Where(x => x.ClaimId == claimId)
                    .OrderByDescending(x => x.CreatedDate)
                    .FirstOrDefaultAsync();

            var claimAmount =
                latestSurveyReport?.EstimatedRepairCost
                ?? claim.EstimatedLossAmount
                ?? 0m;

            var authorityLimit =
                await _context.AuthorityLimits
                    .FirstOrDefaultAsync(
                        x => x.RoleId == RoleConstants.SurveyorId);

            var withinAuthority =
                request.Decision == ClaimDecisionConstants.Approve &&
                authorityLimit != null &&
                (authorityLimit.MaxApprovalAmount == null ||
                    claimAmount <= authorityLimit.MaxApprovalAmount.Value) &&
                composite != null &&
                composite.CompositeBand == ScoringBandConstants.Green;

            var escalated = !withinAuthority;

            var beforeState =
                new
                {
                    claim.StatusId,
                    claim.ReserveAmount
                };

            if (!escalated)
            {
                claim.StatusId = ClaimStatusConstants.RepairAssigned;
                claim.ReserveAmount = claimAmount;
                claim.UpdatedDate = DateTime.UtcNow;
            }

            var decision = new ClaimDecision
            {
                ClaimDecisionId = Guid.NewGuid(),

                ClaimId = claimId,

                DecidedBy = surveyorId,

                RoleId = RoleConstants.SurveyorId,

                Decision = request.Decision,

                Reasoning = request.Reasoning,

                AiScoresSnapshot =
                    JsonSerializer.Serialize(
                        new
                        {
                            composite?.CompositeScore,
                            composite?.CompositeBandName,
                            Stages = composite?.Stages,
                            ClaimAmount = claimAmount
                        }),

                DecisionDate = DateTime.UtcNow
            };

            _context.ClaimDecisions.Add(decision);

            await _context.SaveChangesAsync();

            var afterState =
                new
                {
                    claim.StatusId,
                    claim.ReserveAmount
                };

            await _auditLogService.LogAsync(
                surveyorId,
                $"SurveyorDecision.{GetDecisionName(request.Decision)}." +
                    (escalated ? "Escalated" : "AutoFinalized"),
                "Claim",
                claimId,
                beforeState,
                afterState);

            return new ClaimDecisionResult
            {
                Success = true,
                Escalated = escalated,
                UpdatedClaimStatusId = claim.StatusId,
                Decision = await MapToDtoAsync(decision, escalated)
            };
        }

        // =========================================================
        // APPROVER DECISION
        // =========================================================

        public async Task<ClaimDecisionResult> RecordApproverDecisionAsync(
            Guid claimId,
            Guid approverId,
            ApproverDecisionRequest request)
        {
            if (request.Decision != ClaimDecisionConstants.Approve &&
                request.Decision != ClaimDecisionConstants.Deny)
            {
                return Fail(
                    "An Approver decision must be either Approve or Deny.");
            }

            var claim =
                await _context.Claims
                    .FirstOrDefaultAsync(
                        x => x.ClaimId == claimId);

            if (claim == null)
            {
                return Fail("Claim not found.");
            }

            var latestDecision =
                await _context.ClaimDecisions
                    .Where(x => x.ClaimId == claimId)
                    .OrderByDescending(x => x.DecisionDate)
                    .FirstOrDefaultAsync();

            var isOpenEscalation =
                latestDecision != null &&
                latestDecision.RoleId == RoleConstants.SurveyorId &&
                claim.StatusId == ClaimStatusConstants.SurveyCompleted;

            if (!isOpenEscalation)
            {
                return Fail(
                    "This claim does not have a pending Surveyor decision awaiting Approver review.");
            }

            var beforeState =
                new
                {
                    claim.StatusId,
                    claim.ApprovedAmount
                };

            if (request.Decision == ClaimDecisionConstants.Approve)
            {
                var latestSurveyReport =
                    await _context.SurveyReports
                        .Where(x => x.ClaimId == claimId)
                        .OrderByDescending(x => x.CreatedDate)
                        .FirstOrDefaultAsync();

                var claimAmount =
                    latestSurveyReport?.EstimatedRepairCost
                    ?? claim.EstimatedLossAmount
                    ?? 0m;

                claim.StatusId = ClaimStatusConstants.RepairAssigned;
                claim.ReserveAmount = claimAmount;
            }
            else
            {
                claim.StatusId = ClaimStatusConstants.Rejected;
                claim.DecisionRemarks = request.Reasoning;
            }

            claim.UpdatedDate = DateTime.UtcNow;

            var decision = new ClaimDecision
            {
                ClaimDecisionId = Guid.NewGuid(),

                ClaimId = claimId,

                DecidedBy = approverId,

                RoleId = RoleConstants.ApproverId,

                Decision = request.Decision,

                Reasoning = request.Reasoning,

                AiScoresSnapshot = latestDecision!.AiScoresSnapshot,

                DecisionDate = DateTime.UtcNow
            };

            _context.ClaimDecisions.Add(decision);

            await _context.SaveChangesAsync();

            var afterState =
                new
                {
                    claim.StatusId,
                    claim.ApprovedAmount
                };

            await _auditLogService.LogAsync(
                approverId,
                $"ApproverDecision.{GetDecisionName(request.Decision)}.Finalized",
                "Claim",
                claimId,
                beforeState,
                afterState);

            return new ClaimDecisionResult
            {
                Success = true,
                Escalated = false,
                UpdatedClaimStatusId = claim.StatusId,
                Decision = await MapToDtoAsync(decision, false)
            };
        }

        // =========================================================
        // LATEST / HISTORY
        // =========================================================

        public async Task<ClaimDecisionResponseDto?> GetLatestDecisionAsync(
            Guid claimId)
        {
            var claim =
                await _context.Claims
                    .FirstOrDefaultAsync(
                        x => x.ClaimId == claimId);

            var latest =
                await _context.ClaimDecisions
                    .Where(x => x.ClaimId == claimId)
                    .OrderByDescending(x => x.DecisionDate)
                    .FirstOrDefaultAsync();

            if (latest == null)
            {
                return null;
            }

            var isOpen =
                claim != null &&
                latest.RoleId == RoleConstants.SurveyorId &&
                claim.StatusId == ClaimStatusConstants.SurveyCompleted;

            return await MapToDtoAsync(latest, isOpen);
        }

        public async Task<IEnumerable<ClaimDecisionResponseDto>> GetHistoryAsync(
            Guid claimId)
        {
            var claim =
                await _context.Claims
                    .FirstOrDefaultAsync(
                        x => x.ClaimId == claimId);

            var decisions =
                await _context.ClaimDecisions
                    .Where(x => x.ClaimId == claimId)
                    .OrderByDescending(x => x.DecisionDate)
                    .ToListAsync();

            var result = new List<ClaimDecisionResponseDto>();

            for (var i = 0; i < decisions.Count; i++)
            {
                var decision = decisions[i];

                var isOpen =
                    i == 0 &&
                    claim != null &&
                    decision.RoleId == RoleConstants.SurveyorId &&
                    claim.StatusId == ClaimStatusConstants.SurveyCompleted;

                result.Add(
                    await MapToDtoAsync(decision, isOpen));
            }

            return result;
        }

        // =========================================================
        // MY QUEUE
        // =========================================================

        public async Task<IEnumerable<ClaimQueueItemResponseDto>> GetMyQueueAsync(
            Guid userId,
            int roleId)
        {
            var result = new List<ClaimQueueItemResponseDto>();

            var candidateClaims =
                await _context.Claims
                    .Where(x => x.StatusId == ClaimStatusConstants.SurveyCompleted)
                    .ToListAsync();

            // =====================================================
            // AWAITING SURVEY (Phase 13 - assigned but the Survey &
            // Assessment screen hasn't been completed yet; the moment it
            // is, Claim.StatusId moves to SurveyCompleted and the claim
            // falls into the "AWAITING SURVEYOR DECISION" bucket below,
            // so these two are mutually exclusive by construction).
            // =====================================================

            if (roleId == RoleConstants.SurveyorId ||
                roleId == RoleConstants.AdminId)
            {
                var awaitingSurveyClaims =
                    await _context.Claims
                        .Where(x => x.StatusId == ClaimStatusConstants.SurveyAssigned)
                        .ToListAsync();

                List<Guid>? mySurveyAssignmentClaimIds = null;

                if (roleId == RoleConstants.SurveyorId)
                {
                    mySurveyAssignmentClaimIds =
                        await _context.SurveyAssignments
                            .Where(x => x.SurveyorId == userId)
                            .Select(x => x.ClaimId)
                            .Distinct()
                            .ToListAsync();
                }

                foreach (var claim in awaitingSurveyClaims)
                {
                    if (mySurveyAssignmentClaimIds != null &&
                        !mySurveyAssignmentClaimIds.Contains(claim.ClaimId))
                    {
                        continue;
                    }

                    result.Add(
                        new ClaimQueueItemResponseDto
                        {
                            ClaimId = claim.ClaimId,
                            ClaimNumber = claim.ClaimNumber,
                            StatusId = claim.StatusId ?? 0,
                            EstimatedLossAmount = claim.EstimatedLossAmount,
                            QueueReason = "AwaitingSurvey"
                        });
                }
            }

            // =====================================================
            // AWAITING SURVEYOR DECISION
            // =====================================================

            if (roleId == RoleConstants.SurveyorId ||
                roleId == RoleConstants.AdminId)
            {
                List<Guid>? assignedClaimIds = null;

                if (roleId == RoleConstants.SurveyorId)
                {
                    assignedClaimIds =
                        await _context.SurveyAssignments
                            .Where(x => x.SurveyorId == userId)
                            .Select(x => x.ClaimId)
                            .Distinct()
                            .ToListAsync();
                }

                var decidedClaimIds =
                    await _context.ClaimDecisions
                        .Select(x => x.ClaimId)
                        .Distinct()
                        .ToListAsync();

                foreach (var claim in candidateClaims)
                {
                    if (decidedClaimIds.Contains(claim.ClaimId))
                    {
                        continue;
                    }

                    if (assignedClaimIds != null &&
                        !assignedClaimIds.Contains(claim.ClaimId))
                    {
                        continue;
                    }

                    result.Add(
                        new ClaimQueueItemResponseDto
                        {
                            ClaimId = claim.ClaimId,
                            ClaimNumber = claim.ClaimNumber,
                            StatusId = claim.StatusId ?? 0,
                            EstimatedLossAmount = claim.EstimatedLossAmount,
                            QueueReason = "AwaitingSurveyorDecision"
                        });
                }
            }

            // =====================================================
            // AWAITING APPROVER DECISION
            // =====================================================

            if (roleId == RoleConstants.ApproverId ||
                roleId == RoleConstants.AdminId)
            {
                foreach (var claim in candidateClaims)
                {
                    var latestDecision =
                        await _context.ClaimDecisions
                            .Where(x => x.ClaimId == claim.ClaimId)
                            .OrderByDescending(x => x.DecisionDate)
                            .FirstOrDefaultAsync();

                    if (latestDecision == null ||
                        latestDecision.RoleId != RoleConstants.SurveyorId)
                    {
                        continue;
                    }

                    result.Add(
                        new ClaimQueueItemResponseDto
                        {
                            ClaimId = claim.ClaimId,
                            ClaimNumber = claim.ClaimNumber,
                            StatusId = claim.StatusId ?? 0,
                            EstimatedLossAmount = claim.EstimatedLossAmount,
                            QueueReason = "AwaitingApproverDecision",
                            PendingDecisionId = latestDecision.ClaimDecisionId
                        });
                }
            }

            return result;
        }

        // =========================================================
        // MAPPING / HELPERS
        // =========================================================

        private async Task<ClaimDecisionResponseDto> MapToDtoAsync(
            ClaimDecision decision,
            bool escalated)
        {
            var user =
                await _context.Users
                    .FirstOrDefaultAsync(
                        x => x.UserId == decision.DecidedBy);

            return new ClaimDecisionResponseDto
            {
                ClaimDecisionId = decision.ClaimDecisionId,
                ClaimId = decision.ClaimId,
                DecidedBy = decision.DecidedBy,
                DecidedByName = GetUserDisplayName(user),
                RoleId = decision.RoleId,
                RoleName = GetRoleName(decision.RoleId),
                Decision = decision.Decision,
                DecisionName = GetDecisionName(decision.Decision),
                Reasoning = decision.Reasoning,
                AiScoresSnapshot = decision.AiScoresSnapshot,
                DecisionDate = decision.DecisionDate,
                Escalated = escalated
            };
        }

        private static string GetUserDisplayName(
            User? user)
        {
            if (user == null)
            {
                return "Unknown";
            }

            var firstName = user.FirstName?.Trim();
            var lastName = user.LastName?.Trim();

            if (!string.IsNullOrWhiteSpace(firstName) &&
                !string.IsNullOrWhiteSpace(lastName))
            {
                return $"{firstName} {lastName}";
            }

            if (!string.IsNullOrWhiteSpace(firstName))
            {
                return firstName;
            }

            if (!string.IsNullOrWhiteSpace(lastName))
            {
                return lastName;
            }

            return "Unknown";
        }

        private static string GetRoleName(
            int roleId)
        {
            return roleId switch
            {
                RoleConstants.CustomerId => RoleConstants.Customer,
                RoleConstants.RepairerId => RoleConstants.Repairer,
                RoleConstants.SurveyorId => RoleConstants.Surveyor,
                RoleConstants.ApproverId => RoleConstants.Approver,
                RoleConstants.AdminId => RoleConstants.Admin,
                _ => "Unknown"
            };
        }

        private static string GetDecisionName(
            int decision)
        {
            return decision switch
            {
                ClaimDecisionConstants.Approve => "Approve",
                ClaimDecisionConstants.Review => "Review",
                ClaimDecisionConstants.Deny => "Deny",
                _ => "Unknown"
            };
        }

        private static ClaimDecisionResult Fail(
            string message)
        {
            return new ClaimDecisionResult
            {
                Success = false,
                ErrorMessage = message
            };
        }
    }
}
