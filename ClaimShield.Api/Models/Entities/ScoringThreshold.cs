using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClaimShield.Api.Models.Entities
{
    [Table("ScoringThresholds", Schema = "Masters")]
    public class ScoringThreshold
    {
        [Key]
        [MaxLength(50)]
        public string ThresholdSet { get; set; } = string.Empty;

        public int AmberMin { get; set; }

        public int RedMin { get; set; }

        public bool IsActive { get; set; }
    }
}
