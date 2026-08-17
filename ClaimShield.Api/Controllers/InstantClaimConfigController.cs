using ClaimShield.Api.Authentication;
using ClaimShield.Api.Constants;
using ClaimShield.Api.Interfaces.Services;
using ClaimShield.Api.Models.DTOs.InstantClaim;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ClaimShield.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = RoleConstants.Admin)]
    public class InstantClaimConfigController : ControllerBase
    {
        private readonly IInstantClaimConfigService _configService;
        private readonly ICurrentUserService _currentUserService;

        public InstantClaimConfigController(
            IInstantClaimConfigService configService,
            ICurrentUserService currentUserService)
        {
            _configService = configService;
            _currentUserService = currentUserService;
        }

        private IActionResult NoUser()
        {
            return BadRequest(new
            {
                Success = false,
                Message = "Unable to determine the logged-in Admin."
            });
        }

        // ===== RATE CARDS =====

        [HttpGet("rate-cards")]
        public async Task<IActionResult> GetRateCards()
        {
            return Ok(await _configService.GetRateCardsAsync());
        }

        [HttpPost("rate-cards")]
        public async Task<IActionResult> CreateRateCard(
            [FromBody] CreateInstantClaimRateCardRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            if (!_currentUserService.UserId.HasValue) return NoUser();

            var result =
                await _configService.CreateRateCardAsync(
                    _currentUserService.UserId.Value, request);

            return Ok(new { Success = true, Message = "Rate card created.", RateCard = result });
        }

        [HttpPut("rate-cards/{rateCardId}")]
        public async Task<IActionResult> UpdateRateCard(
            string rateCardId,
            [FromBody] UpdateInstantClaimRateCardRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            if (!_currentUserService.UserId.HasValue) return NoUser();

            var result =
                await _configService.UpdateRateCardAsync(
                    rateCardId, _currentUserService.UserId.Value, request);

            if (result == null)
            {
                return NotFound(new { Success = false, Message = "Rate card not found." });
            }

            return Ok(new { Success = true, Message = "Rate card updated.", RateCard = result });
        }

        [HttpPatch("rate-cards/{rateCardId}/toggle-active")]
        public async Task<IActionResult> ToggleRateCard(
            string rateCardId)
        {
            if (!_currentUserService.UserId.HasValue) return NoUser();

            var result =
                await _configService.ToggleRateCardActiveAsync(
                    rateCardId, _currentUserService.UserId.Value);

            if (result == null)
            {
                return NotFound(new { Success = false, Message = "Rate card not found." });
            }

            return Ok(new { Success = true, Message = "Rate card status updated.", RateCard = result });
        }

        // ===== PARTS PRICING =====

        [HttpGet("parts-pricing")]
        public async Task<IActionResult> GetPartsPricing()
        {
            return Ok(await _configService.GetPartsPricingAsync());
        }

        [HttpPost("parts-pricing")]
        public async Task<IActionResult> CreatePartsPricing(
            [FromBody] CreateInstantClaimPartsPricingRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            if (!_currentUserService.UserId.HasValue) return NoUser();

            var result =
                await _configService.CreatePartsPricingAsync(
                    _currentUserService.UserId.Value, request);

            return Ok(new { Success = true, Message = "Parts pricing created.", PartsPricing = result });
        }

        [HttpPut("parts-pricing/{partsPricingId}")]
        public async Task<IActionResult> UpdatePartsPricing(
            string partsPricingId,
            [FromBody] UpdateInstantClaimPartsPricingRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            if (!_currentUserService.UserId.HasValue) return NoUser();

            var result =
                await _configService.UpdatePartsPricingAsync(
                    partsPricingId, _currentUserService.UserId.Value, request);

            if (result == null)
            {
                return NotFound(new { Success = false, Message = "Parts pricing not found." });
            }

            return Ok(new { Success = true, Message = "Parts pricing updated.", PartsPricing = result });
        }

        [HttpPatch("parts-pricing/{partsPricingId}/toggle-active")]
        public async Task<IActionResult> TogglePartsPricing(
            string partsPricingId)
        {
            if (!_currentUserService.UserId.HasValue) return NoUser();

            var result =
                await _configService.TogglePartsPricingActiveAsync(
                    partsPricingId, _currentUserService.UserId.Value);

            if (result == null)
            {
                return NotFound(new { Success = false, Message = "Parts pricing not found." });
            }

            return Ok(new { Success = true, Message = "Parts pricing status updated.", PartsPricing = result });
        }

        // ===== ELIGIBILITY =====

        [HttpGet("eligibility")]
        public async Task<IActionResult> GetEligibility()
        {
            var result = await _configService.GetEligibilityAsync();

            if (result == null)
            {
                return NotFound(new
                {
                    Success = false,
                    Message = "No eligibility rule configured yet. Until configured, only Green-band claims are Instant Claim eligible (the most conservative default)."
                });
            }

            return Ok(result);
        }

        [HttpPut("eligibility")]
        public async Task<IActionResult> UpsertEligibility(
            [FromBody] UpdateInstantClaimEligibilityRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            if (!_currentUserService.UserId.HasValue) return NoUser();

            var result =
                await _configService.UpsertEligibilityAsync(
                    _currentUserService.UserId.Value, request);

            return Ok(new { Success = true, Message = "Eligibility rule updated.", Eligibility = result });
        }
    }
}
