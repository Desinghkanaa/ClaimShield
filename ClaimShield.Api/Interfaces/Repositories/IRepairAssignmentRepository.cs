using ClaimShield.Api.Models.Entities;

namespace ClaimShield.Api.Interfaces.Repositories
{
    public interface IRepairAssignmentRepository
    {
        Task<IEnumerable<RepairAssignment>> GetAllAsync();

        Task<RepairAssignment?> GetByIdAsync(
            Guid repairAssignmentId);

        Task<IEnumerable<RepairAssignment>> GetByClaimAsync(
            Guid claimId);

        Task<IEnumerable<RepairAssignment>> GetByRepairerAsync(
            Guid repairerId);

        Task AddAsync(
            RepairAssignment repairAssignment);

        Task UpdateAsync(
            RepairAssignment repairAssignment);

        Task DeleteAsync(
            Guid repairAssignmentId);
    }
}