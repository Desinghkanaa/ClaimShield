namespace ClaimShield.Api.Models.DTOs.ClaimScoring
{
    public class ScoringRuleResponseDto
    {
        public string RuleId { get; set; } = string.Empty;

        public int Stage { get; set; }

        public string Category { get; set; } = string.Empty;

        public string ConditionField { get; set; } = string.Empty;

        public string ConditionOperator { get; set; } = string.Empty;

        public string ConditionThreshold { get; set; } = string.Empty;

        public int Severity { get; set; }

        public int Points { get; set; }

        public bool IsActive { get; set; }

        public int Version { get; set; }

        public DateTime EffectiveFrom { get; set; }
    }
}
