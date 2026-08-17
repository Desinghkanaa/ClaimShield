using System.ComponentModel.DataAnnotations.Schema;

namespace ClaimShield.Api.Models.Entities
{
    [Table("AuthorityLimits", Schema = "Masters")]
    public class AuthorityLimit
    {
        public int RoleId { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? MaxApprovalAmount { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal? MaxRiskScore { get; set; }

        public DateTime UpdatedDate { get; set; }

        public Guid? UpdatedBy { get; set; }
    }
}
