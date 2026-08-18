using ClaimShield.Api.Models.DTOs.ReassessmentComments;

namespace ClaimShield.Api.Interfaces.Services
{
    public interface IReassessmentCommentService
    {
        Task<IEnumerable<ReassessmentCommentResponseDto>> GetByClaimAsync(
            Guid claimId);

        Task<(bool Success, string? ErrorMessage, ReassessmentCommentResponseDto? Comment)> CreateAsync(
            Guid authorId,
            CreateReassessmentCommentRequest request);
    }
}
