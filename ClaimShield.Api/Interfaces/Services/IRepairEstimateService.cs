using ClaimShield.Api.Models.DTOs.RepairEstimates;

namespace ClaimShield.Api.Interfaces.Services
{
    public interface IRepairEstimateService
    {
        Task<IEnumerable<RepairEstimateResponseDto>> GetAllAsync();

        Task<RepairEstimateResponseDto?> GetByIdAsync(
            Guid repairEstimateId);

        Task<IEnumerable<RepairEstimateResponseDto>> GetByClaimAsync(
            Guid claimId);

        Task<IEnumerable<RepairEstimateResponseDto>> GetByAssignmentAsync(
            Guid repairAssignmentId);

        Task<RepairEstimateResponseDto> CreateAsync(
            CreateRepairEstimateRequest request);

        Task<bool> UpdateAsync(
            UpdateRepairEstimateRequest request);

        Task<bool> DeleteAsync(
            Guid repairEstimateId);

        Task<bool> ApproveAsync(
            Guid repairEstimateId,
            Guid approvedBy,
            ApproveRepairEstimateRequest request);

        Task<bool> RejectAsync(
            Guid repairEstimateId,
            Guid rejectedBy,
            RejectRepairEstimateRequest request);
    }
}