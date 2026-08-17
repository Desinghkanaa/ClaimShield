namespace ClaimShield.Api.Interfaces.Services
{
    public interface ISupabaseAdminService
    {
        Task<Guid> CreateUserAsync(
            string email,
            string password,
            int roleId,
            string firstName,
            string? lastName,
            string? phoneNumber);

        Task UpdateUserEmailAsync(
            Guid userId,
            string newEmail);

        Task DeleteUserAsync(
            Guid userId);
    }
}
