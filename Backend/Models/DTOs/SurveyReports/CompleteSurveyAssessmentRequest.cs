using System.ComponentModel.DataAnnotations;

namespace ClaimShield.Api.Models.DTOs.SurveyReports
{
    public class CompleteSurveyAssessmentRequest
    {
        [Required]
        public Guid SurveyReportId { get; set; }
    }
}
