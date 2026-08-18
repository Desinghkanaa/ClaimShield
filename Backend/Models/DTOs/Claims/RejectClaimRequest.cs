using System.ComponentModel.DataAnnotations;

namespace ClaimShield.Api.Models.DTOs.Claims
{
    public class RejectClaimRequest
    {
        [Required]
        [MaxLength(1000)]
        public string Remarks { get; set; } = string.Empty;
    }
}