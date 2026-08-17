using ClaimShield.Api.Models.DTOs.Claims;

namespace ClaimShield.Api.Interfaces.Services
{
    public interface IClaimService
    {
        Task<IEnumerable<ClaimResponseDto>> GetAllClaimsAsync();

        Task<ClaimResponseDto?> GetClaimByIdAsync(Guid claimId);

        Task<IEnumerable<ClaimResponseDto>> GetClaimsByCustomerAsync(Guid customerId);

        Task<IEnumerable<ClaimResponseDto>> GetClaimsByPolicyAsync(Guid policyId);

        Task<IEnumerable<ClaimResponseDto>> GetClaimsByVehicleAsync(Guid vehicleId);

        Task<ClaimResponseDto> CreateClaimAsync(CreateClaimRequest request);

        Task<bool> UpdateClaimAsync(UpdateClaimRequest request);

        Task<bool> DeleteClaimAsync(Guid claimId);
    }
}