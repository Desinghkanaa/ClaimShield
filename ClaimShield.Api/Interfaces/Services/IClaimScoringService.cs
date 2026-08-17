using ClaimShield.Api.Models.DTOs.ClaimScoring;

namespace ClaimShield.Api.Interfaces.Services
{
    public interface IClaimScoringService
    {
        Task<ScoringStageDto> ScoreStageAsync(
            Guid claimId,
            int stage);

        Task<InternalClaimScoringDto?> GetInternalScoringAsync(
            Guid claimId);

        Task<CustomerClaimScoreDto?> GetCustomerScoringAsync(
            Guid claimId);

        Task<ScoringAccessResult> GetScoringForUserAsync(
            Guid claimId,
            Guid userId,
            int roleId);
    }
}
