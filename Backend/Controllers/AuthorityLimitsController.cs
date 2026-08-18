using ClaimShield.Api.Authentication;
using ClaimShield.Api.Constants;
using ClaimShield.Api.Interfaces.Services;
using ClaimShield.Api.Models.DTOs.AuthorityLimits;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ClaimShield.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = RoleConstants.Admin)]
    public class AuthorityLimitsController : ControllerBase
    {
        private readonly IAuthorityLimitService _authorityLimitService;
        private readonly ICurrentUserService _currentUserService;

        public AuthorityLimitsController(
            IAuthorityLimitService authorityLimitService,
            ICurrentUserService currentUserService)
        {
            _authorityLimitService = authorityLimitService;
            _currentUserService = currentUserService;
        }

        // =========================================================
        // GET ALL AUTHORITY LIMITS
        // GET: api/AuthorityLimits
        // =========================================================

        [HttpGet]
        [ProducesResponseType(
            typeof(IEnumerable<AuthorityLimitResponseDto>),
            StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetAll()
        {
            var limits =
                await _authorityLimitService.GetAllAsync();

            return Ok(limits);
        }

        // =========================================================
        // GET AUTHORITY LIMIT FOR A ROLE
        // GET: api/AuthorityLimits/{roleId}
        // =========================================================

        [HttpGet("{roleId:int}")]
        [ProducesResponseType(
            typeof(AuthorityLimitResponseDto),
            StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetByRole(
            int roleId)
        {
            var limit =
                await _authorityLimitService.GetByRoleAsync(
                    roleId);

            if (limit == null)
            {
                return NotFound(new
                {
                    Success = false,
                    Message = "No authority limit has been configured for this role yet."
                });
            }

            return Ok(limit);
        }

        // =========================================================
        // SET AUTHORITY LIMIT FOR A ROLE
        // PUT: api/AuthorityLimits/{roleId}
        // =========================================================

        [HttpPut("{roleId:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Upsert(
            int roleId,
            [FromBody] UpdateAuthorityLimitRequest request)
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

            var limit =
                await _authorityLimitService.UpsertAsync(
                    roleId,
                    _currentUserService.UserId.Value,
                    request);

            return Ok(new
            {
                Success = true,
                Message = "Authority limit updated successfully.",
                Limit = limit
            });
        }
    }
}
