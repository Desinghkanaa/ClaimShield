namespace ClaimShield.Api.Models.DTOs.ClaimDecisions
{
    public class ClaimDecisionResult
    {
        public bool Success { get; set; }

        public string? ErrorMessage { get; set; }

        public bool Escalated { get; set; }

        public int? UpdatedClaimStatusId { get; set; }

        public ClaimDecisionResponseDto? Decision { get; set; }
    }
}
