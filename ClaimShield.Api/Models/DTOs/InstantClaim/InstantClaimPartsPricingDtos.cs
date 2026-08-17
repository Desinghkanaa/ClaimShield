using System.ComponentModel.DataAnnotations;

namespace ClaimShield.Api.Models.DTOs.InstantClaim
{
    public class InstantClaimPartsPricingResponseDto
    {
        public string PartsPricingId { get; set; } = string.Empty;

        public string PartType { get; set; } = string.Empty;

        public int? MakeId { get; set; }

        public int? ModelId { get; set; }

        public decimal PartsAmount { get; set; }

        public bool IsActive { get; set; }

        public int Version { get; set; }

        public DateTime EffectiveFrom { get; set; }
    }

    public class CreateInstantClaimPartsPricingRequest
    {
        [Required]
        [MaxLength(50)]
        public string PartType { get; set; } = string.Empty;

        public int? MakeId { get; set; }

        public int? ModelId { get; set; }

        [Required]
        public decimal PartsAmount { get; set; }
    }

    public class UpdateInstantClaimPartsPricingRequest
    {
        public int? MakeId { get; set; }

        public int? ModelId { get; set; }

        [Required]
        public decimal PartsAmount { get; set; }
    }
}
