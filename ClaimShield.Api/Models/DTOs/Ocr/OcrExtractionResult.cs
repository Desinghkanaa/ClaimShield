namespace ClaimShield.Api.Models.DTOs.Ocr
{
    public class OcrExtractionResult
    {
        public string RawText { get; set; } = string.Empty;

        public string? RegistrationNumber { get; set; }

        public string? OwnerName { get; set; }

        public string? ChassisNumber { get; set; }

        public decimal Confidence { get; set; }
    }
}
