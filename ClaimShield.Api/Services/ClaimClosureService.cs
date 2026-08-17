using ClaimShield.Api.Data.Context;
using ClaimShield.Api.Interfaces.Services;
using ClaimShield.Api.Models.DTOs.Claims;
using Microsoft.EntityFrameworkCore;

namespace ClaimShield.Api.Services
{
    public class ClaimClosureService : IClaimClosureService
    {
        private readonly ClaimShieldDbContext _context;

        public ClaimClosureService(
            ClaimShieldDbContext context)
        {
            _context = context;
        }

        // =========================================================
        // CLOSE CLAIM
        // =========================================================

        public async Task<bool> CloseClaimAsync(
            Guid claimId,
            CloseClaimRequest request)
        {
            // -----------------------------------------------------
            // Find claim
            // -----------------------------------------------------

            var claim = await _context.Claims
                .FirstOrDefaultAsync(
                    x => x.ClaimId == claimId);

            if (claim == null)
            {
                return false;
            }

            // -----------------------------------------------------
            // 9 = Settled
            //
            // Only a Settled claim can be closed.
            // -----------------------------------------------------

            if (claim.StatusId != 9)
            {
                return false;
            }

            // -----------------------------------------------------
            // 10 = Closed
            // -----------------------------------------------------

            claim.StatusId = 10;

            // -----------------------------------------------------
            // Update remarks if supplied
            // -----------------------------------------------------

            if (!string.IsNullOrWhiteSpace(request.Remarks))
            {
                // Claims table may not have a Remarks column,
                // so closure remarks are intentionally not stored
                // here unless the Claims entity/table supports one.
            }

            // -----------------------------------------------------
            // Updated date
            // -----------------------------------------------------

            claim.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return true;
        }
    }
}