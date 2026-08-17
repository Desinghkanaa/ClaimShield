using System.ComponentModel.DataAnnotations;

namespace ClaimShield.Api.Models.DTOs.RepairAssignments
{
    public class CreateRepairAssignmentRequest
    {
        [Required]
        public Guid ClaimId { get; set; }

        [Required]
        public Guid RepairerId { get; set; }

        [Required]
        public Guid AssignedBy { get; set; }

        public DateTime? AssignedDate { get; set; }

        public DateTime? ExpectedCompletionDate { get; set; }

        [Required]
        public int AssignmentStatusId { get; set; }

        [MaxLength(500)]
        public string? Remarks { get; set; }
    }
}