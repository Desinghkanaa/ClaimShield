namespace ClaimShield.Api.Models.DTOs.AuditLogs
{
    public class AuditLogResponseDto
    {
        public Guid AuditLogId { get; set; }

        public Guid? UserId { get; set; }

        public string UserName { get; set; } = "System";

        public string Action { get; set; } = string.Empty;

        public string EntityType { get; set; } = string.Empty;

        public Guid EntityId { get; set; }

        public DateTime Timestamp { get; set; }
    }
}
