using ClaimShield.Api.Authentication;
using ClaimShield.Api.Constants;
using ClaimShield.Api.Data.Context;
using ClaimShield.Api.Interfaces.Repositories;
using ClaimShield.Api.Interfaces.Services;
using ClaimShield.Api.Models.DTOs.AuditLogs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClaimShield.Api.Controllers
{
    // Phase 13 - minimal read endpoint on top of the existing, already
    // write-only IAuditLogService/AuditLogs table (EstimateEngineService,
    // ClaimDecisionService, RepairEstimateService, SurveyReportService all
    // already write to it under EntityType "Claim"). No new logging system
    // - this is the "minimal addition" the Survey & Assessment screen's
    // Recent Activity list needs.
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AuditLogsController : ControllerBase
    {
        private readonly ClaimShieldDbContext _context;
        private readonly ICurrentUserService _currentUserService;
        private readonly IClaimRepository _claimRepository;
        private readonly IClaimDecisionService _claimDecisionService;
        private readonly ISurveyAssignmentRepository _surveyAssignmentRepository;

        public AuditLogsController(
            ClaimShieldDbContext context,
            ICurrentUserService currentUserService,
            IClaimRepository claimRepository,
            IClaimDecisionService claimDecisionService,
            ISurveyAssignmentRepository surveyAssignmentRepository)
        {
            _context = context;
            _currentUserService = currentUserService;
            _claimRepository = claimRepository;
            _claimDecisionService = claimDecisionService;
            _surveyAssignmentRepository = surveyAssignmentRepository;
        }

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

        private static IActionResult Forbidden(string message)
        {
            return new ObjectResult(new { Success = false, Message = message })
            {
                StatusCode = StatusCodes.Status403Forbidden
            };
        }

        // Mirrors ClaimsController.CanAccessClaimAsync / SurveyReportsController.
        // CanViewAssessmentAsync - duplicated rather than shared, matching this
        // repo's established per-controller convention for this check.
        private async Task<bool> CanViewClaimActivityAsync(Guid claimId)
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

        // GET: api/AuditLogs/entity/{entityType}/{entityId}
        [HttpGet("entity/{entityType}/{entityId:guid}")]
        [ProducesResponseType(typeof(IEnumerable<AuditLogResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetByEntity(string entityType, Guid entityId)
        {
            if (!IsAdmin)
            {
                if (string.Equals(entityType, "Claim", StringComparison.OrdinalIgnoreCase))
                {
                    if (!await CanViewClaimActivityAsync(entityId))
                    {
                        return Forbidden("You are not authorized to view this activity log.");
                    }
                }
                else
                {
                    return Forbidden("You are not authorized to view this activity log.");
                }
            }

            var logs = await _context.AuditLogs
                .Where(x => x.EntityType == entityType && x.EntityId == entityId)
                .OrderByDescending(x => x.Timestamp)
                .Take(20)
                .ToListAsync();

            var userIds = logs
                .Where(x => x.UserId.HasValue)
                .Select(x => x.UserId!.Value)
                .Distinct()
                .ToList();

            var users = await _context.Users
                .Where(u => userIds.Contains(u.UserId))
                .ToDictionaryAsync(u => u.UserId);

            var result = logs.Select(log => new AuditLogResponseDto
            {
                AuditLogId = log.AuditLogId,
                UserId = log.UserId,
                UserName = log.UserId.HasValue && users.TryGetValue(log.UserId.Value, out var user)
                    ? GetUserDisplayName(user.FirstName, user.LastName)
                    : "System",
                Action = log.Action,
                EntityType = log.EntityType,
                EntityId = log.EntityId,
                Timestamp = log.Timestamp
            });

            return Ok(result);
        }

        private static string GetUserDisplayName(string? firstName, string? lastName)
        {
            firstName = firstName?.Trim();
            lastName = lastName?.Trim();

            if (!string.IsNullOrWhiteSpace(firstName) && !string.IsNullOrWhiteSpace(lastName))
            {
                return $"{firstName} {lastName}";
            }

            if (!string.IsNullOrWhiteSpace(firstName))
            {
                return firstName;
            }

            if (!string.IsNullOrWhiteSpace(lastName))
            {
                return lastName;
            }

            return "Unknown";
        }
    }
}
