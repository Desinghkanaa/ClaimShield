namespace ClaimShield.Api.Interfaces.Services
{
    public interface ISupabaseStorageService
    {
        Task UploadAsync(
            string objectPath,
            Stream content,
            string contentType);

        Task<string> CreateSignedUrlAsync(
            string objectPath,
            int expiresInSeconds = 300);

        // Server-side raw download - used by the OCR service (Phase
        // 12), which needs the actual bytes rather than a signed URL
        // for a browser.
        Task<byte[]> DownloadAsync(
            string objectPath);

        Task DeleteAsync(
            string objectPath);
    }
}
