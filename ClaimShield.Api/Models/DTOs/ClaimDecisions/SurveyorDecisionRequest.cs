using System.ComponentModel.DataAnnotations;

namespace ClaimShield.Api.Models.DTOs.ClaimDecisions
{
    public class SurveyorDecisionRequest
    {
        [Required]
        [Range(1, 3)]
        public int Decision { get; set; }

        [Required]
        [MaxLength(2000)]
        public string Reasoning { get; set; } = string.Empty;
    }
}
