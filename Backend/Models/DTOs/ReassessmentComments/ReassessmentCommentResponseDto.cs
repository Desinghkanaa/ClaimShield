namespace ClaimShield.Api.Models.DTOs.ReassessmentComments
{
    public class ReassessmentCommentResponseDto
    {
        public Guid ReassessmentCommentId { get; set; }

        public Guid ClaimId { get; set; }

        public Guid AuthorId { get; set; }

        public string AuthorName { get; set; } = string.Empty;

        public string Comment { get; set; } = string.Empty;

        public DateTime CreatedDate { get; set; }
    }
}
