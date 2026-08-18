using ClaimShield.Api.Data.Context;
using ClaimShield.Api.Interfaces.Repositories;
using ClaimShield.Api.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace ClaimShield.Api.Repositories
{
    public class ClaimDocumentRepository : IClaimDocumentRepository
    {
        private readonly ClaimShieldDbContext _context;

        public ClaimDocumentRepository(
            ClaimShieldDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ClaimDocument>> GetAllAsync()
        {
            return await _context.ClaimDocuments
                .OrderBy(x => x.FileName)
                .ToListAsync();
        }

        public async Task<ClaimDocument?> GetByIdAsync(
            Guid claimDocumentId)
        {
            return await _context.ClaimDocuments
                .FirstOrDefaultAsync(
                    x => x.ClaimDocumentId == claimDocumentId);
        }

        public async Task<IEnumerable<ClaimDocument>> GetByClaimAsync(
            Guid claimId)
        {
            return await _context.ClaimDocuments
                .Where(x => x.ClaimId == claimId)
                .OrderBy(x => x.FileName)
                .ToListAsync();
        }

        public async Task AddAsync(
            ClaimDocument claimDocument)
        {
            await _context.ClaimDocuments.AddAsync(
                claimDocument);

            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(
            ClaimDocument claimDocument)
        {
            _context.ClaimDocuments.Update(
                claimDocument);

            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(
            Guid claimDocumentId)
        {
            var document =
                await _context.ClaimDocuments.FindAsync(
                    claimDocumentId);

            if (document == null)
            {
                return;
            }

            _context.ClaimDocuments.Remove(document);

            await _context.SaveChangesAsync();
        }
    }
}