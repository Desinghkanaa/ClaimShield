using ClaimShield.Api.AI.Models;
using ClaimShield.Api.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClaimShield.Api.Controllers
{
    [ApiController]
    [Route("api/Ai/actions")]
    [Authorize]
    public class AiActionsController : ControllerBase
    {
        private readonly IClaimClosureService _claimClosureService;
        private readonly IClaimService _claimService;

        public AiActionsController(
            IClaimClosureService claimClosureService,
            IClaimService claimService)
        {
            _claimClosureService = claimClosureService;
            _claimService = claimService;
        }

        // =====================================================
        // REQUEST CLAIM CLOSURE
        // =====================================================

        [HttpPost("close-claim/request")]
        public async Task<IActionResult> RequestClaimClosure(
            [FromBody] AiActionRequest request)
        {
            if (request.ClaimId == Guid.Empty)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Claim ID is required."
                });
            }

            var claim =
                await _claimService.GetClaimByIdAsync(
                    request.ClaimId);

            if (claim == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Claim not found."
                });
            }

            return Ok(new AiActionResponse
            {
                Success = true,

                RequiresConfirmation = true,

                Message =
                    $"Your claim {claim.ClaimNumber} is currently in status {claim.StatusId}. " +
                    "Closing the claim will change its status to Closed. " +
                    "Please explicitly confirm if you want to proceed.",

                Intent = "CLOSE_CLAIM",

                Action = "CLOSE_CLAIM",

                ClaimId = request.ClaimId
            });
        }

        // =====================================================
        // CONFIRM CLAIM CLOSURE
        // =====================================================

        [HttpPost("close-claim/confirm")]
        public async Task<IActionResult> ConfirmClaimClosure(
            [FromBody] AiActionRequest request)
        {
            if (request.ClaimId == Guid.Empty)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Claim ID is required."
                });
            }

            if (!request.Confirmed)
            {
                return BadRequest(new
                {
                    success = false,
                    message =
                        "Claim closure was not confirmed."
                });
            }

            var claim =
                await _claimService.GetClaimByIdAsync(
                    request.ClaimId);

            if (claim == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Claim not found."
                });
            }

            // -------------------------------------------------
            // Only Settled claims can be closed.
            // -------------------------------------------------

            if (claim.StatusId != 9)
            {
                return BadRequest(new
                {
                    success = false,
                    message =
                        "Only a Settled claim can be closed."
                });
            }

            var closeRequest =
                new ClaimShield.Api.Models.DTOs.Claims
                    .CloseClaimRequest
                {
                    Remarks =
                        "Claim closure confirmed through ClaimShield AI."
                };

            var closed =
                await _claimClosureService.CloseClaimAsync(
                    request.ClaimId,
                    closeRequest);

            if (!closed)
            {
                return BadRequest(new
                {
                    success = false,
                    message =
                        "Claim could not be closed."
                });
            }

            return Ok(new
            {
                success = true,

                message =
                    "Claim closed successfully after explicit confirmation.",

                claimId =
                    request.ClaimId,

                statusId = 10,

                status = "Closed"
            });
        }
    }

    // =========================================================
    // ACTION REQUEST
    // =========================================================

    public class AiActionRequest
    {
        public Guid ClaimId { get; set; }

        public bool Confirmed { get; set; }
    }
}