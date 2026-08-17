using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClaimShield.Api.Models.Entities
{
    // Labour rate card, keyed by part type. Mirrors ScoringRule/
    // AuthorityLimit's Admin-configurable, versioned, no-hardcoded-
    // seed-data convention.
    [Table("InstantClaimRateCards", Schema = "Masters")]
    public class InstantClaimRateCard
    {
        [Key]
        [MaxLength(20)]
        public string RateCardId { get; set; } = string.Empty;

        // Windshield-Front / Windshield-Rear / Glass / Tyre - matches
        // ClaimIntake.InstantClaimParts's key set.
        [MaxLength(50)]
        public string PartType { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")]
        public decimal RemoveRefitCharge { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal DentingCharge { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal PaintingCharge { get; set; }

        // Salvage deduction, percentage of parts amount (0-100).
        [Column(TypeName = "decimal(5,2)")]
        public decimal SalvagePercent { get; set; }

        public bool IsActive { get; set; }

        public int Version { get; set; }

        public DateTime EffectiveFrom { get; set; }
    }
}
