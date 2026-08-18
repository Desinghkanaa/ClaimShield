namespace ClaimShield.Api.Models.DTOs.RepairAssignments
{
    public class RepairAssignmentResponseDto
    {
        public Guid RepairAssignmentId { get; set; }

        public Guid ClaimId { get; set; }

        public Guid RepairerId { get; set; }

        public Guid AssignedBy { get; set; }

        public DateTime? AssignedDate { get; set; }

        public DateTime? ExpectedCompletionDate { get; set; }

        public int AssignmentStatusId { get; set; }

        public string? Remarks { get; set; }

        public DateTime? CreatedDate { get; set; }

        public DateTime? UpdatedDate { get; set; }
    }
}