using ClaimShield.Api.Models.Entities;

namespace ClaimShield.Api.Interfaces.Repositories
{
    public interface IClaimDocumentRepository
    {
        Task<IEnumerable<ClaimDocument>> GetAllAsync();

        Task<ClaimDocument?> GetByIdAsync(
            Guid claimDocumentId);

        Task<IEnumerable<ClaimDocument>> GetByClaimAsync(
            Guid claimId);

        Task AddAsync(
            ClaimDocument claimDocument);

        Task UpdateAsync(
            ClaimDocument claimDocument);

        Task DeleteAsync(
            Guid claimDocumentId);
    }
}