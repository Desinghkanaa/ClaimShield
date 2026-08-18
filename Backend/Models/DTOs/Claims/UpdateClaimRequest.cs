using System.ComponentModel.DataAnnotations;

namespace ClaimShield.Api.Models.DTOs.Claims
{
    public class UpdateClaimRequest
    {
        [Required]
        public Guid ClaimId { get; set; }

        [Required]
        public Guid PolicyId { get; set; }

        [Required]
        public Guid CustomerId { get; set; }

        [Required]
        public Guid VehicleId { get; set; }

        [Required]
        [MaxLength(30)]
        public string ClaimNumber { get; set; } = string.Empty;

        [Required]
        public DateTime IncidentDate { get; set; }

        public DateTime? ReportedDate { get; set; }

        [MaxLength(500)]
        public string? IncidentLocation { get; set; }

        public string? IncidentDescription { get; set; }

        public decimal? EstimatedLossAmount { get; set; }

        public decimal? ApprovedAmount { get; set; }

        public bool? IsFraudSuspected { get; set; }

        public int? StatusId { get; set; }
    }
}