using ClaimShield.Api.Models.DTOs.Ocr;

namespace ClaimShield.Api.Interfaces.Services
{
    public interface IOcrService
    {
        Task<OcrExtractionResult> ExtractAsync(
            byte[] imageBytes);
    }
}
