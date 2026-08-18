using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClaimShield.Api.Models.Entities
{
    // Parts price, keyed by part type + vehicle make/model. Same
    // Admin-configurable, versioned, no-hardcoded-seed-data convention
    // as InstantClaimRateCard/ScoringRule.
    [Table("InstantClaimPartsPricing", Schema = "Masters")]
    public class InstantClaimPartsPricing
    {
        [Key]
        [MaxLength(20)]
        public string PartsPricingId { get; set; } = string.Empty;

        [MaxLength(50)]
        public string PartType { get; set; } = string.Empty;

        public int? MakeId { get; set; }

        public int? ModelId { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal PartsAmount { get; set; }

        public bool IsActive { get; set; }

        public int Version { get; set; }

        public DateTime EffectiveFrom { get; set; }
    }
}
