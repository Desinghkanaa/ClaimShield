using ClaimShield.Api.Models.DTOs.AuthorityLimits;

namespace ClaimShield.Api.Interfaces.Services
{
    public interface IAuthorityLimitService
    {
        Task<IEnumerable<AuthorityLimitResponseDto>> GetAllAsync();

        Task<AuthorityLimitResponseDto?> GetByRoleAsync(
            int roleId);

        Task<AuthorityLimitResponseDto> UpsertAsync(
            int roleId,
            Guid updatedBy,
            UpdateAuthorityLimitRequest request);
    }
}
