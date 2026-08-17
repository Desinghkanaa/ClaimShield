namespace ClaimShield.Api.Models.DTOs.ClaimScoring
{
    public class ScoringStageDto
    {
        public int Stage { get; set; }

        public string StageName { get; set; } = string.Empty;

        public int ScoreValue { get; set; }

        public bool HardFlagTriggered { get; set; }

        public int Band { get; set; }

        public string BandName { get; set; } = string.Empty;

        public List<string> TriggeredRuleIds { get; set; } = new();

        public string ReasonText { get; set; } = string.Empty;

        public string RuleSetVersion { get; set; } = string.Empty;

        public DateTime ScoredAt { get; set; }
    }
}
