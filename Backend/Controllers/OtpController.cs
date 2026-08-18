using ClaimShield.Api.Authentication;
using ClaimShield.Api.Constants;
using ClaimShield.Api.Interfaces.Repositories;
using ClaimShield.Api.Interfaces.Services;
using ClaimShield.Api.Models.DTOs.Otp;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ClaimShield.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class OtpController : ControllerBase
    {
        private readonly IOtpService _otpService;
        private readonly IClaimService _claimService;
        private readonly ICustomerRepository _customerRepository;
        private readonly ICurrentUserService _currentUserService;

        public OtpController(
            IOtpService otpService,
            IClaimService claimService,
            ICustomerRepository customerRepository,
            ICurrentUserService currentUserService)
        {
            _otpService = otpService;
            _claimService = claimService;
            _customerRepository = customerRepository;
            _currentUserService = currentUserService;
        }

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

        // Resolves the correct SubjectId for the requested purpose, and
        // enforces that an InstantClaimAccept OTP can only be requested/
        // verified by the Customer who owns that specific claim - never
        // trust the caller to only ask for their own claim.
        private async Task<(bool Ok, Guid SubjectId, IActionResult? Error)> ResolveSubjectAsync(
            string purpose,
            Guid? claimId)
        {
            if (!_currentUserService.UserId.HasValue)
            {
                return (
                    false,
                    Guid.Empty,
                    Forbidden("Unable to determine the logged-in user."));
            }

            if (purpose == OtpPurposeConstants.Login)
            {
                return (true, _currentUserService.UserId.Value, null);
            }

            if (purpose == OtpPurposeConstants.InstantClaimAccept)
            {
                if (!claimId.HasValue)
                {
                    return (
                        false,
                        Guid.Empty,
                        new BadRequestObjectResult(new
                        {
                            Success = false,
                            Message = "claimId is required for this OTP purpose."
                        }));
                }

                var claim =
                    await _claimService.GetClaimByIdAsync(claimId.Value);

                if (claim == null)
                {
                    return (
                        false,
                        Guid.Empty,
                        new NotFoundObjectResult(new
                        {
                            Success = false,
                            Message = "Claim not found."
                        }));
                }

                var customer =
                    await _customerRepository.GetByIdAsync(claim.CustomerId);

                if (customer == null ||
                    customer.UserId != _currentUserService.UserId.Value)
                {
                    return (
                        false,
                        Guid.Empty,
                        Forbidden("You are not authorized to act on this claim."));
                }

                return (true, claimId.Value, null);
            }

            return (
                false,
                Guid.Empty,
                new BadRequestObjectResult(new
                {
                    Success = false,
                    Message = "Unknown OTP purpose."
                }));
        }

        [HttpPost("send")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Send(
            [FromBody] SendOtpRequest request)
        {
            var (ok, subjectId, error) =
                await ResolveSubjectAsync(request.Purpose, request.ClaimId);

            if (!ok)
            {
                return error!;
            }

            var result =
                await _otpService.SendAsync(request.Purpose, subjectId);

            return Ok(result);
        }

        [HttpPost("verify")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Verify(
            [FromBody] VerifyOtpRequest request)
        {
            var (ok, subjectId, error) =
                await ResolveSubjectAsync(request.Purpose, request.ClaimId);

            if (!ok)
            {
                return error!;
            }

            var result =
                await _otpService.VerifyAsync(
                    request.Purpose,
                    subjectId,
                    request.Code);

            return Ok(result);
        }
    }
}
