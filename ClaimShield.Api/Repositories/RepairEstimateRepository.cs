using ClaimShield.Api.Data.Context;
using ClaimShield.Api.Interfaces.Repositories;
using ClaimShield.Api.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace ClaimShield.Api.Repositories
{
    public class RepairEstimateRepository : IRepairEstimateRepository
    {
        private readonly ClaimShieldDbContext _context;

        public RepairEstimateRepository(
            ClaimShieldDbContext context)
        {
            _context = context;
        }

        // =========================================================
        // GET ALL REPAIR ESTIMATES
        // =========================================================

        public async Task<IEnumerable<RepairEstimate>> GetAllAsync()
        {
            return await _context.RepairEstimates
                .OrderByDescending(x => x.SubmittedDate)
                .ToListAsync();
        }

        // =========================================================
        // GET REPAIR ESTIMATE BY ID
        // =========================================================

        public async Task<RepairEstimate?> GetByIdAsync(
            Guid repairEstimateId)
        {
            return await _context.RepairEstimates
                .FirstOrDefaultAsync(
                    x => x.RepairEstimateId == repairEstimateId);
        }

        // =========================================================
        // GET REPAIR ESTIMATES BY CLAIM
        // =========================================================

        public async Task<IEnumerable<RepairEstimate>> GetByClaimAsync(
            Guid claimId)
        {
            return await _context.RepairEstimates
                .Where(x => x.ClaimId == claimId)
                .OrderByDescending(x => x.SubmittedDate)
                .ToListAsync();
        }

        // =========================================================
        // GET REPAIR ESTIMATES BY ASSIGNMENT
        // =========================================================

        public async Task<IEnumerable<RepairEstimate>> GetByAssignmentAsync(
            Guid repairAssignmentId)
        {
            return await _context.RepairEstimates
                .Where(x =>
                    x.RepairAssignmentId == repairAssignmentId)
                .OrderByDescending(x => x.SubmittedDate)
                .ToListAsync();
        }

        // =========================================================
        // CREATE
        // =========================================================

        public async Task AddAsync(
            RepairEstimate repairEstimate)
        {
            await _context.RepairEstimates.AddAsync(
                repairEstimate);

            await _context.SaveChangesAsync();
        }

        // =========================================================
        // UPDATE
        // =========================================================

        public async Task UpdateAsync(
            RepairEstimate repairEstimate)
        {
            _context.RepairEstimates.Update(
                repairEstimate);

            await _context.SaveChangesAsync();
        }

        // =========================================================
        // DELETE
        // =========================================================

        public async Task DeleteAsync(
            Guid repairEstimateId)
        {
            var repairEstimate =
                await _context.RepairEstimates.FindAsync(
                    repairEstimateId);

            if (repairEstimate == null)
            {
                return;
            }

            _context.RepairEstimates.Remove(
                repairEstimate);

            await _context.SaveChangesAsync();
        }
    }
}