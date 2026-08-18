namespace ClaimShield.Api.Interfaces.Services
{
    public interface IAuditLogService
    {
        Task LogAsync(
            Guid? userId,
            string action,
            string entityType,
            Guid entityId,
            object? before = null,
            object? after = null);
    }
}
