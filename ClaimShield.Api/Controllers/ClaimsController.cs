using ClaimShield.Api.Authentication;
using ClaimShield.Api.Constants;
using ClaimShield.Api.Interfaces.Repositories;
using ClaimShield.Api.Interfaces.Services;
using ClaimShield.Api.Models.DTOs.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ClaimShield.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ClaimsController : ControllerBase
    {
        private readonly IClaimService _claimService;
        private readonly IClaimClosureService _claimClosureService;
        private readonly ICustomerRepository _customerRepository;
        private readonly IPolicyRepository _policyRepository;
        private readonly IVehicleRepository _vehicleRepository;
        private readonly ISurveyAssignmentRepository _surveyAssignmentRepository;
        private readonly IRepairAssignmentRepository _repairAssignmentRepository;
        private readonly IClaimScoringService _claimScoringService;
        private readonly IClaimDecisionService _claimDecisionService;
        private readonly ICurrentUserService _currentUserService;

        public ClaimsController(
            IClaimService claimService,
            IClaimClosureService claimClosureService,
            ICustomerRepository customerRepository,
            IPolicyRepository policyRepository,
            IVehicleRepository vehicleRepository,
            ISurveyAssignmentRepository surveyAssignmentRepository,
            IRepairAssignmentRepository repairAssignmentRepository,
            IClaimScoringService claimScoringService,
            IClaimDecisionService claimDecisionService,
            ICurrentUserService currentUserService)
        {
            _claimService = claimService;
            _claimClosureService = claimClosureService;
            _customerRepository = customerRepository;
            _policyRepository = policyRepository;
            _vehicleRepository = vehicleRepository;
            _surveyAssignmentRepository = surveyAssignmentRepository;
            _repairAssignmentRepository = repairAssignmentRepository;
            _claimScoringService = claimScoringService;
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

        private bool IsCustomer =>
            string.Equals(
                _currentUserService.RoleName,
                RoleConstants.Customer,
                StringComparison.OrdinalIgnoreCase);

        private int CurrentRoleId =>
            _currentUserService.RoleName switch
            {
                RoleConstants.Customer => RoleConstants.CustomerId,
                RoleConstants.Repairer => RoleConstants.RepairerId,
                RoleConstants.Surveyor => RoleConstants.SurveyorId,
                RoleConstants.Approver => RoleConstants.ApproverId,
                RoleConstants.Admin => RoleConstants.AdminId,
                _ => 0
            };

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
        // CLAIM ACCESS CHECK
        //
        // Customer = 1, Repairer = 2, Surveyor = 3, Approver = 4,
        // Admin = 5. Mirrors ClaimDocumentService.
        // CanUserAccessClaimAsync / MockAiService.
        // IsClaimAccessibleByCurrentUserAsync's access matrix -
        // duplicated here rather than shared, matching how every
        // other controller/service in this codebase owns its own
        // copy of this check.
        // =========================================================

        private async Task<bool> CanAccessClaimAsync(
            ClaimResponseDto claim,
            Guid userId,
            int roleId)
        {
            if (roleId == RoleConstants.AdminId)
            {
                return true;
            }

            if (roleId == RoleConstants.CustomerId)
            {
                var customer =
                    await _customerRepository.GetByIdAsync(
                        claim.CustomerId);

                return customer != null && customer.UserId == userId;
            }

            if (roleId == RoleConstants.SurveyorId)
            {
                var assignments =
                    await _surveyAssignmentRepository.GetByClaimAsync(
                        claim.ClaimId);

                return assignments.Any(x => x.SurveyorId == userId);
            }

            if (roleId == RoleConstants.RepairerId)
            {
                var assignments =
                    await _repairAssignmentRepository.GetByClaimAsync(
                        claim.ClaimId);

                return assignments.Any(x => x.RepairerId == userId);
            }

            if (roleId == RoleConstants.ApproverId)
            {
                // Once repairs are underway, an Approver's involvement
                // spans the rest of the lifecycle (approving, then
                // handling payment) - not just the RepairInProgress
                // window. Checking the current status (rather than
                // decision history) also works regardless of which
                // approval path a claim took: the maker-checker
                // ClaimDecisionService flow, or the repair-estimate
                // approval flow (IClaimApprovalService), which never
                // creates a ClaimDecision row.
                if (claim.StatusId >= ClaimStatusConstants.RepairInProgress)
                {
                    return true;
                }

                // Also allow while a Surveyor decision is escalated and
                // awaiting Approver review (status is still
                // SurveyCompleted during that window).
                var latestDecision =
                    await _claimDecisionService.GetLatestDecisionAsync(
                        claim.ClaimId);

                return latestDecision?.Escalated == true;
            }

            return false;
        }

        // =========================================================
        // GET ALL
        // GET: api/Claims
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            if (!IsAdmin)
            {
                return Forbidden(
                    "Only an Admin can list all claims.");
            }

            var claims =
                await _claimService.GetAllClaimsAsync();

            return Ok(claims);
        }

        // =========================================================
        // GET BY ID
        // GET: api/Claims/{id}
        // =========================================================

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(
            Guid id)
        {
            var claim =
                await _claimService.GetClaimByIdAsync(id);

            if (claim == null)
            {
                return NotFound(new
                {
                    Success = false,
                    Message = "Claim not found."
                });
            }

            if (!_currentUserService.UserId.HasValue ||
                !await CanAccessClaimAsync(
                    claim,
                    _currentUserService.UserId.Value,
                    CurrentRoleId))
            {
                return Forbidden(
                    "You are not authorized to view this claim.");
            }

            return Ok(claim);
        }

        // =========================================================
        // GET BY CUSTOMER
        // GET: api/Claims/customer/{customerId}
        // =========================================================

        [HttpGet("customer/{customerId:guid}")]
        public async Task<IActionResult> GetByCustomer(
            Guid customerId)
        {
            if (!IsAdmin)
            {
                if (!IsCustomer || !_currentUserService.UserId.HasValue)
                {
                    return Forbidden(
                        "You are not authorized to view these claims.");
                }

                var myCustomer =
                    await _customerRepository.GetByUserIdAsync(
                        _currentUserService.UserId.Value);

                if (myCustomer == null || myCustomer.CustomerId != customerId)
                {
                    return Forbidden(
                        "You are not authorized to view another customer's claims.");
                }
            }

            var claims =
                await _claimService
                    .GetClaimsByCustomerAsync(customerId);

            return Ok(claims);
        }

        // =========================================================
        // GET BY POLICY
        // GET: api/Claims/policy/{policyId}
        // =========================================================

        [HttpGet("policy/{policyId:guid}")]
        [Authorize(Roles = RoleConstants.Admin)]
        public async Task<IActionResult> GetByPolicy(
            Guid policyId)
        {
            var claims =
                await _claimService
                    .GetClaimsByPolicyAsync(policyId);

            return Ok(claims);
        }

        // =========================================================
        // GET BY VEHICLE
        // GET: api/Claims/vehicle/{vehicleId}
        // =========================================================

        [HttpGet("vehicle/{vehicleId:guid}")]
        [Authorize(Roles = RoleConstants.Admin)]
        public async Task<IActionResult> GetByVehicle(
            Guid vehicleId)
        {
            var claims =
                await _claimService
                    .GetClaimsByVehicleAsync(vehicleId);

            return Ok(claims);
        }

        // =========================================================
        // CREATE
        // POST: api/Claims
        // =========================================================

        [HttpPost]
        public async Task<IActionResult> Create(
            CreateClaimRequest request)
        {
            if (!IsAdmin && !IsCustomer)
            {
                return Forbidden(
                    "Only a Customer or Admin can submit a claim.");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (IsCustomer)
            {
                if (!_currentUserService.UserId.HasValue)
                {
                    return Forbidden(
                        "Unable to determine the logged-in user.");
                }

                var myCustomer =
                    await _customerRepository.GetByUserIdAsync(
                        _currentUserService.UserId.Value);

                if (myCustomer == null)
                {
                    return BadRequest(new
                    {
                        Success = false,
                        Message = "No customer record exists for this account."
                    });
                }

                // -------------------------------------------------
                // Never trust the caller for who/what/how much - a
                // Customer submits for themselves, against their own
                // policy/vehicle, always starting at Submitted with
                // no approved amount or fraud flag.
                // -------------------------------------------------

                request.CustomerId = myCustomer.CustomerId;

                var policy =
                    await _policyRepository.GetByIdAsync(
                        request.PolicyId);

                if (policy == null ||
                    policy.CustomerId != myCustomer.CustomerId)
                {
                    return Forbidden(
                        "You can only submit a claim against your own policy.");
                }

                var vehicle =
                    await _vehicleRepository.GetByIdAsync(
                        request.VehicleId);

                if (vehicle == null ||
                    vehicle.CustomerId != myCustomer.CustomerId)
                {
                    return Forbidden(
                        "You can only submit a claim for your own vehicle.");
                }

                request.StatusId = ClaimStatusConstants.Submitted;
                request.ApprovedAmount = null;
                request.IsFraudSuspected = false;
            }

            var claim =
                await _claimService
                    .CreateClaimAsync(request);

            return CreatedAtAction(
                nameof(GetById),
                new
                {
                    id = claim.ClaimId
                },
                claim);
        }

        // =========================================================
        // UPDATE
        // PUT: api/Claims
        // =========================================================

        [HttpPut]
        [Authorize(Roles = RoleConstants.Admin)]
        public async Task<IActionResult> Update(
            UpdateClaimRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var updated =
                await _claimService
                    .UpdateClaimAsync(request);

            if (!updated)
            {
                return NotFound(new
                {
                    Success = false,
                    Message = "Claim not found."
                });
            }

            return Ok(new
            {
                Success = true,
                Message = "Claim updated successfully."
            });
        }

        // =========================================================
        // DELETE
        // DELETE: api/Claims/{id}
        // =========================================================

        [HttpDelete("{id:guid}")]
        [Authorize(Roles = RoleConstants.Admin)]
        public async Task<IActionResult> Delete(
            Guid id)
        {
            var deleted =
                await _claimService
                    .DeleteClaimAsync(id);

            if (!deleted)
            {
                return NotFound(new
                {
                    Success = false,
                    Message = "Claim not found."
                });
            }

            return Ok(new
            {
                Success = true,
                Message = "Claim deleted successfully."
            });
        }

        // =========================================================
        // CLOSE CLAIM
        // POST: api/Claims/{claimId}/close
        // =========================================================

        [HttpPost("{claimId:guid}/close")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> CloseClaim(
            Guid claimId,
            [FromBody] CloseClaimRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var claim =
                await _claimService.GetClaimByIdAsync(claimId);

            if (claim == null)
            {
                return NotFound(new
                {
                    Success = false,
                    Message = "Claim not found."
                });
            }

            if (!IsAdmin)
            {
                if (!IsCustomer || !_currentUserService.UserId.HasValue)
                {
                    return Forbidden(
                        "You are not authorized to close this claim.");
                }

                var myCustomer =
                    await _customerRepository.GetByUserIdAsync(
                        _currentUserService.UserId.Value);

                if (myCustomer == null ||
                    myCustomer.CustomerId != claim.CustomerId)
                {
                    return Forbidden(
                        "You are not authorized to close this claim.");
                }
            }

            var result =
                await _claimClosureService.CloseClaimAsync(
                    claimId,
                    request);

            if (!result)
            {
                return BadRequest(new
                {
                    Success = false,
                    Message =
                        "Claim could not be closed. " +
                        "Only a Settled claim can be closed."
                });
            }

            return Ok(new
            {
                Success = true,
                Message = "Claim closed successfully.",
                ClaimId = claimId,
                StatusId = 10,
                Status = "Closed"
            });
        }

        // =========================================================
        // SCORING RESULTS
        //
        // Customer sees band only; Surveyor/Approver/Admin see full
        // rule-level detail. Enforced server-side via
        // IClaimScoringService.GetScoringForUserAsync - never a
        // filtered 200, always a hard 403 for unauthorized access so
        // claim existence is never leaked to the wrong caller.
        // GET: api/Claims/{claimId}/scoring-results
        // =========================================================

        [HttpGet("{claimId:guid}/scoring-results")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetScoringResults(
            Guid claimId)
        {
            if (!_currentUserService.UserId.HasValue)
            {
                return Forbidden(
                    "Unable to determine the logged-in user.");
            }

            var access =
                await _claimScoringService.GetScoringForUserAsync(
                    claimId,
                    _currentUserService.UserId.Value,
                    CurrentRoleId);

            if (!access.ClaimFound)
            {
                return NotFound(new
                {
                    Success = false,
                    Message = "Claim not found."
                });
            }

            if (!access.Authorized)
            {
                return Forbidden(
                    "You are not authorized to view scoring results for this claim.");
            }

            if (access.CustomerView != null)
            {
                return Ok(access.CustomerView);
            }

            if (access.InternalView != null)
            {
                return Ok(access.InternalView);
            }

            return NotFound(new
            {
                Success = false,
                Message = "No scoring results are available for this claim yet."
            });
        }
    }
}
