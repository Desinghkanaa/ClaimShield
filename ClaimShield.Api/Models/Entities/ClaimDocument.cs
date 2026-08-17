using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClaimShield.Api.Models.Entities
{
    [Table("ClaimDocuments", Schema = "dbo")]
    public class ClaimDocument
    {
        [Key]
        public Guid ClaimDocumentId { get; set; }

        public Guid ClaimId { get; set; }

        public int DocumentTypeId { get; set; }

        [Required]
        [MaxLength(510)]
        public string FileName { get; set; } = string.Empty;

        [Required]
        [MaxLength(510)]
        public string OriginalFileName { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string FileExtension { get; set; } = string.Empty;

        public long FileSize { get; set; }

        [Required]
        [MaxLength(1000)]
        public string FilePath { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string ContentType { get; set; } = string.Empty;

        public Guid UploadedBy { get; set; }

        public DateTime? UploadedDate { get; set; }

        public bool? IsVerified { get; set; }

        public Guid? VerifiedBy { get; set; }

        public DateTime? VerifiedDate { get; set; }

        [MaxLength(1000)]
        public string? Remarks { get; set; }
    }
}