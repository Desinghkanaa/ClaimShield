using ClaimShield.Api.Models.Entities;

namespace ClaimShield.Api.Interfaces.Repositories
{
    public interface IRoleRepository
    {
        Task<IEnumerable<Role>> GetAllRolesAsync();

        Task<Role?> GetRoleByIdAsync(int roleId);

        Task<Role?> GetRoleByNameAsync(string roleName);
    }
}