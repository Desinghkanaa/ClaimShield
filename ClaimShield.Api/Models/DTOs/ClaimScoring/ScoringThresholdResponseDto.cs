namespace ClaimShield.Api.Models.DTOs.ClaimScoring
{
    public class ScoringThresholdResponseDto
    {
        public string ThresholdSet { get; set; } = string.Empty;

        public int AmberMin { get; set; }

        public int RedMin { get; set; }

        public bool IsActive { get; set; }
    }
}
