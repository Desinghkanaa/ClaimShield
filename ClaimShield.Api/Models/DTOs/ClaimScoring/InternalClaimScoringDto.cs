namespace ClaimShield.Api.Models.DTOs.ClaimScoring
{
    // Surveyor / Approver / Admin shape - full rule-level detail.

    public class InternalClaimScoringDto
    {
        public Guid ClaimId { get; set; }

        public int CompositeScore { get; set; }

        public int CompositeBand { get; set; }

        public string CompositeBandName { get; set; } = string.Empty;

        public DateTime? LastScoredAt { get; set; }

        public List<ScoringStageDto> Stages { get; set; } = new();
    }
}
