using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClaimShield.Api.Models.Entities
{
    [Table("Policies")]
    public class Policy
    {
        [Key]
        public Guid PolicyId { get; set; }

        public Guid CustomerId { get; set; }

        public Guid VehicleId { get; set; }

        [MaxLength(30)]
        public string PolicyNumber { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")]
        public decimal CoverageAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal PremiumAmount { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public DateTime? CreatedDate { get; set; }

        public DateTime? UpdatedDate { get; set; }

        public int? PolicyTypeId { get; set; }

        public int? PolicyStatusId { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? IDV { get; set; }

        // Voluntary + compulsory excess combined, deducted from any
        // claim payout. Used by the Estimate Engine (Phase 12).
        [Column(TypeName = "decimal(18,2)")]
        public decimal? Excess { get; set; }

        // Simple comma-separated list (e.g. "Zero Depreciation, Engine
        // Protect") - not a product catalog, see Phase 12 plan.
        [MaxLength(500)]
        public string? AddOns { get; set; }

        // Navigation Properties
        public Customer? Customer { get; set; }

        public Vehicle? Vehicle { get; set; }
    }
}