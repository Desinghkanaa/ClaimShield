using ClaimShield.Api.Constants;
using ClaimShield.Api.Data.Context;
using ClaimShield.Api.Interfaces.Services;
using ClaimShield.Api.Models.DTOs.Claims;
using Microsoft.EntityFrameworkCore;

namespace ClaimShield.Api.Services
{
    public class ClaimApprovalService : IClaimApprovalService
    {
        private readonly ClaimShieldDbContext _context;

        public ClaimApprovalService(
            ClaimShieldDbContext context)
        {
            _context = context;
        }

        // =========================================================
        // APPROVE CLAIM
        // =========================================================

        public async Task<bool> ApproveClaimAsync(
            Guid claimId,
            ApproveClaimRequest request)
        {
            var claim = await _context.Claims
                .FirstOrDefaultAsync(
                    x => x.ClaimId == claimId);

            if (claim == null)
            {
                return false;
            }

            // -----------------------------------------------------
            // Prevent approval of an already approved claim
            // -----------------------------------------------------

            if (claim.StatusId == ClaimStatusConstants.Approved)
            {
                return false;
            }

            // -----------------------------------------------------
            // Prevent approval of a rejected or closed claim
            // -----------------------------------------------------

            if (claim.StatusId == ClaimStatusConstants.Rejected ||
                claim.StatusId == ClaimStatusConstants.Closed)
            {
                return false;
            }

            // -----------------------------------------------------
            // Set approved amount
            // -----------------------------------------------------

            claim.ApprovedAmount =
                request.ApprovedAmount;

            claim.StatusId = ClaimStatusConstants.Approved;

            // -----------------------------------------------------
            // Decision reasoning (previously accepted but discarded)
            // -----------------------------------------------------

            claim.DecisionRemarks =
                request.Remarks;

            // -----------------------------------------------------
            // Update date
            // -----------------------------------------------------

            claim.UpdatedDate =
                DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return true;
        }

        // =========================================================
        // REJECT CLAIM
        // =========================================================

        public async Task<bool> RejectClaimAsync(
            Guid claimId,
            RejectClaimRequest request)
        {
            var claim = await _context.Claims
                .FirstOrDefaultAsync(
                    x => x.ClaimId == claimId);

            if (claim == null)
            {
                return false;
            }

            // -----------------------------------------------------
            // Prevent rejection of an already approved claim
            // -----------------------------------------------------

            if (claim.StatusId == ClaimStatusConstants.Approved)
            {
                return false;
            }

            // -----------------------------------------------------
            // Prevent rejection of an already rejected claim
            // -----------------------------------------------------

            if (claim.StatusId == ClaimStatusConstants.Rejected)
            {
                return false;
            }

            claim.StatusId = ClaimStatusConstants.Rejected;

            // -----------------------------------------------------
            // Rejected claims should not have an approved amount
            // -----------------------------------------------------

            claim.ApprovedAmount = null;

            // -----------------------------------------------------
            // Rejection reason (previously accepted but discarded)
            // -----------------------------------------------------

            claim.DecisionRemarks =
                request.Remarks;

            // -----------------------------------------------------
            // Update date
            // -----------------------------------------------------

            claim.UpdatedDate =
                DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return true;
        }
    }
}