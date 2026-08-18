using ClaimShield.Api.Data.Context;
using ClaimShield.Api.Interfaces.Repositories;
using ClaimShield.Api.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace ClaimShield.Api.Repositories
{
    public class PolicyRepository : IPolicyRepository
    {
        private readonly ClaimShieldDbContext _context;

        public PolicyRepository(ClaimShieldDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Policy>> GetAllAsync()
        {
            return await _context.Policies
                .OrderBy(p => p.PolicyNumber)
                .ToListAsync();
        }

        public async Task<Policy?> GetByIdAsync(Guid policyId)
        {
            return await _context.Policies
                .FirstOrDefaultAsync(p => p.PolicyId == policyId);
        }

        public async Task<IEnumerable<Policy>> GetByCustomerAsync(Guid customerId)
        {
            return await _context.Policies
                .Where(p => p.CustomerId == customerId)
                .ToListAsync();
        }

        public async Task AddAsync(Policy policy)
        {
            await _context.Policies.AddAsync(policy);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Policy policy)
        {
            _context.Policies.Update(policy);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid policyId)
        {
            var policy = await _context.Policies.FindAsync(policyId);

            if (policy != null)
            {
                _context.Policies.Remove(policy);
                await _context.SaveChangesAsync();
            }
        }
    }
}