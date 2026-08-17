using System.ComponentModel.DataAnnotations;

namespace ClaimShield.Api.Models.DTOs.ReassessmentComments
{
    public class CreateReassessmentCommentRequest
    {
        [Required]
        public Guid ClaimId { get; set; }

        [Required]
        [MaxLength(2000)]
        public string Comment { get; set; } = string.Empty;
    }
}
