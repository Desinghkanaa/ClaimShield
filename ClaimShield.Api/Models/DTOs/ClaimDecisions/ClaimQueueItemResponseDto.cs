namespace ClaimShield.Api.Models.DTOs.ClaimDecisions
{
    public class ClaimQueueItemResponseDto
    {
        public Guid ClaimId { get; set; }

        public string ClaimNumber { get; set; } = string.Empty;

        public int StatusId { get; set; }

        public decimal? EstimatedLossAmount { get; set; }

        // "AwaitingSurvey", "AwaitingSurveyorDecision", or "AwaitingApproverDecision"
        public string QueueReason { get; set; } = string.Empty;

        // Populated only when QueueReason is AwaitingApproverDecision.
        public Guid? PendingDecisionId { get; set; }
    }
}
