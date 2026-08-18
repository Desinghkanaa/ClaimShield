using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClaimShield.Api.Models.Entities
{
    [Table("Claims")]
    public class Claim
    {
        [Key]
        public Guid ClaimId { get; set; }

        public Guid PolicyId { get; set; }

        public Guid CustomerId { get; set; }

        public Guid VehicleId { get; set; }

        [MaxLength(30)]
        public string ClaimNumber { get; set; } = string.Empty;

        public DateTime IncidentDate { get; set; }

        public DateTime? ReportedDate { get; set; }

        [MaxLength(500)]
        public string? IncidentLocation { get; set; }

        public string? IncidentDescription { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? EstimatedLossAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? ApprovedAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? ReserveAmount { get; set; }

        public bool? IsFraudSuspected { get; set; }

        [MaxLength(1000)]
        public string? DecisionRemarks { get; set; }

        public DateTime? CreatedDate { get; set; }

        public DateTime? UpdatedDate { get; set; }

        public int? StatusId { get; set; }

        // Navigation Properties
        public Policy? Policy { get; set; }

        public Customer? Customer { get; set; }

        public Vehicle? Vehicle { get; set; }
    }
}