using System.ComponentModel.DataAnnotations;

namespace ClaimShield.Api.Models.DTOs.ClaimRaise
{
    public class InstantClaimPartsSelection
    {
        public bool WindshieldFront { get; set; }

        public bool WindshieldRear { get; set; }

        public bool Glass { get; set; }

        public bool Tyre { get; set; }
    }

    public class RaiseStep1Request
    {
        [Required]
        public Guid PolicyId { get; set; }

        [Required]
        public Guid VehicleId { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Vehicle location is required.")]
        public int VehicleLocationAtLoss { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Loss type is required.")]
        public int LossType { get; set; }

        [Required]
        public DateTime DateOfLoss { get; set; }

        [Required]
        [MaxLength(500)]
        public string LocationOfLoss { get; set; } = string.Empty;

        [Required]
        [MinLength(10, ErrorMessage = "Please provide a bit more detail (at least 10 characters).")]
        public string Description { get; set; } = string.Empty;

        public bool InstantClaimToggle { get; set; }

        public InstantClaimPartsSelection? InstantClaimParts { get; set; }

        public decimal? CustomerEstimatedAmount { get; set; }
    }

    public class RaiseStep1ResponseDto
    {
        public Guid ClaimId { get; set; }

        public string ClaimNumber { get; set; } = string.Empty;

        public string Message { get; set; } =
            "Your claim is registered successfully. You're just two steps away from getting paid instantly.";
    }
}
