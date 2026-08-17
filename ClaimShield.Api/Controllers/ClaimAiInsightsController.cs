using ClaimShield.Api.Authentication;
using ClaimShield.Api.Constants;
using ClaimShield.Api.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ClaimShield.Api.Controllers
{
    // =================================================================
    // Backward-compatible alias for the pre-Phase-9 "ai-insights" route.
    // Delegates entirely to IClaimScoringService.GetScoringForUserAsync -
    // the exact same role-branching method ClaimsController's
    // /scoring-results endpoint uses - so the two routes can never
    // enforce visibility differently.
    // =================================================================

    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ClaimAiInsightsController : ControllerBase
    {
        private readonly IClaimScoringService _claimScoringService;
        private readonly ICurrentUserService _currentUserService;

        public ClaimAiInsightsController(
            IClaimScoringService claimScoringService,
            ICurrentUserService currentUserService)
        {
            _claimScoringService = claimScoringService;
            _currentUserService = currentUserService;
        }

        private int CurrentRoleId =>
            _currentUserService.RoleName switch
            {
                RoleConstants.Customer => RoleConstants.CustomerId,
                RoleConstants.Repairer => RoleConstants.RepairerId,
                RoleConstants.Surveyor => RoleConstants.SurveyorId,
                RoleConstants.Approver => RoleConstants.ApproverId,
                RoleConstants.Admin => RoleConstants.AdminId,
                _ => 0
            };

        private static IActionResult Forbidden(
            string message)
        {
            return new ObjectResult(new
            {
                Success = false,
                Message = message
            })
            {
                StatusCode = StatusCodes.Status403Forbidden
            };
        }

        // =========================================================
        // GET SCORING RESULTS FOR A CLAIM
        // GET: api/ClaimAiInsights/claim/{claimId}
        // =========================================================

        [HttpGet("claim/{claimId:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetByClaim(
            Guid claimId)
        {
            if (!_currentUserService.UserId.HasValue)
            {
                return Forbidden(
                    "Unable to determine the logged-in user.");
            }

            var access =
                await _claimScoringService.GetScoringForUserAsync(
                    claimId,
                    _currentUserService.UserId.Value,
                    CurrentRoleId);

            if (!access.ClaimFound)
            {
                return NotFound(new
                {
                    Success = false,
                    Message = "Claim not found."
                });
            }

            if (!access.Authorized)
            {
                return Forbidden(
                    "You are not authorized to view scoring results for this claim.");
            }

            if (access.CustomerView != null)
            {
                return Ok(access.CustomerView);
            }

            if (access.InternalView != null)
            {
                return Ok(access.InternalView);
            }

            return NotFound(new
            {
                Success = false,
                Message = "No scoring results are available for this claim yet."
            });
        }
    }
}
