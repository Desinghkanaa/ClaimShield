using ClaimShield.Api.Models.Entities;

namespace ClaimShield.Api.Interfaces.Repositories
{
    public interface ISurveyReportRepository
    {
        Task<IEnumerable<SurveyReport>> GetAllAsync();

        Task<SurveyReport?> GetByIdAsync(Guid surveyReportId);

        Task<IEnumerable<SurveyReport>> GetByClaimAsync(Guid claimId);

        Task<IEnumerable<SurveyReport>> GetByAssignmentAsync(
            Guid surveyAssignmentId);

        Task<IEnumerable<SurveyReport>> GetBySurveyorAsync(
            Guid surveyorId);

        Task AddAsync(SurveyReport surveyReport);

        Task UpdateAsync(SurveyReport surveyReport);

        Task DeleteAsync(Guid surveyReportId);
    }
}