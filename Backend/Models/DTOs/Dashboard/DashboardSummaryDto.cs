namespace ClaimShield.Api.Models.DTOs.Dashboard
{
    public class StatusCountDto
    {
        public int StatusId { get; set; }

        public string StatusName { get; set; } = string.Empty;

        public int Count { get; set; }
    }

    public class DailyCountDto
    {
        public string Date { get; set; } = string.Empty;

        public int Count { get; set; }
    }

    public class BandCountDto
    {
        public string BandName { get; set; } = string.Empty;

        public int Count { get; set; }
    }

    public class DashboardSummaryDto
    {
        public int TotalClaims { get; set; }

        public int TotalCustomers { get; set; }

        public decimal TotalPaidAmount { get; set; }

        public decimal AverageClaimAmount { get; set; }

        public double? AverageApprovalTurnaroundDays { get; set; }

        public List<StatusCountDto> ClaimsByStatus { get; set; } = new();

        public List<DailyCountDto> ClaimsOverTime { get; set; } = new();

        public List<BandCountDto> RiskBandDistribution { get; set; } = new();

        public List<StatusCountDto> PaymentsByStatus { get; set; } = new();

        public List<StatusCountDto> RepairEstimateOutcomes { get; set; } = new();
    }
}
