using ClaimShield.Api.Data.Context;
using ClaimShield.Api.Interfaces.Repositories;
using ClaimShield.Api.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace ClaimShield.Api.Repositories
{
    public class PaymentRepository : IPaymentRepository
    {
        private readonly ClaimShieldDbContext _context;

        public PaymentRepository(
            ClaimShieldDbContext context)
        {
            _context = context;
        }

        // =========================================================
        // GET ALL PAYMENTS
        // =========================================================

        public async Task<IEnumerable<Payment>> GetAllAsync()
        {
            return await _context.Payments
                .OrderByDescending(x => x.CreatedDate)
                .ToListAsync();
        }

        // =========================================================
        // GET PAYMENT BY ID
        // =========================================================

        public async Task<Payment?> GetByIdAsync(
            Guid paymentId)
        {
            return await _context.Payments
                .FirstOrDefaultAsync(
                    x => x.PaymentId == paymentId);
        }

        // =========================================================
        // GET PAYMENTS BY CLAIM
        // =========================================================

        public async Task<IEnumerable<Payment>> GetByClaimAsync(
            Guid claimId)
        {
            return await _context.Payments
                .Where(x => x.ClaimId == claimId)
                .OrderByDescending(x => x.CreatedDate)
                .ToListAsync();
        }

        // =========================================================
        // CREATE PAYMENT
        // =========================================================

        public async Task AddAsync(
            Payment payment)
        {
            await _context.Payments.AddAsync(payment);

            await _context.SaveChangesAsync();
        }

        // =========================================================
        // UPDATE PAYMENT
        // =========================================================

        public async Task UpdateAsync(
            Payment payment)
        {
            _context.Payments.Update(payment);

            await _context.SaveChangesAsync();
        }

        // =========================================================
        // DELETE PAYMENT
        // =========================================================

        public async Task DeleteAsync(
            Guid paymentId)
        {
            var payment =
                await _context.Payments.FindAsync(paymentId);

            if (payment == null)
            {
                return;
            }

            _context.Payments.Remove(payment);

            await _context.SaveChangesAsync();
        }
    }
}