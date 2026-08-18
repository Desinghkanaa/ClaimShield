namespace ClaimShield.Api.Models.DTOs.ClaimDecisions
{
    public class ClaimDecisionResponseDto
    {
        public Guid ClaimDecisionId { get; set; }

        public Guid ClaimId { get; set; }

        public Guid DecidedBy { get; set; }

        public string DecidedByName { get; set; } = string.Empty;

        public int RoleId { get; set; }

        public string RoleName { get; set; } = string.Empty;

        public int Decision { get; set; }

        public string DecisionName { get; set; } = string.Empty;

        public string Reasoning { get; set; } = string.Empty;

        public string? AiScoresSnapshot { get; set; }

        public DateTime DecisionDate { get; set; }

        // True when this decision required (or is still awaiting)
        // Approver sign-off rather than taking effect immediately.
        public bool Escalated { get; set; }
    }
}
