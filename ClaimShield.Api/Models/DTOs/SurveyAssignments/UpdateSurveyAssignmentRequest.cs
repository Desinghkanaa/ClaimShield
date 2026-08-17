using System.ComponentModel.DataAnnotations;

namespace ClaimShield.Api.Models.DTOs.SurveyAssignments
{
    public class UpdateSurveyAssignmentRequest
    {
        [Required]
        public Guid SurveyAssignmentId { get; set; }

        [Required]
        public Guid ClaimId { get; set; }

        [Required]
        public Guid SurveyorId { get; set; }

        [Required]
        public Guid AssignedBy { get; set; }

        public DateTime? AssignedDate { get; set; }

        public DateTime? DueDate { get; set; }

        [Required]
        public int AssignmentStatusId { get; set; }

        [Required]
        public int InspectionMode { get; set; }

        [MaxLength(1000)]
        public string? Remarks { get; set; }
    }
}