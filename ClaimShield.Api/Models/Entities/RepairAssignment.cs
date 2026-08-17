using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClaimShield.Api.Models.Entities
{
    [Table("RepairAssignments", Schema = "dbo")]
    public class RepairAssignment
    {
        [Key]
        public Guid RepairAssignmentId { get; set; }

        public Guid ClaimId { get; set; }

        public Guid RepairerId { get; set; }

        public Guid AssignedBy { get; set; }

        public DateTime? AssignedDate { get; set; }

        public DateTime? ExpectedCompletionDate { get; set; }

        public int AssignmentStatusId { get; set; }

        [MaxLength(500)]
        public string? Remarks { get; set; }

        public DateTime? CreatedDate { get; set; }

        public DateTime? UpdatedDate { get; set; }
    }
}