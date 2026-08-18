using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClaimShield.Api.Models.Entities
{
    [Table("ClaimScoringResults", Schema = "dbo")]
    public class ClaimScoringResult
    {
        [Key]
        public Guid ClaimScoringResultId { get; set; }

        public Guid ClaimId { get; set; }

        public int Stage { get; set; }

        public int ScoreValue { get; set; }

        public bool HardFlagTriggered { get; set; }

        public int Band { get; set; }

        [Column(TypeName = "jsonb")]
        public string TriggeredRuleIds { get; set; } = "[]";

        public string ReasonText { get; set; } = string.Empty;

        [MaxLength(2000)]
        public string RuleSetVersion { get; set; } = string.Empty;

        public DateTime ScoredAt { get; set; }

        public Guid? SupersededBy { get; set; }
    }
}
