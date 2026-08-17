using ClaimShield.Api.Models.DTOs.SurveyAssignments;

namespace ClaimShield.Api.Interfaces.Services
{
    public interface ISurveyAssignmentService
    {
        Task<IEnumerable<SurveyAssignmentResponseDto>> GetAllAsync();

        Task<SurveyAssignmentResponseDto?> GetByIdAsync(
            Guid surveyAssignmentId);

        Task<IEnumerable<SurveyAssignmentResponseDto>> GetByClaimAsync(
            Guid claimId);

        Task<IEnumerable<SurveyAssignmentResponseDto>> GetBySurveyorAsync(
            Guid surveyorId);

        Task<SurveyAssignmentResponseDto> CreateAsync(
            CreateSurveyAssignmentRequest request);

        Task<bool> UpdateAsync(
            UpdateSurveyAssignmentRequest request);

        Task<bool> DeleteAsync(Guid surveyAssignmentId);
    }
}