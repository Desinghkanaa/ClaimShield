using ClaimShield.Api.Models.DTOs.InstantClaim;

namespace ClaimShield.Api.Interfaces.Services
{
    public interface IInstantClaimConfigService
    {
        Task<IEnumerable<InstantClaimRateCardResponseDto>> GetRateCardsAsync();

        Task<InstantClaimRateCardResponseDto> CreateRateCardAsync(
            Guid createdBy,
            CreateInstantClaimRateCardRequest request);

        Task<InstantClaimRateCardResponseDto?> UpdateRateCardAsync(
            string rateCardId,
            Guid updatedBy,
            UpdateInstantClaimRateCardRequest request);

        Task<InstantClaimRateCardResponseDto?> ToggleRateCardActiveAsync(
            string rateCardId,
            Guid updatedBy);

        Task<IEnumerable<InstantClaimPartsPricingResponseDto>> GetPartsPricingAsync();

        Task<InstantClaimPartsPricingResponseDto> CreatePartsPricingAsync(
            Guid createdBy,
            CreateInstantClaimPartsPricingRequest request);

        Task<InstantClaimPartsPricingResponseDto?> UpdatePartsPricingAsync(
            string partsPricingId,
            Guid updatedBy,
            UpdateInstantClaimPartsPricingRequest request);

        Task<InstantClaimPartsPricingResponseDto?> TogglePartsPricingActiveAsync(
            string partsPricingId,
            Guid updatedBy);

        Task<InstantClaimEligibilityResponseDto?> GetEligibilityAsync();

        Task<InstantClaimEligibilityResponseDto> UpsertEligibilityAsync(
            Guid updatedBy,
            UpdateInstantClaimEligibilityRequest request);
    }
}
