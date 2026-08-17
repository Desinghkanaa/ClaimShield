using System.ComponentModel.DataAnnotations;

namespace ClaimShield.Api.Models.DTOs.ClaimDecisions
{
    public class ApproverDecisionRequest
    {
        // Only Approve (1) or Deny (3) are valid for the checker step.
        // "Review" is a Surveyor-only, maker-side decision.

        [Required]
        public int Decision { get; set; }

        [Required]
        [MaxLength(2000)]
        public string Reasoning { get; set; } = string.Empty;
    }
}
