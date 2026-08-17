using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClaimShield.Api.Models.Entities
{
    [Table("ReassessmentComments")]
    public class ReassessmentComment
    {
        [Key]
        public Guid ReassessmentCommentId { get; set; }

        public Guid ClaimId { get; set; }

        public Guid AuthorId { get; set; }

        [Required]
        public string Comment { get; set; } = string.Empty;

        public DateTime CreatedDate { get; set; }
    }
}
