using ClaimShield.Api.Authentication;
using ClaimShield.Api.Constants;
using ClaimShield.Api.Interfaces.Services;
using ClaimShield.Api.Models.DTOs.ReassessmentComments;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ClaimShield.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ReassessmentCommentsController : ControllerBase
    {
        private readonly IReassessmentCommentService _reassessmentCommentService;
        private readonly ICurrentUserService _currentUserService;

        public ReassessmentCommentsController(
            IReassessmentCommentService reassessmentCommentService,
            ICurrentUserService currentUserService)
        {
            _reassessmentCommentService = reassessmentCommentService;
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
        // GET COMMENT THREAD FOR A CLAIM
        // GET: api/ReassessmentComments/claim/{claimId}
        // =========================================================

        [HttpGet("claim/{claimId:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetByClaim(
            Guid claimId)
        {
            if (!IsAdmin && !IsApprover && !IsSurveyor)
            {
                return Forbidden(
                    "You are not authorized to view this comment thread.");
            }

            var comments =
                await _reassessmentCommentService.GetByClaimAsync(
                    claimId);

            return Ok(comments);
        }

        // =========================================================
        // POST A REASSESSMENT RESPONSE
        // POST: api/ReassessmentComments
        // =========================================================

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Create(
            [FromBody] CreateReassessmentCommentRequest request)
        {
            if (!IsAdmin && !IsApprover && !IsSurveyor)
            {
                return Forbidden(
                    "Only a Surveyor, Approver, or Admin can post to this thread.");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (!_currentUserService.UserId.HasValue)
            {
                return Forbidden(
                    "Unable to determine the logged-in user.");
            }

            var (success, error, comment) =
                await _reassessmentCommentService.CreateAsync(
                    _currentUserService.UserId.Value,
                    request);

            if (!success)
            {
                return BadRequest(new
                {
                    Success = false,
                    Message = error
                });
            }

            return CreatedAtAction(
                nameof(GetByClaim),
                new
                {
                    claimId = request.ClaimId
                },
                comment);
        }
    }
}
