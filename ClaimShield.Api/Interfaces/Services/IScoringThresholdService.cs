using ClaimShield.Api.Models.DTOs.ClaimScoring;

namespace ClaimShield.Api.Interfaces.Services
{
    public interface IScoringThresholdService
    {
        Task<ScoringThresholdResponseDto?> GetActiveAsync();

        Task<ScoringThresholdResponseDto> UpsertAsync(
            Guid updatedBy,
            UpdateScoringThresholdRequest request);
    }
}
