namespace ClaimShield.Api.Models.DTOs.RepairEstimates
{
    public class RepairEstimateResponseDto
    {
        public Guid RepairEstimateId { get; set; }

        public Guid RepairAssignmentId { get; set; }

        public Guid ClaimId { get; set; }

        public decimal EstimatedAmount { get; set; }

        public int? EstimatedCompletionDays { get; set; }

        public string? EstimateRemarks { get; set; }

        public DateTime? SubmittedDate { get; set; }

        public decimal? ApprovedAmount { get; set; }

        public DateTime? ApprovalDate { get; set; }

        public int? ApprovalStatusId { get; set; }

        public string? ApprovalStatus { get; set; }

        public string? ApprovalRemarks { get; set; }

        public DateTime? CreatedDate { get; set; }
    }
}