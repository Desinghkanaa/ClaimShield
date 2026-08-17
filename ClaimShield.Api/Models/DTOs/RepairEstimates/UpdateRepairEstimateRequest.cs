using System.ComponentModel.DataAnnotations;

namespace ClaimShield.Api.Models.DTOs.RepairEstimates
{
    public class UpdateRepairEstimateRequest
    {
        [Required]
        public Guid RepairEstimateId { get; set; }

        [Required]
        public Guid RepairAssignmentId { get; set; }

        [Required]
        public Guid ClaimId { get; set; }

        [Required]
        public decimal EstimatedAmount { get; set; }

        public int? EstimatedCompletionDays { get; set; }

        public string? EstimateRemarks { get; set; }

        public DateTime? SubmittedDate { get; set; }
    }
}