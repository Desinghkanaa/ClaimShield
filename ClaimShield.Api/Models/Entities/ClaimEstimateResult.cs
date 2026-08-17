using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClaimShield.Api.Models.Entities
{
    [Table("ClaimEstimateResults", Schema = "dbo")]
    public class ClaimEstimateResult
    {
        [Key]
        public Guid ClaimId { get; set; }

        // jsonb: { removeRefitCharge, dentingCharge, paintingCharge,
        // totalLabourCharges, totalPartsAmount, policyExcess,
        // salvageAmount, otherDeductions } - all decimal.
        [Column(TypeName = "jsonb")]
        public string LineItems { get; set; } = "{}";

        [Column(TypeName = "decimal(18,2)")]
        public decimal NetAssessmentAmount { get; set; }

        [MaxLength(2000)]
        public string RuleSetVersion { get; set; } = string.Empty;

        public DateTime GeneratedAt { get; set; }

        // Null = pending. InstantClaimDecisionConstants.
        public int? CustomerDecision { get; set; }

        public DateTime? DecisionAt { get; set; }

        public DateTime? OtpVerifiedAt { get; set; }
    }
}
