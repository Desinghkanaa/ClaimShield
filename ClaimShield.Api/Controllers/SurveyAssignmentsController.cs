using ClaimShield.Api.Authentication;
using ClaimShield.Api.Constants;
using ClaimShield.Api.Interfaces.Services;
using ClaimShield.Api.Models.DTOs.SurveyAssignments;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ClaimShield.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class SurveyAssignmentsController : ControllerBase
    {
        private readonly ISurveyAssignmentService _surveyAssignmentService;
        private readonly ICurrentUserService _currentUserService;

        public SurveyAssignmentsController(
            ISurveyAssignmentService surveyAssignmentService,
            ICurrentUserService currentUserService)
        {
            _surveyAssignmentService = surveyAssignmentService;
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
        // GET ALL SURVEY ASSIGNMENTS
        // GET: api/SurveyAssignments
        // =========================================================

        [HttpGet]
        [ProducesResponseType(
            typeof(IEnumerable<SurveyAssignmentResponseDto>),
            StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetAll()
        {
            if (!IsAdmin)
            {
                return Forbidden(
                    "Only an Admin can list all survey assignments.");
            }

            var assignments =
                await _surveyAssignmentService.GetAllAsync();

            return Ok(assignments);
        }

        // =========================================================
        // GET SURVEY ASSIGNMENT BY ID
        // GET: api/SurveyAssignments/{id}
        // =========================================================

        [HttpGet("{id:guid}")]
        [ProducesResponseType(
            typeof(SurveyAssignmentResponseDto),
            StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetById(Guid id)
        {
            var assignment =
                await _surveyAssignmentService.GetByIdAsync(id);

            if (assignment == null)
            {
                return NotFound(new
                {
                    Success = false,
                    Message = "Survey assignment not found."
                });
            }

            if (!IsAdmin &&
                assignment.SurveyorId != _currentUserService.UserId)
            {
                return Forbidden(
                    "You are not authorized to view this survey assignment.");
            }

            return Ok(assignment);
        }

        // =========================================================
        // GET ASSIGNMENTS BY CLAIM
        // GET: api/SurveyAssignments/claim/{claimId}
        // =========================================================

        [HttpGet("claim/{claimId:guid}")]
        [ProducesResponseType(
            typeof(IEnumerable<SurveyAssignmentResponseDto>),
            StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetByClaim(
            Guid claimId)
        {
            if (!IsAdmin &&
                !IsSurveyor)
            {
                return Forbidden(
                    "You are not authorized to view survey assignments for this claim.");
            }

            var assignments =
                await _surveyAssignmentService.GetByClaimAsync(
                    claimId);

            if (!IsAdmin)
            {
                assignments =
                    assignments.Where(
                        x =>
                            x.SurveyorId ==
                            _currentUserService.UserId);
            }

            return Ok(assignments);
        }

        // =========================================================
        // GET ASSIGNMENTS BY SURVEYOR
        // GET: api/SurveyAssignments/surveyor/{surveyorId}
        // =========================================================

        [HttpGet("surveyor/{surveyorId:guid}")]
        [ProducesResponseType(
            typeof(IEnumerable<SurveyAssignmentResponseDto>),
            StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetBySurveyor(
            Guid surveyorId)
        {
            if (!IsAdmin &&
                (!IsSurveyor ||
                 surveyorId != _currentUserService.UserId))
            {
                return Forbidden(
                    "You are not authorized to view another surveyor's assignments.");
            }

            var assignments =
                await _surveyAssignmentService.GetBySurveyorAsync(
                    surveyorId);

            return Ok(assignments);
        }

        // =========================================================
        // CREATE SURVEY ASSIGNMENT
        // POST: api/SurveyAssignments
        // =========================================================

        [HttpPost]
        [ProducesResponseType(
            typeof(SurveyAssignmentResponseDto),
            StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Create(
            CreateSurveyAssignmentRequest request)
        {
            if (!IsAdmin)
            {
                return Forbidden(
                    "Only an Admin can create a survey assignment.");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // -----------------------------------------------------
            // AssignedBy must be the authenticated Admin, not
            // whatever the client sent in the request body.
            // -----------------------------------------------------

            if (_currentUserService.UserId.HasValue)
            {
                request.AssignedBy =
                    _currentUserService.UserId.Value;
            }

            var assignment =
                await _surveyAssignmentService.CreateAsync(
                    request);

            return CreatedAtAction(
                nameof(GetById),
                new
                {
                    id = assignment.SurveyAssignmentId
                },
                assignment);
        }

        // =========================================================
        // UPDATE SURVEY ASSIGNMENT
        // PUT: api/SurveyAssignments
        // =========================================================

        [HttpPut]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Update(
            UpdateSurveyAssignmentRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var existing =
                await _surveyAssignmentService.GetByIdAsync(
                    request.SurveyAssignmentId);

            if (existing == null)
            {
                return NotFound(new
                {
                    Success = false,
                    Message = "Survey assignment not found."
                });
            }

            if (!IsAdmin &&
                existing.SurveyorId != _currentUserService.UserId)
            {
                return Forbidden(
                    "You are not authorized to update this survey assignment.");
            }

            var updated =
                await _surveyAssignmentService.UpdateAsync(
                    request);

            if (!updated)
            {
                return NotFound(new
                {
                    Success = false,
                    Message = "Survey assignment not found."
                });
            }

            return Ok(new
            {
                Success = true,
                Message =
                    "Survey assignment updated successfully."
            });
        }

        // =========================================================
        // DELETE SURVEY ASSIGNMENT
        // DELETE: api/SurveyAssignments/{id}
        // =========================================================

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Delete(Guid id)
        {
            if (!IsAdmin)
            {
                return Forbidden(
                    "Only an Admin can delete a survey assignment.");
            }

            var deleted =
                await _surveyAssignmentService.DeleteAsync(id);

            if (!deleted)
            {
                return NotFound(new
                {
                    Success = false,
                    Message = "Survey assignment not found."
                });
            }

            return Ok(new
            {
                Success = true,
                Message =
                    "Survey assignment deleted successfully."
            });
        }
    }
}
