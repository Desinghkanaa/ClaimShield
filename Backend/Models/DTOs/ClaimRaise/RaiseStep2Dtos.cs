using System.ComponentModel.DataAnnotations;

namespace ClaimShield.Api.Models.DTOs.ClaimRaise
{
    public class RaiseStep2Request
    {
        [Required]
        public bool VehicleParkedSafely { get; set; }

        [Required]
        public bool DeathOccurred { get; set; }
    }

    public class RaiseStep2ResponseDto
    {
        public int MatchStatus { get; set; }

        public bool RoutedToSurveyor { get; set; }

        public string Message { get; set; } = string.Empty;
    }
}
