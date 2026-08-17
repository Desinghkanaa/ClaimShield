using ClaimShield.Api.Data.Context;
using ClaimShield.Api.Interfaces.Services;
using ClaimShield.Api.Models.DTOs.ClaimScoring;
using ClaimShield.Api.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace ClaimShield.Api.Services
{
    // =================================================================
    // Admin CRUD over the rule repository. No seed data is written
    // anywhere in this system - stakeholder-confirmed rule values are
    // entered here, by an Admin, not hardcoded. Every create/edit/
    // toggle increments Version (on edit) and writes an AuditLogs
    // entry, so historical ClaimScoringResult.RuleSetVersion snapshots
    // never silently change meaning.
    // =================================================================

    public class ScoringRuleService : IScoringRuleService
    {
        private readonly ClaimShieldDbContext _context;
        private readonly IAuditLogService _auditLogService;

        public ScoringRuleService(
            ClaimShieldDbContext context,
            IAuditLogService auditLogService)
        {
            _context = context;
            _auditLogService = auditLogService;
        }

        public async Task<IEnumerable<ScoringRuleResponseDto>> GetAllAsync(
            int? stage)
        {
            var query = _context.ScoringRules.AsQueryable();

            if (stage.HasValue)
            {
                query = query.Where(x => x.Stage == stage.Value);
            }

            var rules =
                await query
                    .OrderBy(x => x.RuleId)
                    .ToListAsync();

            return rules.Select(MapToDto);
        }

        public async Task<ScoringRuleResponseDto?> GetByIdAsync(
            string ruleId)
        {
            var rule =
                await _context.ScoringRules
                    .FirstOrDefaultAsync(
                        x => x.RuleId == ruleId);

            return rule == null
                ? null
                : MapToDto(rule);
        }

        public async Task<ScoringRuleResponseDto> CreateAsync(
            Guid createdBy,
            CreateScoringRuleRequest request)
        {
            var ruleId =
                await GenerateNextRuleIdAsync(
                    request.Stage);

            var rule = new ScoringRule
            {
                RuleId = ruleId,

                Stage = request.Stage,

                Category = request.Category,

                ConditionField = request.ConditionField,

                ConditionOperator = request.ConditionOperator,

                ConditionThreshold = request.ConditionThreshold,

                Severity = request.Severity,

                Points = request.Points,

                IsActive = true,

                Version = 1,

                EffectiveFrom = DateTime.UtcNow
            };

            _context.ScoringRules.Add(rule);

            await _context.SaveChangesAsync();

            await _auditLogService.LogAsync(
                createdBy,
                "ScoringRule.Created",
                "ScoringRule",
                Guid.Empty,
                null,
                MapToDto(rule));

            return MapToDto(rule);
        }

        public async Task<ScoringRuleResponseDto?> UpdateAsync(
            string ruleId,
            Guid updatedBy,
            UpdateScoringRuleRequest request)
        {
            var rule =
                await _context.ScoringRules
                    .FirstOrDefaultAsync(
                        x => x.RuleId == ruleId);

            if (rule == null)
            {
                return null;
            }

            var before = MapToDto(rule);

            rule.Category = request.Category;

            rule.ConditionField = request.ConditionField;

            rule.ConditionOperator = request.ConditionOperator;

            rule.ConditionThreshold = request.ConditionThreshold;

            rule.Severity = request.Severity;

            rule.Points = request.Points;

            rule.Version += 1;

            rule.EffectiveFrom = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            await _auditLogService.LogAsync(
                updatedBy,
                "ScoringRule.Updated",
                "ScoringRule",
                Guid.Empty,
                before,
                MapToDto(rule));

            return MapToDto(rule);
        }

        public async Task<ScoringRuleResponseDto?> ToggleActiveAsync(
            string ruleId,
            Guid updatedBy)
        {
            var rule =
                await _context.ScoringRules
                    .FirstOrDefaultAsync(
                        x => x.RuleId == ruleId);

            if (rule == null)
            {
                return null;
            }

            var before = MapToDto(rule);

            rule.IsActive = !rule.IsActive;

            await _context.SaveChangesAsync();

            await _auditLogService.LogAsync(
                updatedBy,
                rule.IsActive
                    ? "ScoringRule.Activated"
                    : "ScoringRule.Deactivated",
                "ScoringRule",
                Guid.Empty,
                before,
                MapToDto(rule));

            return MapToDto(rule);
        }

        // =========================================================
        // RuleId format: "S{stage}-R{seq:D2}" - human-readable and
        // stage-scoped, e.g. "S1-R01". Sequence is derived from the
        // max existing suffix for the stage, not a row count, so a
        // deactivated/never-deleted rule can't cause a collision.
        // =========================================================

        private async Task<string> GenerateNextRuleIdAsync(
            int stage)
        {
            var prefix = $"S{stage}-R";

            var existingIds =
                await _context.ScoringRules
                    .Where(x => x.Stage == stage)
                    .Select(x => x.RuleId)
                    .ToListAsync();

            var maxSeq = 0;

            foreach (var id in existingIds)
            {
                if (!id.StartsWith(prefix))
                {
                    continue;
                }

                var suffix = id[prefix.Length..];

                if (int.TryParse(suffix, out var seq) && seq > maxSeq)
                {
                    maxSeq = seq;
                }
            }

            return $"{prefix}{(maxSeq + 1):D2}";
        }

        private static ScoringRuleResponseDto MapToDto(
            ScoringRule rule)
        {
            return new ScoringRuleResponseDto
            {
                RuleId = rule.RuleId,
                Stage = rule.Stage,
                Category = rule.Category,
                ConditionField = rule.ConditionField,
                ConditionOperator = rule.ConditionOperator,
                ConditionThreshold = rule.ConditionThreshold,
                Severity = rule.Severity,
                Points = rule.Points,
                IsActive = rule.IsActive,
                Version = rule.Version,
                EffectiveFrom = rule.EffectiveFrom
            };
        }
    }
}
