using ClaimShield.Api.Data.Context;
using ClaimShield.Api.Interfaces.Repositories;
using ClaimShield.Api.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace ClaimShield.Api.Repositories
{
    public class RepairAssignmentRepository : IRepairAssignmentRepository
    {
        private readonly ClaimShieldDbContext _context;

        public RepairAssignmentRepository(
            ClaimShieldDbContext context)
        {
            _context = context;
        }

        // =========================================================
        // GET ALL REPAIR ASSIGNMENTS
        // =========================================================

        public async Task<IEnumerable<RepairAssignment>> GetAllAsync()
        {
            return await _context.RepairAssignments
                .OrderByDescending(x => x.AssignedDate)
                .ToListAsync();
        }

        // =========================================================
        // GET REPAIR ASSIGNMENT BY ID
        // =========================================================

        public async Task<RepairAssignment?> GetByIdAsync(
            Guid repairAssignmentId)
        {
            return await _context.RepairAssignments
                .FirstOrDefaultAsync(
                    x => x.RepairAssignmentId == repairAssignmentId);
        }

        // =========================================================
        // GET REPAIR ASSIGNMENTS BY CLAIM
        // =========================================================

        public async Task<IEnumerable<RepairAssignment>> GetByClaimAsync(
            Guid claimId)
        {
            return await _context.RepairAssignments
                .Where(x => x.ClaimId == claimId)
                .OrderByDescending(x => x.AssignedDate)
                .ToListAsync();
        }

        // =========================================================
        // GET REPAIR ASSIGNMENTS BY REPAIRER
        // =========================================================

        public async Task<IEnumerable<RepairAssignment>> GetByRepairerAsync(
            Guid repairerId)
        {
            return await _context.RepairAssignments
                .Where(x => x.RepairerId == repairerId)
                .OrderByDescending(x => x.AssignedDate)
                .ToListAsync();
        }

        // =========================================================
        // CREATE
        // =========================================================

        public async Task AddAsync(
            RepairAssignment repairAssignment)
        {
            await _context.RepairAssignments.AddAsync(
                repairAssignment);

            await _context.SaveChangesAsync();
        }

        // =========================================================
        // UPDATE
        // =========================================================

        public async Task UpdateAsync(
            RepairAssignment repairAssignment)
        {
            _context.RepairAssignments.Update(
                repairAssignment);

            await _context.SaveChangesAsync();
        }

        // =========================================================
        // DELETE
        // =========================================================

        public async Task DeleteAsync(
            Guid repairAssignmentId)
        {
            var repairAssignment =
                await _context.RepairAssignments.FindAsync(
                    repairAssignmentId);

            if (repairAssignment == null)
            {
                return;
            }

            _context.RepairAssignments.Remove(
                repairAssignment);

            await _context.SaveChangesAsync();
        }
    }
}