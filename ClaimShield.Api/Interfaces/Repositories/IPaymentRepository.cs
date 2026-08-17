using ClaimShield.Api.Models.Entities;

namespace ClaimShield.Api.Interfaces.Repositories
{
    public interface IPaymentRepository
    {
        Task<IEnumerable<Payment>> GetAllAsync();

        Task<Payment?> GetByIdAsync(
            Guid paymentId);

        Task<IEnumerable<Payment>> GetByClaimAsync(
            Guid claimId);

        Task AddAsync(
            Payment payment);

        Task UpdateAsync(
            Payment payment);

        Task DeleteAsync(
            Guid paymentId);
    }
}