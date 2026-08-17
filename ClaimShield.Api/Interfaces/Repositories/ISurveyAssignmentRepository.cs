using ClaimShield.Api.Models.Entities;

namespace ClaimShield.Api.Interfaces.Repositories
{
    public interface ISurveyAssignmentRepository
    {
        Task<IEnumerable<SurveyAssignment>> GetAllAsync();

        Task<SurveyAssignment?> GetByIdAsync(Guid surveyAssignmentId);

        Task<IEnumerable<SurveyAssignment>> GetByClaimAsync(Guid claimId);

        Task<IEnumerable<SurveyAssignment>> GetBySurveyorAsync(Guid surveyorId);

        Task AddAsync(SurveyAssignment surveyAssignment);

        Task UpdateAsync(SurveyAssignment surveyAssignment);

        Task DeleteAsync(Guid surveyAssignmentId);
    }
}