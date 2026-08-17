using System.Text.Json;
using ClaimShield.Api.Data.Context;
using ClaimShield.Api.Interfaces.Services;
using ClaimShield.Api.Models.Entities;

namespace ClaimShield.Api.Services
{
    public class AuditLogService : IAuditLogService
    {
        private readonly ClaimShieldDbContext _context;

        public AuditLogService(
            ClaimShieldDbContext context)
        {
            _context = context;
        }

        public async Task LogAsync(
            Guid? userId,
            string action,
            string entityType,
            Guid entityId,
            object? before = null,
            object? after = null)
        {
            var auditLog = new AuditLog
            {
                AuditLogId = Guid.NewGuid(),

                UserId = userId,

                Action = action,

                EntityType = entityType,

                EntityId = entityId,

                BeforeState =
                    before == null
                        ? null
                        : JsonSerializer.Serialize(before),

                AfterState =
                    after == null
                        ? null
                        : JsonSerializer.Serialize(after),

                Timestamp = DateTime.UtcNow
            };

            _context.AuditLogs.Add(auditLog);

            await _context.SaveChangesAsync();
        }
    }
}
