using ClaimShield.Api.Data.Context;
using ClaimShield.Api.Interfaces.Repositories;
using ClaimShield.Api.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace ClaimShield.Api.Repositories
{
    public class SurveyAssignmentRepository : ISurveyAssignmentRepository
    {
        private readonly ClaimShieldDbContext _context;

        public SurveyAssignmentRepository(ClaimShieldDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<SurveyAssignment>> GetAllAsync()
        {
            return await _context.SurveyAssignments
                .OrderByDescending(x => x.AssignedDate)
                .ToListAsync();
        }

        public async Task<SurveyAssignment?> GetByIdAsync(Guid surveyAssignmentId)
        {
            return await _context.SurveyAssignments
                .FirstOrDefaultAsync(
                    x => x.SurveyAssignmentId == surveyAssignmentId);
        }

        public async Task<IEnumerable<SurveyAssignment>> GetByClaimAsync(
            Guid claimId)
        {
            return await _context.SurveyAssignments
                .Where(x => x.ClaimId == claimId)
                .OrderByDescending(x => x.AssignedDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<SurveyAssignment>> GetBySurveyorAsync(
            Guid surveyorId)
        {
            return await _context.SurveyAssignments
                .Where(x => x.SurveyorId == surveyorId)
                .OrderByDescending(x => x.AssignedDate)
                .ToListAsync();
        }

        public async Task AddAsync(SurveyAssignment surveyAssignment)
        {
            await _context.SurveyAssignments.AddAsync(surveyAssignment);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(SurveyAssignment surveyAssignment)
        {
            _context.SurveyAssignments.Update(surveyAssignment);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid surveyAssignmentId)
        {
            var surveyAssignment =
                await _context.SurveyAssignments.FindAsync(surveyAssignmentId);

            if (surveyAssignment == null)
                return;

            _context.SurveyAssignments.Remove(surveyAssignment);

            await _context.SaveChangesAsync();
        }
    }
}