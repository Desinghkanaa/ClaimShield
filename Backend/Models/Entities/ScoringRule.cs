using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClaimShield.Api.Models.Entities
{
    [Table("ScoringRules", Schema = "Masters")]
    public class ScoringRule
    {
        [Key]
        [MaxLength(20)]
        public string RuleId { get; set; } = string.Empty;

        public int Stage { get; set; }

        [MaxLength(100)]
        public string Category { get; set; } = string.Empty;

        [MaxLength(100)]
        public string ConditionField { get; set; } = string.Empty;

        [MaxLength(10)]
        public string ConditionOperator { get; set; } = string.Empty;

        [MaxLength(50)]
        public string ConditionThreshold { get; set; } = string.Empty;

        public int Severity { get; set; }

        public int Points { get; set; }

        public bool IsActive { get; set; }

        public int Version { get; set; }

        public DateTime EffectiveFrom { get; set; }
    }
}
