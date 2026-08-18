namespace ClaimShield.Api.Models.DTOs.Otp
{
    public class OtpSendResultDto
    {
        public bool Success { get; set; }

        public string Message { get; set; } = string.Empty;

        public DateTime ExpiresAt { get; set; }

        // Only populated when running in Development (no SMS vendor
        // configured) - the genuine local delivery channel, not a
        // bypass. Null in every other environment.
        public string? DevModeCode { get; set; }
    }

    public class OtpVerifyResultDto
    {
        public bool Success { get; set; }

        // OtpVerifyResultConstants value.
        public int ResultCode { get; set; }

        public string Message { get; set; } = string.Empty;
    }
}
