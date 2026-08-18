using ClaimShield.Api.Models.Entities;

namespace ClaimShield.Api.Interfaces.Repositories
{
    public interface IPolicyRepository
    {
        Task<IEnumerable<Policy>> GetAllAsync();

        Task<Policy?> GetByIdAsync(Guid policyId);

        Task<IEnumerable<Policy>> GetByCustomerAsync(Guid customerId);

        Task AddAsync(Policy policy);

        Task UpdateAsync(Policy policy);

        Task DeleteAsync(Guid policyId);
    }
}