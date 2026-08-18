namespace ClaimShield.Api.Models.DTOs.SurveyReports
{
    public class SurveyReportResponseDto
    {
        public Guid SurveyReportId { get; set; }

        public Guid SurveyAssignmentId { get; set; }

        public Guid ClaimId { get; set; }

        public Guid SurveyorId { get; set; }

        public DateTime InspectionDate { get; set; }

        public int? OdometerReading { get; set; }

        public int DamageTypeId { get; set; }

        public string? DamageDescription { get; set; }

        public decimal? EstimatedRepairCost { get; set; }

        public bool? TotalLoss { get; set; }

        public string? SurveyRemarks { get; set; }

        public DateTime? CreatedDate { get; set; }

        public DateTime? UpdatedDate { get; set; }
    }
}