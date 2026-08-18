using System.ComponentModel.DataAnnotations;

namespace ClaimShield.Api.Models.DTOs.ClaimScoring
{
    public class CreateScoringRuleRequest
    {
        [Required]
        public int Stage { get; set; }

        [Required]
        [MaxLength(100)]
        public string Category { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string ConditionField { get; set; } = string.Empty;

        [Required]
        [MaxLength(10)]
        public string ConditionOperator { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string ConditionThreshold { get; set; } = string.Empty;

        [Required]
        public int Severity { get; set; }

        public int Points { get; set; }
    }
}
