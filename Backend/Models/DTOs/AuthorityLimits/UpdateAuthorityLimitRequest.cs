namespace ClaimShield.Api.Models.DTOs.AuthorityLimits
{
    public class UpdateAuthorityLimitRequest
    {
        // Null = no cap on that dimension.
        public decimal? MaxApprovalAmount { get; set; }

        public decimal? MaxRiskScore { get; set; }
    }
}
