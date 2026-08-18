using System.ComponentModel.DataAnnotations;

namespace ClaimShield.Api.Models.DTOs.ClaimDocuments
{
    public class CreateClaimDocumentRequest
    {
        [Required]
        public Guid ClaimId { get; set; }

        [Required]
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

        [Required]
        public long FileSize { get; set; }

        [Required]
        [MaxLength(1000)]
        public string FilePath { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string ContentType { get; set; } = string.Empty;

        [Required]
        public Guid UploadedBy { get; set; }

        public DateTime? UploadedDate { get; set; }

        public bool? IsVerified { get; set; }

        public Guid? VerifiedBy { get; set; }

        public DateTime? VerifiedDate { get; set; }

        [MaxLength(1000)]
        public string? Remarks { get; set; }
    }
}