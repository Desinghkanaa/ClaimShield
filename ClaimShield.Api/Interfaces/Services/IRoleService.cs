using ClaimShield.Api.Models.Entities;

namespace ClaimShield.Api.Interfaces.Services
{
    public interface IRoleService
    {
        Task<IEnumerable<Role>> GetAllRolesAsync();

        Task<Role?> GetRoleByIdAsync(int roleId);

        Task<Role?> GetRoleByNameAsync(string roleName);
    }
}