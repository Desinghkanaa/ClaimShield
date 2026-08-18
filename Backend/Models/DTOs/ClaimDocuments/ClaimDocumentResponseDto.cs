namespace ClaimShield.Api.Models.DTOs.ClaimDocuments
{
    public class ClaimDocumentResponseDto
    {
        public Guid ClaimDocumentId { get; set; }

        public Guid ClaimId { get; set; }

        public int DocumentTypeId { get; set; }

        public string FileName { get; set; } = string.Empty;

        public string OriginalFileName { get; set; } = string.Empty;

        public string FileExtension { get; set; } = string.Empty;

        public long FileSize { get; set; }

        public string FilePath { get; set; } = string.Empty;

        public string ContentType { get; set; } = string.Empty;

        public Guid UploadedBy { get; set; }

        public DateTime? UploadedDate { get; set; }

        public bool? IsVerified { get; set; }

        public Guid? VerifiedBy { get; set; }

        public DateTime? VerifiedDate { get; set; }

        public string? Remarks { get; set; }
    }
}