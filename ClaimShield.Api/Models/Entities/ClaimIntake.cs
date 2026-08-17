using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClaimShield.Api.Models.Entities
{
    [Table("ClaimIntakes", Schema = "dbo")]
    public class ClaimIntake
    {
        [Key]
        public Guid ClaimId { get; set; }

        public int VehicleLocationAtLoss { get; set; }

        public int LossType { get; set; }

        public bool InstantClaimToggle { get; set; }

        // jsonb: { "windshieldFront": bool, "windshieldRear": bool,
        // "glass": bool, "tyre": bool } - matches
        // ClaimScoringResult.TriggeredRuleIds's jsonb convention.
        [Column(TypeName = "jsonb")]
        public string InstantClaimParts { get; set; } = "{}";

        [Column(TypeName = "decimal(18,2)")]
        public decimal? CustomerEstimatedAmount { get; set; }

        public bool? VehicleParkedSafely { get; set; }

        public bool? DeathOccurred { get; set; }

        public DateTime CreatedDate { get; set; }
    }
}
