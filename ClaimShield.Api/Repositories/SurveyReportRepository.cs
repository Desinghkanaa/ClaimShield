using ClaimShield.Api.Data.Context;
using ClaimShield.Api.Interfaces.Repositories;
using ClaimShield.Api.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace ClaimShield.Api.Repositories
{
    public class SurveyReportRepository : ISurveyReportRepository
    {
        private readonly ClaimShieldDbContext _context;

        public SurveyReportRepository(ClaimShieldDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<SurveyReport>> GetAllAsync()
        {
            return await _context.SurveyReports
                .OrderByDescending(x => x.InspectionDate)
                .ToListAsync();
        }

        public async Task<SurveyReport?> GetByIdAsync(Guid surveyReportId)
        {
            return await _context.SurveyReports
                .FirstOrDefaultAsync(
                    x => x.SurveyReportId == surveyReportId);
        }

        public async Task<IEnumerable<SurveyReport>> GetByClaimAsync(
            Guid claimId)
        {
            return await _context.SurveyReports
                .Where(x => x.ClaimId == claimId)
                .OrderByDescending(x => x.InspectionDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<SurveyReport>> GetByAssignmentAsync(
            Guid surveyAssignmentId)
        {
            return await _context.SurveyReports
                .Where(x => x.SurveyAssignmentId == surveyAssignmentId)
                .OrderByDescending(x => x.InspectionDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<SurveyReport>> GetBySurveyorAsync(
            Guid surveyorId)
        {
            return await _context.SurveyReports
                .Where(x => x.SurveyorId == surveyorId)
                .OrderByDescending(x => x.InspectionDate)
                .ToListAsync();
        }

        public async Task AddAsync(SurveyReport surveyReport)
        {
            await _context.SurveyReports.AddAsync(surveyReport);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(SurveyReport surveyReport)
        {
            _context.SurveyReports.Update(surveyReport);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid surveyReportId)
        {
            var surveyReport =
                await _context.SurveyReports.FindAsync(surveyReportId);

            if (surveyReport == null)
                return;

            _context.SurveyReports.Remove(surveyReport);

            await _context.SaveChangesAsync();
        }
    }
}