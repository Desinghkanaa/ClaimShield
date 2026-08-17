using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClaimShield.Api.Models.Entities
{
    [Table("ClaimDecisions")]
    public class ClaimDecision
    {
        [Key]
        public Guid ClaimDecisionId { get; set; }

        public Guid ClaimId { get; set; }

        public Guid DecidedBy { get; set; }

        public int RoleId { get; set; }

        public int Decision { get; set; }

        [Required]
        public string Reasoning { get; set; } = string.Empty;

        [Column(TypeName = "jsonb")]
        public string? AiScoresSnapshot { get; set; }

        public DateTime DecisionDate { get; set; }
    }
}
