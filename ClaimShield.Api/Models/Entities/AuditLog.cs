using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClaimShield.Api.Models.Entities
{
    [Table("AuditLogs")]
    public class AuditLog
    {
        [Key]
        public Guid AuditLogId { get; set; }

        public Guid? UserId { get; set; }

        [Required]
        [MaxLength(200)]
        public string Action { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string EntityType { get; set; } = string.Empty;

        public Guid EntityId { get; set; }

        [Column(TypeName = "jsonb")]
        public string? BeforeState { get; set; }

        [Column(TypeName = "jsonb")]
        public string? AfterState { get; set; }

        public DateTime Timestamp { get; set; }
    }
}
