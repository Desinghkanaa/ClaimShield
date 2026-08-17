using System.ComponentModel.DataAnnotations;

namespace ClaimShield.Api.Models.DTOs.InstantClaim
{
    public class InstantClaimRateCardResponseDto
    {
        public string RateCardId { get; set; } = string.Empty;

        public string PartType { get; set; } = string.Empty;

        public decimal RemoveRefitCharge { get; set; }

        public decimal DentingCharge { get; set; }

        public decimal PaintingCharge { get; set; }

        public decimal SalvagePercent { get; set; }

        public bool IsActive { get; set; }

        public int Version { get; set; }

        public DateTime EffectiveFrom { get; set; }
    }

    public class CreateInstantClaimRateCardRequest
    {
        [Required]
        [MaxLength(50)]
        public string PartType { get; set; } = string.Empty;

        [Required]
        public decimal RemoveRefitCharge { get; set; }

        [Required]
        public decimal DentingCharge { get; set; }

        [Required]
        public decimal PaintingCharge { get; set; }

        [Required]
        [Range(0, 100)]
        public decimal SalvagePercent { get; set; }
    }

    public class UpdateInstantClaimRateCardRequest
    {
        [Required]
        public decimal RemoveRefitCharge { get; set; }

        [Required]
        public decimal DentingCharge { get; set; }

        [Required]
        public decimal PaintingCharge { get; set; }

        [Required]
        [Range(0, 100)]
        public decimal SalvagePercent { get; set; }
    }
}
