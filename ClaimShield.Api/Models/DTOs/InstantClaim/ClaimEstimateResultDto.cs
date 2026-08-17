namespace ClaimShield.Api.Models.DTOs.InstantClaim
{
    public class EstimateLineItemsDto
    {
        public decimal RemoveRefitCharge { get; set; }

        public decimal DentingCharge { get; set; }

        public decimal PaintingCharge { get; set; }

        public decimal TotalLabourCharges { get; set; }

        public decimal TotalPartsAmount { get; set; }

        public decimal PolicyExcess { get; set; }

        public decimal SalvageAmount { get; set; }

        public decimal OtherDeductions { get; set; }
    }

    public class ClaimEstimateResultDto
    {
        public Guid ClaimId { get; set; }

        public EstimateLineItemsDto LineItems { get; set; } = new();

        public decimal NetAssessmentAmount { get; set; }

        public string RuleSetVersion { get; set; } = string.Empty;

        public DateTime GeneratedAt { get; set; }

        public int? CustomerDecision { get; set; }
    }

    // Returned by raise/estimate when the claim is not (or no longer)
    // eligible for the Instant Claim path - the frontend uses Reason to
    // choose the right routed-to-Surveyor message.
    public class NotEligibleResultDto
    {
        public bool Eligible { get; set; } = false;

        public string Reason { get; set; } = string.Empty;
    }
}
