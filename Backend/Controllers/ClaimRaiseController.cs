using ClaimShield.Api.Authentication;
using ClaimShield.Api.Constants;
using ClaimShield.Api.Interfaces.Services;
using ClaimShield.Api.Models.DTOs.ClaimRaise;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ClaimShield.Api.Controllers
{
    // Backs the Customer Raise Claim wizard (Phase 12). Every action
    // requires the caller to be the Customer who owns the claim -
    // enforced inside IClaimRaiseService, not just by role.
    [ApiController]
    [Authorize(Roles = RoleConstants.Customer)]
    public class ClaimRaiseController : ControllerBase
    {
        private readonly IClaimRaiseService _claimRaiseService;
        private readonly ICurrentUserService _currentUserService;

        public ClaimRaiseController(
            IClaimRaiseService claimRaiseService,
            ICurrentUserService currentUserService)
        {
            _claimRaiseService = claimRaiseService;
            _currentUserService = currentUserService;
        }

        private bool TryGetUserId(out Guid userId)
        {
            userId = _currentUserService.UserId ?? Guid.Empty;
            return _currentUserService.UserId.HasValue;
        }

        [HttpPost("api/Claims/raise/step1")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Step1(
            [FromBody] RaiseStep1Request request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            if (!TryGetUserId(out var userId))
            {
                return BadRequest(new { Success = false, Message = "Unable to determine the logged-in user." });
            }

            var (success, error, result) =
                await _claimRaiseService.Step1Async(userId, request);

            if (!success)
            {
                return BadRequest(new { Success = false, Message = error });
            }

            return Ok(result);
        }

        [HttpPut("api/Claims/{claimId:guid}/raise/step2")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Step2(
            Guid claimId,
            [FromBody] RaiseStep2Request request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            if (!TryGetUserId(out var userId))
            {
                return BadRequest(new { Success = false, Message = "Unable to determine the logged-in user." });
            }

            var (success, error, result) =
                await _claimRaiseService.Step2Async(claimId, userId, request);

            if (!success)
            {
                return error == "Claim not found."
                    ? NotFound(new { Success = false, Message = error })
                    : BadRequest(new { Success = false, Message = error });
            }

            return Ok(result);
        }

        [HttpPost("api/Claims/{claimId:guid}/raise/estimate")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Estimate(
            Guid claimId)
        {
            if (!TryGetUserId(out var userId))
            {
                return BadRequest(new { Success = false, Message = "Unable to determine the logged-in user." });
            }

            var (success, error, result) =
                await _claimRaiseService.GenerateEstimateAsync(claimId, userId);

            if (!success)
            {
                return error == "Claim not found."
                    ? NotFound(new { Success = false, Message = error })
                    : BadRequest(new { Success = false, Message = error });
            }

            if (result!.Eligible)
            {
                return Ok(new
                {
                    Eligible = true,
                    result.Estimate!.ClaimId,
                    result.Estimate.LineItems,
                    result.Estimate.NetAssessmentAmount,
                    result.Estimate.RuleSetVersion,
                    result.Estimate.GeneratedAt,
                    result.Estimate.CustomerDecision
                });
            }

            return Ok(new { Eligible = false, Reason = result.Reason });
        }

        [HttpPost("api/Claims/{claimId:guid}/instant-claim/accept")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Accept(
            Guid claimId)
        {
            if (!TryGetUserId(out var userId))
            {
                return BadRequest(new { Success = false, Message = "Unable to determine the logged-in user." });
            }

            var (success, error, result) =
                await _claimRaiseService.AcceptAsync(claimId, userId);

            if (!success)
            {
                return error == "Claim not found."
                    ? NotFound(new { Success = false, Message = error })
                    : BadRequest(new { Success = false, Message = error });
            }

            return Ok(result);
        }

        [HttpPost("api/Claims/{claimId:guid}/instant-claim/decline")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Decline(
            Guid claimId)
        {
            if (!TryGetUserId(out var userId))
            {
                return BadRequest(new { Success = false, Message = "Unable to determine the logged-in user." });
            }

            var (success, error, result) =
                await _claimRaiseService.DeclineAsync(claimId, userId);

            if (!success)
            {
                return error == "Claim not found."
                    ? NotFound(new { Success = false, Message = error })
                    : BadRequest(new { Success = false, Message = error });
            }

            return Ok(result);
        }
    }
}
