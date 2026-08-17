namespace ClaimShield.Api.Models.DTOs.AuthorityLimits
{
    public class AuthorityLimitResponseDto
    {
        public int RoleId { get; set; }

        public string RoleName { get; set; } = string.Empty;

        public decimal? MaxApprovalAmount { get; set; }

        public decimal? MaxRiskScore { get; set; }

        public DateTime UpdatedDate { get; set; }

        public Guid? UpdatedBy { get; set; }
    }
}
