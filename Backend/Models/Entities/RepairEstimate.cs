using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClaimShield.Api.Models.Entities
{
    [Table("RepairEstimates", Schema = "dbo")]
    public class RepairEstimate
    {
        [Key]
        public Guid RepairEstimateId { get; set; }

        public Guid RepairAssignmentId { get; set; }

        public Guid ClaimId { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal EstimatedAmount { get; set; }

        public int? EstimatedCompletionDays { get; set; }

        public string? EstimateRemarks { get; set; }

        public DateTime? SubmittedDate { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? ApprovedAmount { get; set; }

        public DateTime? ApprovalDate { get; set; }

        // Null = pending. Set only via the dedicated Approve/Reject
        // actions - never by the Repairer's generic update.
        public int? ApprovalStatusId { get; set; }

        [MaxLength(1000)]
        public string? ApprovalRemarks { get; set; }

        public DateTime? CreatedDate { get; set; }
    }
}