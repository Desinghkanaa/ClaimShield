namespace ClaimShield.Api.Models.DTOs.Policies
{
    public class PolicyResponseDto
    {
        public Guid PolicyId { get; set; }

        public Guid CustomerId { get; set; }

        public Guid VehicleId { get; set; }

        public string PolicyNumber { get; set; } = string.Empty;

        public decimal CoverageAmount { get; set; }

        public decimal PremiumAmount { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public int? PolicyTypeId { get; set; }

        public int? PolicyStatusId { get; set; }

        public decimal? IDV { get; set; }

        public decimal? Excess { get; set; }

        public string? AddOns { get; set; }
    }
}