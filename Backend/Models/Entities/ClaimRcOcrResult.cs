using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClaimShield.Api.Models.Entities
{
    [Table("ClaimRcOcrResults", Schema = "dbo")]
    public class ClaimRcOcrResult
    {
        [Key]
        public Guid ClaimId { get; set; }

        [MaxLength(30)]
        public string? ExtractedRegNumber { get; set; }

        [MaxLength(200)]
        public string? ExtractedOwnerName { get; set; }

        [MaxLength(50)]
        public string? ExtractedChassisNumber { get; set; }

        [MaxLength(30)]
        public string? PlatePhotoExtractedRegNumber { get; set; }

        [MaxLength(30)]
        public string? PolicyRegNumber { get; set; }

        public int MatchStatus { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal? RawOcrConfidence { get; set; }

        public DateTime ProcessedAt { get; set; }
    }
}
