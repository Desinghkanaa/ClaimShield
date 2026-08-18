using System.ComponentModel.DataAnnotations;

namespace ClaimShield.Api.Models.DTOs.SurveyReports
{
    public class UpdateSurveyReportRequest
    {
        [Required]
        public Guid SurveyReportId { get; set; }

        [Required]
        public Guid SurveyAssignmentId { get; set; }

        [Required]
        public Guid ClaimId { get; set; }

        [Required]
        public Guid SurveyorId { get; set; }

        [Required]
        public DateTime InspectionDate { get; set; }

        public int? OdometerReading { get; set; }

        [Required]
        public int DamageTypeId { get; set; }

        public string? DamageDescription { get; set; }

        public decimal? EstimatedRepairCost { get; set; }

        public bool? TotalLoss { get; set; }

        public string? SurveyRemarks { get; set; }
    }
}