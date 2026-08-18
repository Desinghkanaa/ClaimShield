using ClaimShield.Api.Models.DTOs.ClaimScoring;

namespace ClaimShield.Api.Interfaces.Services
{
    public interface IScoringRuleService
    {
        Task<IEnumerable<ScoringRuleResponseDto>> GetAllAsync(
            int? stage);

        Task<ScoringRuleResponseDto?> GetByIdAsync(
            string ruleId);

        Task<ScoringRuleResponseDto> CreateAsync(
            Guid createdBy,
            CreateScoringRuleRequest request);

        Task<ScoringRuleResponseDto?> UpdateAsync(
            string ruleId,
            Guid updatedBy,
            UpdateScoringRuleRequest request);

        Task<ScoringRuleResponseDto?> ToggleActiveAsync(
            string ruleId,
            Guid updatedBy);
    }
}
