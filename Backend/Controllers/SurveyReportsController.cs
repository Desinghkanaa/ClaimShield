using ClaimShield.Api.Authentication;
using ClaimShield.Api.Constants;
using ClaimShield.Api.Interfaces.Repositories;
using ClaimShield.Api.Interfaces.Services;
using ClaimShield.Api.Models.DTOs.SurveyReports;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ClaimShield.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class SurveyReportsController : ControllerBase
    {
        private readonly ISurveyReportService _surveyReportService;
        private readonly ICurrentUserService _currentUserService;
        private readonly IClaimRepository _claimRepository;
        private readonly IClaimDecisionService _claimDecisionService;
        private readonly ISurveyAssignmentRepository _surveyAssignmentRepository;

        public SurveyReportsController(
            ISurveyReportService surveyReportService,
            ICurrentUserService currentUserService,
            IClaimRepository claimRepository,
            IClaimDecisionService claimDecisionService,
            ISurveyAssignmentRepository surveyAssignmentRepository)
        {
            _surveyReportService = surveyReportService;
            _currentUserService = currentUserService;
            _claimRepository = claimRepository;
            _claimDecisionService = claimDecisionService;
            _surveyAssignmentRepository = surveyAssignmentRepository;
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

        private bool IsApprover =>
            string.Equals(
                _currentUserService.RoleName,
                RoleConstants.Approver,
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

        // Mirrors ClaimsController.CanAccessClaimAsync - duplicated here
        // rather than shared, matching how every other controller/service
        // in this codebase owns its own copy of this check.
        private async Task<bool> CanViewAssessmentAsync(Guid claimId)
        {
            if (IsAdmin)
            {
                return true;
            }

            if (!_currentUserService.UserId.HasValue)
            {
                return false;
            }

            var userId = _currentUserService.UserId.Value;

            if (IsSurveyor)
            {
                var assignments = await _surveyAssignmentRepository.GetByClaimAsync(claimId);
                return assignments.Any(x => x.SurveyorId == userId);
            }

            if (IsApprover)
            {
                var claim = await _claimRepository.GetByIdAsync(claimId);

                if (claim == null)
                {
                    return false;
                }

                if (claim.StatusId >= ClaimStatusConstants.RepairInProgress)
                {
                    return true;
                }

                var latestDecision = await _claimDecisionService.GetLatestDecisionAsync(claimId);
                return latestDecision?.Escalated == true;
            }

            return false;
        }

        // =========================================================
        // GET ALL SURVEY REPORTS
        // GET: api/SurveyReports
        // =========================================================

        [HttpGet]
        [ProducesResponseType(
            typeof(IEnumerable<SurveyReportResponseDto>),
            StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetAll()
        {
            if (!IsAdmin)
            {
                return Forbidden(
                    "Only an Admin can list all survey reports.");
            }

            var reports =
                await _surveyReportService.GetAllAsync();

            return Ok(reports);
        }

        // =========================================================
        // GET SURVEY REPORT BY ID
        // GET: api/SurveyReports/{id}
        // =========================================================

        [HttpGet("{id:guid}")]
        [ProducesResponseType(
            typeof(SurveyReportResponseDto),
            StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetById(Guid id)
        {
            var report =
                await _surveyReportService.GetByIdAsync(id);

            if (report == null)
            {
                return NotFound(new
                {
                    Success = false,
                    Message = "Survey report not found."
                });
            }

            if (!IsAdmin &&
                report.SurveyorId != _currentUserService.UserId)
            {
                return Forbidden(
                    "You are not authorized to view this survey report.");
            }

            return Ok(report);
        }

        // =========================================================
        // GET SURVEY REPORTS BY CLAIM
        // GET: api/SurveyReports/claim/{claimId}
        // =========================================================

        [HttpGet("claim/{claimId:guid}")]
        [ProducesResponseType(
            typeof(IEnumerable<SurveyReportResponseDto>),
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
                    "You are not authorized to view survey reports for this claim.");
            }

            var reports =
                await _surveyReportService.GetByClaimAsync(
                    claimId);

            if (!IsAdmin)
            {
                reports =
                    reports.Where(
                        x =>
                            x.SurveyorId ==
                            _currentUserService.UserId);
            }

            return Ok(reports);
        }

        // =========================================================
        // GET SURVEY REPORTS BY ASSIGNMENT
        // GET: api/SurveyReports/assignment/{surveyAssignmentId}
        // =========================================================

        [HttpGet("assignment/{surveyAssignmentId:guid}")]
        [ProducesResponseType(
            typeof(IEnumerable<SurveyReportResponseDto>),
            StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetByAssignment(
            Guid surveyAssignmentId)
        {
            if (!IsAdmin &&
                !IsSurveyor)
            {
                return Forbidden(
                    "You are not authorized to view these survey reports.");
            }

            var reports =
                await _surveyReportService.GetByAssignmentAsync(
                    surveyAssignmentId);

            if (!IsAdmin)
            {
                reports =
                    reports.Where(
                        x =>
                            x.SurveyorId ==
                            _currentUserService.UserId);
            }

            return Ok(reports);
        }

        // =========================================================
        // GET SURVEY REPORTS BY SURVEYOR
        // GET: api/SurveyReports/surveyor/{surveyorId}
        // =========================================================

        [HttpGet("surveyor/{surveyorId:guid}")]
        [ProducesResponseType(
            typeof(IEnumerable<SurveyReportResponseDto>),
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
                    "You are not authorized to view another surveyor's reports.");
            }

            var reports =
                await _surveyReportService.GetBySurveyorAsync(
                    surveyorId);

            return Ok(reports);
        }

        // =========================================================
        // CREATE SURVEY REPORT
        // POST: api/SurveyReports
        // =========================================================

        [HttpPost]
        [ProducesResponseType(
            typeof(SurveyReportResponseDto),
            StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Create(
            CreateSurveyReportRequest request)
        {
            if (!IsSurveyor)
            {
                return Forbidden(
                    "Only a Surveyor can submit a survey report.");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (request.SurveyorId != _currentUserService.UserId)
            {
                return Forbidden(
                    "You can only submit a survey report as yourself.");
            }

            var report =
                await _surveyReportService.CreateAsync(
                    request);

            return CreatedAtAction(
                nameof(GetById),
                new
                {
                    id = report.SurveyReportId
                },
                report);
        }

        // =========================================================
        // UPDATE SURVEY REPORT
        // PUT: api/SurveyReports
        // =========================================================

        [HttpPut]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Update(
            UpdateSurveyReportRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var existing =
                await _surveyReportService.GetByIdAsync(
                    request.SurveyReportId);

            if (existing == null)
            {
                return NotFound(new
                {
                    Success = false,
                    Message = "Survey report not found."
                });
            }

            if (!IsAdmin &&
                existing.SurveyorId != _currentUserService.UserId)
            {
                return Forbidden(
                    "You are not authorized to update this survey report.");
            }

            var updated =
                await _surveyReportService.UpdateAsync(
                    request);

            if (!updated)
            {
                return NotFound(new
                {
                    Success = false,
                    Message = "Survey report not found."
                });
            }

            return Ok(new
            {
                Success = true,
                Message = "Survey report updated successfully."
            });
        }

        // =========================================================
        // DELETE SURVEY REPORT
        // DELETE: api/SurveyReports/{id}
        // =========================================================

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Delete(Guid id)
        {
            var existing =
                await _surveyReportService.GetByIdAsync(id);

            if (existing == null)
            {
                return NotFound(new
                {
                    Success = false,
                    Message = "Survey report not found."
                });
            }

            if (!IsAdmin &&
                existing.SurveyorId != _currentUserService.UserId)
            {
                return Forbidden(
                    "You are not authorized to delete this survey report.");
            }

            var deleted =
                await _surveyReportService.DeleteAsync(id);

            if (!deleted)
            {
                return NotFound(new
                {
                    Success = false,
                    Message = "Survey report not found."
                });
            }

            return Ok(new
            {
                Success = true,
                Message = "Survey report deleted successfully."
            });
        }

        // =========================================================
        // Phase 13 - Surveyor Survey & Assessment screen
        // =========================================================

        // GET: api/SurveyReports/assessment/claim/{claimId}
        [HttpGet("assessment/claim/{claimId:guid}")]
        [ProducesResponseType(typeof(SurveyAssessmentResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetAssessmentByClaim(Guid claimId)
        {
            if (!await CanViewAssessmentAsync(claimId))
            {
                return Forbidden(
                    "You are not authorized to view the assessment for this claim.");
            }

            var assessment = await _surveyReportService.GetAssessmentByClaimAsync(claimId);

            return Ok(assessment);
        }

        // POST: api/SurveyReports/assessment/draft
        [HttpPost("assessment/draft")]
        [ProducesResponseType(typeof(SurveyAssessmentResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> SaveDraft(SaveSurveyAssessmentRequest request)
        {
            if (!IsSurveyor)
            {
                return Forbidden("Only a Surveyor can save a survey assessment.");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (!_currentUserService.UserId.HasValue ||
                request.SurveyorId != _currentUserService.UserId.Value)
            {
                return Forbidden("You can only save an assessment as yourself.");
            }

            var assignments =
                await _surveyAssignmentRepository.GetByClaimAsync(request.ClaimId);

            var ownsAssignment = assignments.Any(
                x => x.SurveyAssignmentId == request.SurveyAssignmentId &&
                     x.SurveyorId == _currentUserService.UserId.Value);

            if (!ownsAssignment)
            {
                return Forbidden("You are not assigned to survey this claim.");
            }

            try
            {
                var result = await _surveyReportService.SaveDraftAsync(
                    _currentUserService.UserId.Value,
                    request);

                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Success = false, Message = ex.Message });
            }
        }

        // POST: api/SurveyReports/assessment/complete
        [HttpPost("assessment/complete")]
        [ProducesResponseType(typeof(SurveyAssessmentResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> CompleteAssessment(CompleteSurveyAssessmentRequest request)
        {
            if (!IsSurveyor)
            {
                return Forbidden("Only a Surveyor can complete a survey assessment.");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (!_currentUserService.UserId.HasValue)
            {
                return Forbidden("You are not authorized to complete this assessment.");
            }

            try
            {
                var result = await _surveyReportService.CompleteAssessmentAsync(
                    _currentUserService.UserId.Value,
                    request);

                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Success = false, Message = ex.Message });
            }
        }
    }
}
