using ClaimShield.Api.Models.Entities;

namespace ClaimShield.Api.Interfaces.Repositories
{
    public interface IRepairEstimateRepository
    {
        Task<IEnumerable<RepairEstimate>> GetAllAsync();

        Task<RepairEstimate?> GetByIdAsync(
            Guid repairEstimateId);

        Task<IEnumerable<RepairEstimate>> GetByClaimAsync(
            Guid claimId);

        Task<IEnumerable<RepairEstimate>> GetByAssignmentAsync(
            Guid repairAssignmentId);

        Task AddAsync(
            RepairEstimate repairEstimate);

        Task UpdateAsync(
            RepairEstimate repairEstimate);

        Task DeleteAsync(
            Guid repairEstimateId);
    }
}