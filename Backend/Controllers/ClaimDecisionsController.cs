using ClaimShield.Api.Authentication;
using ClaimShield.Api.Constants;
using ClaimShield.Api.Interfaces.Services;
using ClaimShield.Api.Models.DTOs.ClaimDecisions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ClaimShield.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ClaimDecisionsController : ControllerBase
    {
        private readonly IClaimDecisionService _claimDecisionService;
        private readonly ICurrentUserService _currentUserService;

        public ClaimDecisionsController(
            IClaimDecisionService claimDecisionService,
            ICurrentUserService currentUserService)
        {
            _claimDecisionService = claimDecisionService;
            _currentUserService = currentUserService;
        }

        // =========================================================
        // ACCESS HELPERS
        // =========================================================

        private bool IsAdmin =>
            string.Equals(
                _currentUserService.RoleName,
                RoleConstants.Admin,
                StringComparison.OrdinalIgnoreCase);

        private bool IsApprover =>
            string.Equals(
                _currentUserService.RoleName,
                RoleConstants.Approver,
                StringComparison.OrdinalIgnoreCase);

        private bool IsSurveyor =>
            string.Equals(
                _currentUserService.RoleName,
                RoleConstants.Surveyor,
                StringComparison.OrdinalIgnoreCase);

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
        // MY QUEUE
        // GET: api/ClaimDecisions/my-queue
        // =========================================================

        [HttpGet("my-queue")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetMyQueue()
        {
            if (!_currentUserService.UserId.HasValue)
            {
                return Forbidden(
                    "Unable to determine the logged-in user.");
            }

            var roleId =
                IsAdmin
                    ? RoleConstants.AdminId
                    : IsSurveyor
                        ? RoleConstants.SurveyorId
                        : IsApprover
                            ? RoleConstants.ApproverId
                            : 0;

            if (roleId == 0)
            {
                return Forbidden(
                    "Only a Surveyor, Approver, or Admin has a decision queue.");
            }

            var queue =
                await _claimDecisionService.GetMyQueueAsync(
                    _currentUserService.UserId.Value,
                    roleId);

            return Ok(queue);
        }

        // =========================================================
        // LATEST DECISION FOR A CLAIM
        // GET: api/ClaimDecisions/claim/{claimId}
        // =========================================================

        [HttpGet("claim/{claimId:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetLatest(
            Guid claimId)
        {
            if (!IsAdmin && !IsApprover && !IsSurveyor)
            {
                return Forbidden(
                    "You are not authorized to view claim decisions.");
            }

            var decision =
                await _claimDecisionService.GetLatestDecisionAsync(
                    claimId);

            if (decision == null)
            {
                return NotFound(new
                {
                    Success = false,
                    Message = "No decision has been recorded for this claim."
                });
            }

            return Ok(decision);
        }

        // =========================================================
        // DECISION HISTORY FOR A CLAIM
        // GET: api/ClaimDecisions/claim/{claimId}/history
        // =========================================================

        [HttpGet("claim/{claimId:guid}/history")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetHistory(
            Guid claimId)
        {
            if (!IsAdmin && !IsApprover)
            {
                return Forbidden(
                    "Only an Approver or Admin can view the full decision history.");
            }

            var history =
                await _claimDecisionService.GetHistoryAsync(
                    claimId);

            return Ok(history);
        }

        // =========================================================
        // SURVEYOR DECISION (maker)
        // POST: api/ClaimDecisions/{claimId}/surveyor-decision
        // =========================================================

        [HttpPost("{claimId:guid}/surveyor-decision")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> SurveyorDecision(
            Guid claimId,
            [FromBody] SurveyorDecisionRequest request)
        {
            if (!IsSurveyor)
            {
                return Forbidden(
                    "Only a Surveyor can record this decision.");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (!_currentUserService.UserId.HasValue)
            {
                return Forbidden(
                    "Unable to determine the logged-in Surveyor.");
            }

            var result =
                await _claimDecisionService.RecordSurveyorDecisionAsync(
                    claimId,
                    _currentUserService.UserId.Value,
                    request);

            if (!result.Success)
            {
                return BadRequest(new
                {
                    Success = false,
                    Message = result.ErrorMessage
                });
            }

            return Ok(new
            {
                Success = true,

                Message =
                    result.Escalated
                        ? "Decision recorded and escalated to an Approver for review."
                        : "Decision recorded and finalized within your authority limits.",

                Escalated = result.Escalated,

                ClaimStatusId = result.UpdatedClaimStatusId,

                Decision = result.Decision
            });
        }

        // =========================================================
        // APPROVER DECISION (checker)
        // POST: api/ClaimDecisions/{claimId}/approver-decision
        // =========================================================

        [HttpPost("{claimId:guid}/approver-decision")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> ApproverDecision(
            Guid claimId,
            [FromBody] ApproverDecisionRequest request)
        {
            if (!IsApprover && !IsAdmin)
            {
                return Forbidden(
                    "Only an Approver or Admin can record this decision.");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (!_currentUserService.UserId.HasValue)
            {
                return Forbidden(
                    "Unable to determine the logged-in Approver.");
            }

            var result =
                await _claimDecisionService.RecordApproverDecisionAsync(
                    claimId,
                    _currentUserService.UserId.Value,
                    request);

            if (!result.Success)
            {
                return BadRequest(new
                {
                    Success = false,
                    Message = result.ErrorMessage
                });
            }

            return Ok(new
            {
                Success = true,

                Message = "Approver decision recorded and finalized.",

                ClaimStatusId = result.UpdatedClaimStatusId,

                Decision = result.Decision
            });
        }
    }
}
