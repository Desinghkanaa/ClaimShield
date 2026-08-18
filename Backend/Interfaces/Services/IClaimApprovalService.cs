using ClaimShield.Api.Models.DTOs.Claims;

namespace ClaimShield.Api.Interfaces.Services
{
    public interface IClaimApprovalService
    {
        Task<bool> ApproveClaimAsync(
            Guid claimId,
            ApproveClaimRequest request);

        Task<bool> RejectClaimAsync(
            Guid claimId,
            RejectClaimRequest request);
    }
}