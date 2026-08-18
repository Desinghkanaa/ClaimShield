using ClaimShield.Api.Data.Context;
using ClaimShield.Api.Interfaces.Services;
using ClaimShield.Api.Models.DTOs.InstantClaim;
using ClaimShield.Api.Models.Entities;

using Microsoft.EntityFrameworkCore;

namespace ClaimShield.Api.Services
{
    // Admin CRUD over the Estimate Engine's config - mirrors
    // ScoringRuleService/ScoringThresholdService's pattern exactly
    // (auto-generated ids, versioning on edit, audit logging, no seed
    // data). Combined into one service/controller for the three
    // related config concerns rather than three separate files, given
    // the scope of this phase.
    public class InstantClaimConfigService : IInstantClaimConfigService
    {
        private const string DefaultEligibilitySet = "default";

        private readonly ClaimShieldDbContext _context;
        private readonly IAuditLogService _auditLogService;

        public InstantClaimConfigService(
            ClaimShieldDbContext context,
            IAuditLogService auditLogService)
        {
            _context = context;
            _auditLogService = auditLogService;
        }

        // =========================================================
        // RATE CARDS
        // =========================================================

        public async Task<IEnumerable<InstantClaimRateCardResponseDto>> GetRateCardsAsync()
        {
            var rows =
                await _context.InstantClaimRateCards
                    .OrderBy(x => x.RateCardId)
                    .ToListAsync();

            return rows.Select(MapRateCard);
        }

        public async Task<InstantClaimRateCardResponseDto> CreateRateCardAsync(
            Guid createdBy,
            CreateInstantClaimRateCardRequest request)
        {
            var id = await GenerateNextIdAsync("RC", _context.InstantClaimRateCards.Select(x => x.RateCardId));

            var rateCard = new InstantClaimRateCard
            {
                RateCardId = id,
                PartType = request.PartType,
                RemoveRefitCharge = request.RemoveRefitCharge,
                DentingCharge = request.DentingCharge,
                PaintingCharge = request.PaintingCharge,
                SalvagePercent = request.SalvagePercent,
                IsActive = true,
                Version = 1,
                EffectiveFrom = DateTime.UtcNow
            };

            _context.InstantClaimRateCards.Add(rateCard);

            await _context.SaveChangesAsync();

            await _auditLogService.LogAsync(
                createdBy,
                "InstantClaimRateCard.Created",
                "InstantClaimRateCard",
                Guid.Empty,
                null,
                MapRateCard(rateCard));

            return MapRateCard(rateCard);
        }

        public async Task<InstantClaimRateCardResponseDto?> UpdateRateCardAsync(
            string rateCardId,
            Guid updatedBy,
            UpdateInstantClaimRateCardRequest request)
        {
            var rateCard =
                await _context.InstantClaimRateCards
                    .FirstOrDefaultAsync(x => x.RateCardId == rateCardId);

            if (rateCard == null)
            {
                return null;
            }

            var before = MapRateCard(rateCard);

            rateCard.RemoveRefitCharge = request.RemoveRefitCharge;
            rateCard.DentingCharge = request.DentingCharge;
            rateCard.PaintingCharge = request.PaintingCharge;
            rateCard.SalvagePercent = request.SalvagePercent;
            rateCard.Version += 1;
            rateCard.EffectiveFrom = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            await _auditLogService.LogAsync(
                updatedBy,
                "InstantClaimRateCard.Updated",
                "InstantClaimRateCard",
                Guid.Empty,
                before,
                MapRateCard(rateCard));

            return MapRateCard(rateCard);
        }

        public async Task<InstantClaimRateCardResponseDto?> ToggleRateCardActiveAsync(
            string rateCardId,
            Guid updatedBy)
        {
            var rateCard =
                await _context.InstantClaimRateCards
                    .FirstOrDefaultAsync(x => x.RateCardId == rateCardId);

            if (rateCard == null)
            {
                return null;
            }

            var before = MapRateCard(rateCard);

            rateCard.IsActive = !rateCard.IsActive;

            await _context.SaveChangesAsync();

            await _auditLogService.LogAsync(
                updatedBy,
                rateCard.IsActive
                    ? "InstantClaimRateCard.Activated"
                    : "InstantClaimRateCard.Deactivated",
                "InstantClaimRateCard",
                Guid.Empty,
                before,
                MapRateCard(rateCard));

            return MapRateCard(rateCard);
        }

        // =========================================================
        // PARTS PRICING
        // =========================================================

        public async Task<IEnumerable<InstantClaimPartsPricingResponseDto>> GetPartsPricingAsync()
        {
            var rows =
                await _context.InstantClaimPartsPricing
                    .OrderBy(x => x.PartsPricingId)
                    .ToListAsync();

            return rows.Select(MapPartsPricing);
        }

        public async Task<InstantClaimPartsPricingResponseDto> CreatePartsPricingAsync(
            Guid createdBy,
            CreateInstantClaimPartsPricingRequest request)
        {
            var id = await GenerateNextIdAsync("PP", _context.InstantClaimPartsPricing.Select(x => x.PartsPricingId));

            var pricing = new InstantClaimPartsPricing
            {
                PartsPricingId = id,
                PartType = request.PartType,
                MakeId = request.MakeId,
                ModelId = request.ModelId,
                PartsAmount = request.PartsAmount,
                IsActive = true,
                Version = 1,
                EffectiveFrom = DateTime.UtcNow
            };

            _context.InstantClaimPartsPricing.Add(pricing);

            await _context.SaveChangesAsync();

            await _auditLogService.LogAsync(
                createdBy,
                "InstantClaimPartsPricing.Created",
                "InstantClaimPartsPricing",
                Guid.Empty,
                null,
                MapPartsPricing(pricing));

            return MapPartsPricing(pricing);
        }

        public async Task<InstantClaimPartsPricingResponseDto?> UpdatePartsPricingAsync(
            string partsPricingId,
            Guid updatedBy,
            UpdateInstantClaimPartsPricingRequest request)
        {
            var pricing =
                await _context.InstantClaimPartsPricing
                    .FirstOrDefaultAsync(x => x.PartsPricingId == partsPricingId);

            if (pricing == null)
            {
                return null;
            }

            var before = MapPartsPricing(pricing);

            pricing.MakeId = request.MakeId;
            pricing.ModelId = request.ModelId;
            pricing.PartsAmount = request.PartsAmount;
            pricing.Version += 1;
            pricing.EffectiveFrom = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            await _auditLogService.LogAsync(
                updatedBy,
                "InstantClaimPartsPricing.Updated",
                "InstantClaimPartsPricing",
                Guid.Empty,
                before,
                MapPartsPricing(pricing));

            return MapPartsPricing(pricing);
        }

        public async Task<InstantClaimPartsPricingResponseDto?> TogglePartsPricingActiveAsync(
            string partsPricingId,
            Guid updatedBy)
        {
            var pricing =
                await _context.InstantClaimPartsPricing
                    .FirstOrDefaultAsync(x => x.PartsPricingId == partsPricingId);

            if (pricing == null)
            {
                return null;
            }

            var before = MapPartsPricing(pricing);

            pricing.IsActive = !pricing.IsActive;

            await _context.SaveChangesAsync();

            await _auditLogService.LogAsync(
                updatedBy,
                pricing.IsActive
                    ? "InstantClaimPartsPricing.Activated"
                    : "InstantClaimPartsPricing.Deactivated",
                "InstantClaimPartsPricing",
                Guid.Empty,
                before,
                MapPartsPricing(pricing));

            return MapPartsPricing(pricing);
        }

        // =========================================================
        // ELIGIBILITY
        // =========================================================

        public async Task<InstantClaimEligibilityResponseDto?> GetEligibilityAsync()
        {
            var row =
                await _context.InstantClaimEligibilities
                    .FirstOrDefaultAsync(x => x.IsActive);

            return row == null ? null : MapEligibility(row);
        }

        public async Task<InstantClaimEligibilityResponseDto> UpsertEligibilityAsync(
            Guid updatedBy,
            UpdateInstantClaimEligibilityRequest request)
        {
            var row =
                await _context.InstantClaimEligibilities
                    .FirstOrDefaultAsync(x => x.EligibilitySet == DefaultEligibilitySet);

            var before = row == null ? null : MapEligibility(row);

            if (row == null)
            {
                row = new InstantClaimEligibility
                {
                    EligibilitySet = DefaultEligibilitySet
                };

                _context.InstantClaimEligibilities.Add(row);
            }

            row.MinEligibleBand = request.MinEligibleBand;
            row.IsActive = true;

            await _context.SaveChangesAsync();

            await _auditLogService.LogAsync(
                updatedBy,
                "InstantClaimEligibility.Updated",
                "InstantClaimEligibility",
                Guid.Empty,
                before,
                MapEligibility(row));

            return MapEligibility(row);
        }

        // =========================================================
        // HELPERS
        // =========================================================

        private static async Task<string> GenerateNextIdAsync(
            string prefix,
            IQueryable<string> existingIdsQuery)
        {
            var existingIds = await existingIdsQuery.ToListAsync();

            var maxSeq = 0;

            foreach (var id in existingIds)
            {
                if (!id.StartsWith(prefix + "-"))
                {
                    continue;
                }

                var suffix = id[(prefix.Length + 1)..];

                if (int.TryParse(suffix, out var seq) && seq > maxSeq)
                {
                    maxSeq = seq;
                }
            }

            return $"{prefix}-{(maxSeq + 1):D3}";
        }

        private static InstantClaimRateCardResponseDto MapRateCard(
            InstantClaimRateCard rateCard)
        {
            return new InstantClaimRateCardResponseDto
            {
                RateCardId = rateCard.RateCardId,
                PartType = rateCard.PartType,
                RemoveRefitCharge = rateCard.RemoveRefitCharge,
                DentingCharge = rateCard.DentingCharge,
                PaintingCharge = rateCard.PaintingCharge,
                SalvagePercent = rateCard.SalvagePercent,
                IsActive = rateCard.IsActive,
                Version = rateCard.Version,
                EffectiveFrom = rateCard.EffectiveFrom
            };
        }

        private static InstantClaimPartsPricingResponseDto MapPartsPricing(
            InstantClaimPartsPricing pricing)
        {
            return new InstantClaimPartsPricingResponseDto
            {
                PartsPricingId = pricing.PartsPricingId,
                PartType = pricing.PartType,
                MakeId = pricing.MakeId,
                ModelId = pricing.ModelId,
                PartsAmount = pricing.PartsAmount,
                IsActive = pricing.IsActive,
                Version = pricing.Version,
                EffectiveFrom = pricing.EffectiveFrom
            };
        }

        private static InstantClaimEligibilityResponseDto MapEligibility(
            InstantClaimEligibility eligibility)
        {
            return new InstantClaimEligibilityResponseDto
            {
                EligibilitySet = eligibility.EligibilitySet,
                MinEligibleBand = eligibility.MinEligibleBand,
                IsActive = eligibility.IsActive
            };
        }
    }
}
