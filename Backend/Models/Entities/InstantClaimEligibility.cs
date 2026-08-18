using System.ComponentModel.DataAnnotations;

namespace ClaimShield.Api.Models.Entities
{
    // Single-row config, mirrors ScoringThreshold's "default" row
    // convention. No seed data - fail-safe to Green-only (most
    // conservative) until an Admin explicitly widens it.
    public class InstantClaimEligibility
    {
        [Key]
        [MaxLength(50)]
        public string EligibilitySet { get; set; } = string.Empty;

        // Minimum eligible band: 1=Green, 2=Amber, 3=Red. A claim's
        // composite band must be numerically <= this to qualify.
        public int MinEligibleBand { get; set; }

        public bool IsActive { get; set; }
    }
}
