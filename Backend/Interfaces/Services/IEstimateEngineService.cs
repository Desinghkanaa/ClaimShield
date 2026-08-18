using ClaimShield.Api.Models.DTOs.InstantClaim;

namespace ClaimShield.Api.Interfaces.Services
{
    public interface IEstimateEngineService
    {
        // Throws InvalidOperationException (with a clear, user-facing
        // message) if a required rate card or parts price row is
        // missing/inactive for a selected part - never fabricates a
        // number.
        Task<ClaimEstimateResultDto> GenerateAsync(
            Guid claimId);
    }
}
