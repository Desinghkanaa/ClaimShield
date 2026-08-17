using ClaimShield.Api.Data.Context;
using ClaimShield.Api.Interfaces.Services;
using ClaimShield.Api.Models.DTOs.ClaimScoring;
using ClaimShield.Api.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace ClaimShield.Api.Services
{
    // =================================================================
    // Single "default" threshold set for this POC scope (matches the
    // plan's ThresholdSet PK design). No seed data - starts empty, so
    // ClaimScoringService.DetermineBandAsync fails safe to Red until
    // an Admin configures this via the UI.
    // =================================================================

    public class ScoringThresholdService : IScoringThresholdService
    {
        private const string DefaultThresholdSet = "default";

        private readonly ClaimShieldDbContext _context;
        private readonly IAuditLogService _auditLogService;

        public ScoringThresholdService(
            ClaimShieldDbContext context,
            IAuditLogService auditLogService)
        {
            _context = context;
            _auditLogService = auditLogService;
        }

        public async Task<ScoringThresholdResponseDto?> GetActiveAsync()
        {
            var threshold =
                await _context.ScoringThresholds
                    .FirstOrDefaultAsync(
                        x => x.IsActive);

            return threshold == null
                ? null
                : MapToDto(threshold);
        }

        public async Task<ScoringThresholdResponseDto> UpsertAsync(
            Guid updatedBy,
            UpdateScoringThresholdRequest request)
        {
            var threshold =
                await _context.ScoringThresholds
                    .FirstOrDefaultAsync(
                        x => x.ThresholdSet == DefaultThresholdSet);

            var before =
                threshold == null
                    ? null
                    : MapToDto(threshold);

            if (threshold == null)
            {
                threshold = new ScoringThreshold
                {
                    ThresholdSet = DefaultThresholdSet
                };

                _context.ScoringThresholds.Add(threshold);
            }

            threshold.AmberMin = request.AmberMin;

            threshold.RedMin = request.RedMin;

            threshold.IsActive = true;

            await _context.SaveChangesAsync();

            await _auditLogService.LogAsync(
                updatedBy,
                "ScoringThreshold.Updated",
                "ScoringThreshold",
                Guid.Empty,
                before,
                MapToDto(threshold));

            return MapToDto(threshold);
        }

        private static ScoringThresholdResponseDto MapToDto(
            ScoringThreshold threshold)
        {
            return new ScoringThresholdResponseDto
            {
                ThresholdSet = threshold.ThresholdSet,
                AmberMin = threshold.AmberMin,
                RedMin = threshold.RedMin,
                IsActive = threshold.IsActive
            };
        }
    }
}
