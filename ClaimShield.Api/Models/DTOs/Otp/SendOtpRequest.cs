using System.ComponentModel.DataAnnotations;

namespace ClaimShield.Api.Models.DTOs.Otp
{
    public class SendOtpRequest
    {
        [Required]
        public string Purpose { get; set; } = string.Empty;

        // Required only for Purpose=InstantClaimAccept - the claim this
        // OTP is scoped to. Ignored for Purpose=Login (subject is
        // always the caller's own UserId).
        public Guid? ClaimId { get; set; }
    }
}
