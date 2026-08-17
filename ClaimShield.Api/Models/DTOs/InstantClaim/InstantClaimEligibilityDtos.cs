using System.ComponentModel.DataAnnotations;

namespace ClaimShield.Api.Models.DTOs.InstantClaim
{
    public class InstantClaimEligibilityResponseDto
    {
        public string EligibilitySet { get; set; } = string.Empty;

        public int MinEligibleBand { get; set; }

        public bool IsActive { get; set; }
    }

    public class UpdateInstantClaimEligibilityRequest
    {
        [Required]
        [Range(1, 3)]
        public int MinEligibleBand { get; set; }
    }
}
