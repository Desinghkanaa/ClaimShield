using ClaimShield.Api.Models.DTOs.RepairAssignments;

namespace ClaimShield.Api.Interfaces.Services
{
    public interface IRepairAssignmentService
    {
        Task<IEnumerable<RepairAssignmentResponseDto>> GetAllAsync();

        Task<RepairAssignmentResponseDto?> GetByIdAsync(
            Guid repairAssignmentId);

        Task<IEnumerable<RepairAssignmentResponseDto>> GetByClaimAsync(
            Guid claimId);

        Task<IEnumerable<RepairAssignmentResponseDto>> GetByRepairerAsync(
            Guid repairerId);

        Task<RepairAssignmentResponseDto> CreateAsync(
            CreateRepairAssignmentRequest request);

        Task<bool> UpdateAsync(
            UpdateRepairAssignmentRequest request);

        Task<bool> DeleteAsync(
            Guid repairAssignmentId);
    }
}