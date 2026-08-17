using System.ComponentModel.DataAnnotations;

namespace ClaimShield.Api.Models.DTOs.Claims
{
    public class CreateClaimRequest
    {
        [Required]
        public Guid PolicyId { get; set; }

        [Required]
        public Guid CustomerId { get; set; }

        [Required]
        public Guid VehicleId { get; set; }

        [Required]
        public DateTime IncidentDate { get; set; }

        public DateTime? ReportedDate { get; set; }

        public string? IncidentLocation { get; set; }

        public string? IncidentDescription { get; set; }

        public decimal? EstimatedLossAmount { get; set; }

        public decimal? ApprovedAmount { get; set; }

        public bool? IsFraudSuspected { get; set; }

        public int? StatusId { get; set; }
    }
}