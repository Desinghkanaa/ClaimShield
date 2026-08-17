namespace ClaimShield.Api.Models.DTOs.ClaimScoring
{
    // Customer-facing shape - deliberately its own class (not a filtered
    // view of InternalClaimScoringDto) so a future serialization change
    // can never accidentally leak rule-level detail to a Customer.

    public class CustomerClaimScoreDto
    {
        public Guid ClaimId { get; set; }

        public int CompositeScore { get; set; }

        public int CompositeBand { get; set; }

        public string CompositeBandName { get; set; } = string.Empty;

        public DateTime? LastScoredAt { get; set; }
    }
}
