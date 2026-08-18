using ClaimShield.Api.Models.DTOs.Otp;

namespace ClaimShield.Api.Interfaces.Services
{
    public interface IOtpService
    {
        Task<OtpSendResultDto> SendAsync(
            string purpose,
            Guid subjectId);

        Task<OtpVerifyResultDto> VerifyAsync(
            string purpose,
            Guid subjectId,
            string code);

        // Consumes a verified, unconsumed, still-fresh OTP for the
        // given purpose+subject (used by the Instant Claim Accept
        // action). Returns false if no such OTP exists - the caller
        // must never treat this as optional.
        Task<bool> ConsumeFreshVerificationAsync(
            string purpose,
            Guid subjectId,
            TimeSpan freshWithin);
    }
}
