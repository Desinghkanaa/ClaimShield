using ClaimShield.Api.Models.DTOs.SurveyReports;

namespace ClaimShield.Api.Interfaces.Services
{
    public interface ISurveyReportService
    {
        Task<IEnumerable<SurveyReportResponseDto>> GetAllAsync();

        Task<SurveyReportResponseDto?> GetByIdAsync(
            Guid surveyReportId);

        Task<IEnumerable<SurveyReportResponseDto>> GetByClaimAsync(
            Guid claimId);

        Task<IEnumerable<SurveyReportResponseDto>> GetByAssignmentAsync(
            Guid surveyAssignmentId);

        Task<IEnumerable<SurveyReportResponseDto>> GetBySurveyorAsync(
            Guid surveyorId);

        Task<SurveyReportResponseDto> CreateAsync(
            CreateSurveyReportRequest request);

        Task<bool> UpdateAsync(
            UpdateSurveyReportRequest request);

        Task<bool> DeleteAsync(Guid surveyReportId);

        // ---- Phase 13 - Surveyor Survey & Assessment screen ----

        Task<SurveyAssessmentResponseDto?> GetAssessmentByClaimAsync(
            Guid claimId);

        Task<SurveyAssessmentResponseDto> SaveDraftAsync(
            Guid surveyorId,
            SaveSurveyAssessmentRequest request);

        Task<SurveyAssessmentResponseDto> CompleteAssessmentAsync(
            Guid surveyorId,
            CompleteSurveyAssessmentRequest request);
    }
}