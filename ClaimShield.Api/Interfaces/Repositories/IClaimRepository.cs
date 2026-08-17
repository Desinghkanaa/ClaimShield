using ClaimShield.Api.Models.Entities;

namespace ClaimShield.Api.Interfaces.Repositories
{
    public interface IClaimRepository
    {
        Task<IEnumerable<Claim>> GetAllAsync();

        Task<Claim?> GetByIdAsync(Guid claimId);

        // Phase 13 - includes Policy/Customer(.User)/Vehicle for the
        // Surveyor claim-detail header. Kept separate from GetByIdAsync so
        // the cheap access-check call sites elsewhere aren't paying for
        // three extra joins they don't need.
        Task<Claim?> GetByIdWithDetailsAsync(Guid claimId);

        Task<IEnumerable<Claim>> GetByCustomerAsync(Guid customerId);

        Task<IEnumerable<Claim>> GetByPolicyAsync(Guid policyId);

        Task<IEnumerable<Claim>> GetByVehicleAsync(Guid vehicleId);

        Task AddAsync(Claim claim);

        Task UpdateAsync(Claim claim);

        Task DeleteAsync(Guid claimId);
    }
}