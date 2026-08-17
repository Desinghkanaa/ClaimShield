using ClaimShield.Api.Models.DTOs.ClaimDecisions;

namespace ClaimShield.Api.Interfaces.Services
{
    public interface IClaimDecisionService
    {
        Task<ClaimDecisionResponseDto?> GetLatestDecisionAsync(
            Guid claimId);

        Task<IEnumerable<ClaimDecisionResponseDto>> GetHistoryAsync(
            Guid claimId);

        Task<IEnumerable<ClaimQueueItemResponseDto>> GetMyQueueAsync(
            Guid userId,
            int roleId);

        Task<ClaimDecisionResult> RecordSurveyorDecisionAsync(
            Guid claimId,
            Guid surveyorId,
            SurveyorDecisionRequest request);

        Task<ClaimDecisionResult> RecordApproverDecisionAsync(
            Guid claimId,
            Guid approverId,
            ApproverDecisionRequest request);
    }
}
