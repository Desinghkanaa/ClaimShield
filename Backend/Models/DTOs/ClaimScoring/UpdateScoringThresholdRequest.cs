using System.ComponentModel.DataAnnotations;

namespace ClaimShield.Api.Models.DTOs.ClaimScoring
{
    public class UpdateScoringThresholdRequest
    {
        [Required]
        public int AmberMin { get; set; }

        [Required]
        public int RedMin { get; set; }
    }
}
