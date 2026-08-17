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
    public class ScoringRulesController : ControllerBase
    {
        private readonly IScoringRuleService _scoringRuleService;
        private readonly ICurrentUserService _currentUserService;

        public ScoringRulesController(
            IScoringRuleService scoringRuleService,
            ICurrentUserService currentUserService)
        {
            _scoringRuleService = scoringRuleService;
            _currentUserService = currentUserService;
        }

        // =========================================================
        // GET ALL RULES (optionally filtered by stage)
        // GET: api/ScoringRules?stage=1
        // =========================================================

        [HttpGet]
        [ProducesResponseType(
            typeof(IEnumerable<ScoringRuleResponseDto>),
            StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetAll(
            [FromQuery] int? stage)
        {
            var rules =
                await _scoringRuleService.GetAllAsync(stage);

            return Ok(rules);
        }

        // =========================================================
        // CREATE RULE
        // POST: api/ScoringRules
        // =========================================================

        [HttpPost]
        [ProducesResponseType(
            typeof(ScoringRuleResponseDto),
            StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Create(
            [FromBody] CreateScoringRuleRequest request)
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

            var rule =
                await _scoringRuleService.CreateAsync(
                    _currentUserService.UserId.Value,
                    request);

            return Ok(new
            {
                Success = true,
                Message = "Scoring rule created successfully.",
                Rule = rule
            });
        }

        // =========================================================
        // UPDATE RULE (increments Version, resets EffectiveFrom)
        // PUT: api/ScoringRules/{ruleId}
        // =========================================================

        [HttpPut("{ruleId}")]
        [ProducesResponseType(
            typeof(ScoringRuleResponseDto),
            StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Update(
            string ruleId,
            [FromBody] UpdateScoringRuleRequest request)
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

            var rule =
                await _scoringRuleService.UpdateAsync(
                    ruleId,
                    _currentUserService.UserId.Value,
                    request);

            if (rule == null)
            {
                return NotFound(new
                {
                    Success = false,
                    Message = "Scoring rule not found."
                });
            }

            return Ok(new
            {
                Success = true,
                Message = "Scoring rule updated successfully.",
                Rule = rule
            });
        }

        // =========================================================
        // TOGGLE ACTIVE
        // PATCH: api/ScoringRules/{ruleId}/toggle-active
        // =========================================================

        [HttpPatch("{ruleId}/toggle-active")]
        [ProducesResponseType(
            typeof(ScoringRuleResponseDto),
            StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> ToggleActive(
            string ruleId)
        {
            if (!_currentUserService.UserId.HasValue)
            {
                return BadRequest(new
                {
                    Success = false,
                    Message = "Unable to determine the logged-in Admin."
                });
            }

            var rule =
                await _scoringRuleService.ToggleActiveAsync(
                    ruleId,
                    _currentUserService.UserId.Value);

            if (rule == null)
            {
                return NotFound(new
                {
                    Success = false,
                    Message = "Scoring rule not found."
                });
            }

            return Ok(new
            {
                Success = true,
                Message = "Scoring rule status updated successfully.",
                Rule = rule
            });
        }
    }
}
