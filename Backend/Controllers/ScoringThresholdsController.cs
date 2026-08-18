using ClaimShield.Api.Authentication;
using ClaimShield.Api.Constants;
using ClaimShield.Api.Interfaces.Services;
using ClaimShield.Api.Models.DTOs.ClaimScoring;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ClaimShield.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = RoleConstants.Admin)]
    public class ScoringThresholdsController : ControllerBase
    {
        private readonly IScoringThresholdService _scoringThresholdService;
        private readonly ICurrentUserService _currentUserService;

        public ScoringThresholdsController(
            IScoringThresholdService scoringThresholdService,
            ICurrentUserService currentUserService)
        {
            _scoringThresholdService = scoringThresholdService;
            _currentUserService = currentUserService;
        }

        // =========================================================
        // GET ACTIVE THRESHOLDS
        // GET: api/ScoringThresholds
        // =========================================================

        [HttpGet]
        [ProducesResponseType(
            typeof(ScoringThresholdResponseDto),
            StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetActive()
        {
            var threshold =
                await _scoringThresholdService.GetActiveAsync();

            if (threshold == null)
            {
                return NotFound(new
                {
                    Success = false,
                    Message =
                        "No scoring thresholds have been configured yet. " +
                        "Until configured, all claims fail safe to Red."
                });
            }

            return Ok(threshold);
        }

        // =========================================================
        // SET THRESHOLDS
        // PUT: api/ScoringThresholds
        // =========================================================

        [HttpPut]
        [ProducesResponseType(
            typeof(ScoringThresholdResponseDto),
            StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Upsert(
            [FromBody] UpdateScoringThresholdRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (!_currentUserService.UserId.HasValue)
            {
                return BadRequest(new
                {
                    Success = false,
                    Message = "Unable to determine the logged-in Admin."
                });
            }

            var threshold =
                await _scoringThresholdService.UpsertAsync(
                    _currentUserService.UserId.Value,
                    request);

            return Ok(new
            {
                Success = true,
                Message = "Scoring thresholds updated successfully.",
                Threshold = threshold
            });
        }
    }
}
