using ClaimShield.Api.Constants;
using ClaimShield.Api.Interfaces.Services;
using ClaimShield.Api.Models.DTOs.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ClaimShield.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = $"{RoleConstants.Approver},{RoleConstants.Admin}")]
    public class ClaimApprovalController : ControllerBase
    {
        private readonly IClaimApprovalService _claimApprovalService;

        public ClaimApprovalController(
            IClaimApprovalService claimApprovalService)
        {
            _claimApprovalService = claimApprovalService;
        }

        // =========================================================
        // APPROVE CLAIM
        // POST: api/ClaimApproval/{claimId}/approve
        // =========================================================

        [HttpPost("{claimId:guid}/approve")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> ApproveClaim(
            Guid claimId,
            [FromBody] ApproveClaimRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var approved =
                await _claimApprovalService.ApproveClaimAsync(
                    claimId,
                    request);

            if (!approved)
            {
                return BadRequest(new
                {
                    Success = false,
                    Message =
                        "Claim could not be approved. " +
                        "It may not exist, may already be approved, " +
                        "or may already be rejected/closed."
                });
            }

            return Ok(new
            {
                Success = true,
                Message = "Claim approved successfully.",
                ClaimId = claimId,
                ApprovedAmount = request.ApprovedAmount,
                StatusId = ClaimStatusConstants.Approved,
                Status = "Approved"
            });
        }

        // =========================================================
        // REJECT CLAIM
        // POST: api/ClaimApproval/{claimId}/reject
        // =========================================================

        [HttpPost("{claimId:guid}/reject")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> RejectClaim(
            Guid claimId,
            [FromBody] RejectClaimRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var rejected =
                await _claimApprovalService.RejectClaimAsync(
                    claimId,
                    request);

            if (!rejected)
            {
                return BadRequest(new
                {
                    Success = false,
                    Message =
                        "Claim could not be rejected. " +
                        "It may not exist, may already be approved, " +
                        "or may already be rejected."
                });
            }

            return Ok(new
            {
                Success = true,
                Message = "Claim rejected successfully.",
                ClaimId = claimId,
                StatusId = ClaimStatusConstants.Rejected,
                Status = "Rejected"
            });
        }
    }
}