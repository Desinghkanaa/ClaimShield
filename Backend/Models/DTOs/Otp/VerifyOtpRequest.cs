using System.ComponentModel.DataAnnotations;

namespace ClaimShield.Api.Models.DTOs.Otp
{
    public class VerifyOtpRequest
    {
        [Required]
        public string Purpose { get; set; } = string.Empty;

        public Guid? ClaimId { get; set; }

        [Required]
        [MaxLength(6)]
        public string Code { get; set; } = string.Empty;
    }
}
