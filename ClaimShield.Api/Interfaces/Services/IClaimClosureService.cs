using ClaimShield.Api.Models.DTOs.Claims;

namespace ClaimShield.Api.Interfaces.Services
{
    public interface IClaimClosureService
    {
        Task<bool> CloseClaimAsync(
            Guid claimId,
            CloseClaimRequest request);
    }
}