using System.Text.RegularExpressions;

using ClaimShield.Api.AI.Interfaces;
using ClaimShield.Api.AI.Models;
using ClaimShield.Api.Authentication;
using ClaimShield.Api.Constants;

using ClaimShield.Api.Interfaces.Repositories;
using ClaimShield.Api.Interfaces.Services;

using ClaimShield.Api.Models.DTOs.Claims;
using ClaimShield.Api.Models.Entities;

using Microsoft.AspNetCore.Http;

namespace ClaimShield.Api.AI.Services
{
    public class MockAiService : IAiService
    {
        // =========================================================
        // SERVICES
        // =========================================================

        private readonly IClaimService _claimService;
        private readonly IPaymentService _paymentService;
        private readonly IClaimDocumentService _claimDocumentService;
        private readonly ISurveyAssignmentService _surveyAssignmentService;
        private readonly IRepairAssignmentService _repairAssignmentService;
        private readonly IClaimClosureService _claimClosureService;

        // =========================================================
        // REPOSITORIES
        // =========================================================

        private readonly IUserRepository _userRepository;
        private readonly ICustomerRepository _customerRepository;

        // =========================================================
        // HTTP CONTEXT
        // =========================================================

        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ICurrentUserService _currentUserService;

        // =========================================================
        // CONSTRUCTOR
        // =========================================================

        public MockAiService(
            IClaimService claimService,
            IPaymentService paymentService,
            IClaimDocumentService claimDocumentService,
            ISurveyAssignmentService surveyAssignmentService,
            IRepairAssignmentService repairAssignmentService,
            IClaimClosureService claimClosureService,
            IUserRepository userRepository,
            ICustomerRepository customerRepository,
            IHttpContextAccessor httpContextAccessor,
            ICurrentUserService currentUserService)
        {
            _claimService = claimService;
            _paymentService = paymentService;
            _claimDocumentService = claimDocumentService;
            _surveyAssignmentService = surveyAssignmentService;
            _repairAssignmentService = repairAssignmentService;
            _claimClosureService = claimClosureService;

            _userRepository = userRepository;
            _customerRepository = customerRepository;

            _httpContextAccessor = httpContextAccessor;
            _currentUserService = currentUserService;
        }

        // =========================================================
        // MAIN CHAT METHOD
        // =========================================================

        public async Task<AiChatResponse> ChatAsync(
            AiChatRequest request)
        {
            if (request == null ||
                string.IsNullOrWhiteSpace(request.Message))
            {
                return new AiChatResponse
                {
                    Success = false,
                    Message = "Please provide a message.",
                    Intent = "GENERAL_CHAT"
                };
            }

            var originalMessage =
                request.Message.Trim();

            var message =
                originalMessage.ToLowerInvariant();

            // =====================================================
            // CLAIM NUMBER DETECTION
            // =====================================================

            if (!request.ClaimId.HasValue)
            {
                var extractedClaimId =
                    await GetClaimIdFromMessageAsync(
                        originalMessage);

                if (extractedClaimId.HasValue)
                {
                    request.ClaimId =
                        extractedClaimId.Value;
                }
            }

            // =====================================================
            // APPROVER - PENDING APPROVALS
            // =====================================================

            if (IsPendingApprovalIntent(message))
            {
                return await HandlePendingApprovalsAsync();
            }

            // =====================================================
            // APPROVER - APPROVE CLAIM
            // =====================================================

            if (IsApproveClaimIntent(message))
            {
                return await HandleApproveClaimAsync(
                    request.ClaimId,
                    originalMessage);
            }

            // =====================================================
            // CONFIRMATION
            // =====================================================

            if (request.Confirmed &&
                IsConfirmation(message))
            {
                return await HandleConfirmedActionAsync(
                    request.ClaimId);
            }

            // =====================================================
            // CANCELLATION
            // =====================================================

            if (IsCancellation(message))
            {
                return new AiChatResponse
                {
                    Success = true,

                    Message =
                        "Okay. I will not perform the requested action.",

                    Intent =
                        "ACTION_CANCELLED"
                };
            }

            // =====================================================
            // CLAIM CLOSURE
            // =====================================================

            if (IsCloseClaimIntent(message))
            {
                return await HandleCloseClaimAsync(
                    request.ClaimId);
            }

            // =====================================================
            // CLAIM SECURITY
            // =====================================================

            if (request.ClaimId.HasValue)
            {
                var authorized =
                    await IsClaimAccessibleByCurrentUserAsync(
                        request.ClaimId.Value);

                if (!authorized)
                {
                    return ClaimAccessDenied();
                }
            }

            // =====================================================
            // INTENT DETECTION
            // =====================================================

            var intents =
                DetectAllIntents(message);

            // =====================================================
            // MULTIPLE QUESTIONS
            // =====================================================

            if (intents.Count > 1)
            {
                return await HandleMultiIntentAsync(
                    request.ClaimId,
                    intents);
            }

            // =====================================================
            // SINGLE QUESTION
            // =====================================================

            var intent =
                intents.Count == 1
                    ? intents[0]
                    : "GENERAL_CHAT";

            switch (intent)
            {
                case "GET_CLAIM_STATUS":

                    return await HandleClaimStatusAsync(
                        request.ClaimId);

                case "GET_CLAIM_DETAILS":

                    return await HandleClaimDetailsAsync(
                        request.ClaimId);

                case "GET_PAYMENT_STATUS":

                    return await HandlePaymentStatusAsync(
                        request.ClaimId);

                case "GET_SURVEY_STATUS":

                    return await HandleSurveyStatusAsync(
                        request.ClaimId);

                case "GET_REPAIR_STATUS":

                    return await HandleRepairStatusAsync(
                        request.ClaimId);

                case "GET_DOCUMENTS":

                    return await HandleDocumentsAsync(
                        request.ClaimId);

                default:

                    return GeneralResponse();
            }
        }

        // =========================================================
        // CLAIM NUMBER -> CLAIM ID
        // =========================================================

        private async Task<Guid?> GetClaimIdFromMessageAsync(
            string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                return null;
            }

            // Claim numbers are "CLM" + the first 8 hex characters of a
            // GUID (see ClaimService.GenerateClaimNumber) - i.e. 0-9 AND
            // A-F, not digits only.
            var match =
                Regex.Match(
                    message,
                    @"\bCLM[0-9A-F]{8}\b",
                    RegexOptions.IgnoreCase);

            if (!match.Success)
            {
                return null;
            }

            var claimNumber =
                match.Value.ToUpperInvariant();

            var claims =
                await _claimService.GetAllClaimsAsync();

            var claim =
                claims.FirstOrDefault(
                    x =>
                        string.Equals(
                            x.ClaimNumber,
                            claimNumber,
                            StringComparison.OrdinalIgnoreCase));

            return claim?.ClaimId;
        }

        // =========================================================
        // CLAIM ACCESS CONTROL
        // =========================================================
        //
        // Customer = 1
        // Repairer = 2
        // Surveyor = 3
        // Approver = 4
        // Admin = 5
        // =========================================================

        private async Task<bool> IsClaimAccessibleByCurrentUserAsync(
            Guid claimId)
        {
            var httpContext =
                _httpContextAccessor.HttpContext;

            if (httpContext == null)
            {
                return false;
            }

            var currentUser =
                httpContext.User;

            if (currentUser == null ||
                currentUser.Identity == null ||
                !currentUser.Identity.IsAuthenticated)
            {
                return false;
            }

            var currentUserId =
                _currentUserService.UserId;

            if (!currentUserId.HasValue)
            {
                return false;
            }

            var databaseUser =
                await _userRepository.GetByIdAsync(
                    currentUserId.Value);

            if (databaseUser == null)
            {
                return false;
            }

            var role =
                _currentUserService.RoleName;

            // =====================================================
            // ADMIN
            // =====================================================

            if (databaseUser.RoleId == RoleConstants.AdminId ||
                string.Equals(
                    role,
                    RoleConstants.Admin,
                    StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            // =====================================================
            // CLAIM
            // =====================================================

            var claim =
                await _claimService.GetClaimByIdAsync(
                    claimId);

            if (claim == null)
            {
                return false;
            }

            // =====================================================
            // SURVEYOR
            // =====================================================

            if (databaseUser.RoleId == RoleConstants.SurveyorId ||
                string.Equals(
                    role,
                    RoleConstants.Surveyor,
                    StringComparison.OrdinalIgnoreCase))
            {
                var surveys =
                    await _surveyAssignmentService.GetByClaimAsync(
                        claimId);

                if (surveys == null)
                {
                    return false;
                }

                return surveys.Any(
                    x =>
                        x.SurveyorId ==
                        currentUserId.Value);
            }

            // =====================================================
            // REPAIRER
            // =====================================================

            if (databaseUser.RoleId == RoleConstants.RepairerId ||
                string.Equals(
                    role,
                    RoleConstants.Repairer,
                    StringComparison.OrdinalIgnoreCase))
            {
                var repairs =
                    await _repairAssignmentService.GetByClaimAsync(
                        claimId);

                if (repairs == null)
                {
                    return false;
                }

                return repairs.Any(
                    x =>
                        x.RepairerId ==
                        currentUserId.Value);
            }

            // =====================================================
            // APPROVER
            // =====================================================
            //
            // Status 6 = Repair In Progress
            // Status 7 = Approved
            // Status 8 = Rejected
            // =====================================================

            if (databaseUser.RoleId == RoleConstants.ApproverId ||
                string.Equals(
                    role,
                    RoleConstants.Approver,
                    StringComparison.OrdinalIgnoreCase))
            {
                return
                    Convert.ToInt32(
                        claim.StatusId) == ClaimStatusConstants.RepairInProgress;
            }

            // =====================================================
            // CUSTOMER
            // =====================================================

            if (databaseUser.RoleId == RoleConstants.CustomerId ||
                string.Equals(
                    role,
                    RoleConstants.Customer,
                    StringComparison.OrdinalIgnoreCase))
            {
                var customer =
                    await _customerRepository.GetByIdAsync(
                        claim.CustomerId);

                if (customer == null)
                {
                    return false;
                }

                return
                    customer.UserId ==
                    currentUserId.Value;
            }

            return false;
        }

        // =========================================================
        // PENDING APPROVALS
        // =========================================================

        private async Task<AiChatResponse>
            HandlePendingApprovalsAsync()
        {
            var currentUser =
                _httpContextAccessor.HttpContext?.User;

            if (currentUser == null ||
                currentUser.Identity == null ||
                !currentUser.Identity.IsAuthenticated)
            {
                return ClaimAccessDenied();
            }

            var currentUserId =
                _currentUserService.UserId;

            if (!currentUserId.HasValue)
            {
                return new AiChatResponse
                {
                    Success = false,

                    Message =
                        "Unable to determine the logged-in user.",

                    Intent =
                        "APPROVAL_ACCESS_DENIED"
                };
            }

            var databaseUser =
                await _userRepository.GetByIdAsync(
                    currentUserId.Value);

            if (databaseUser == null ||
                databaseUser.RoleId != RoleConstants.ApproverId)
            {
                return new AiChatResponse
                {
                    Success = false,

                    Message =
                        "Only an Approver can view claims waiting for approval.",

                    Intent =
                        "APPROVAL_ACCESS_DENIED"
                };
            }

            // =====================================================
            // CORRECT SERVICE METHOD:
            //
            // GetAllClaimsAsync()
            // =====================================================

            var claims =
                await _claimService.GetAllClaimsAsync();

            var pendingClaims =
                claims
                    .Where(
                        x =>
                            Convert.ToInt32(
                                x.StatusId) == ClaimStatusConstants.RepairInProgress)
                    .ToList();

            if (pendingClaims.Count == 0)
            {
                return new AiChatResponse
                {
                    Success = true,

                    Message =
                        "There are currently no claims waiting for approval.",

                    Intent =
                        "GET_PENDING_APPROVALS"
                };
            }

            var lines =
                new List<string>();

            foreach (var claim in pendingClaims)
            {
                var customerName =
                    await GetCustomerNameAsync(
                        claim.CustomerId);

                var amount =
                    claim.EstimatedLossAmount
                    ?? 0m;

                lines.Add(
                    $"• Claim {claim.ClaimNumber} - " +
                    $"{customerName} - " +
                    $"Estimated loss: ₹ {amount:N2}");
            }

            return new AiChatResponse
            {
                Success = true,

                Message =
                    $"There are {pendingClaims.Count} " +
                    $"claim(s) waiting for approval:\n\n" +
                    string.Join(
                        "\n",
                        lines),

                Intent =
                    "GET_PENDING_APPROVALS"
            };
        }

        // =========================================================
        // APPROVE CLAIM INTENT
        // =========================================================

        private static bool IsApproveClaimIntent(
            string message)
        {
            return
                message.Contains(
                    "approve claim") ||

                message.Contains(
                    "approve the claim") ||

                message.Contains(
                    "approve this claim") ||

                Regex.IsMatch(
                    message,
                    @"\bapprove\s+clm[0-9a-f]{8}\b",
                    RegexOptions.IgnoreCase);
        }

        // =========================================================
        // APPROVE CLAIM
        // =========================================================

        private async Task<AiChatResponse>
            HandleApproveClaimAsync(
                Guid? claimId,
                string originalMessage)
        {
            var currentUser =
                _httpContextAccessor.HttpContext?.User;

            if (currentUser == null)
            {
                return ClaimAccessDenied();
            }

            var currentUserId =
                _currentUserService.UserId;

            if (!currentUserId.HasValue)
            {
                return new AiChatResponse
                {
                    Success = false,

                    Message =
                        "Unable to determine the logged-in approver.",

                    Intent =
                        "APPROVE_CLAIM"
                };
            }

            var databaseUser =
                await _userRepository.GetByIdAsync(
                    currentUserId.Value);

            if (databaseUser == null ||
                databaseUser.RoleId != RoleConstants.ApproverId)
            {
                return new AiChatResponse
                {
                    Success = false,

                    Message =
                        "Only an Approver can approve a claim.",

                    Intent =
                        "APPROVAL_ACCESS_DENIED"
                };
            }

            var resolvedClaimId =
                claimId ??
                await GetClaimIdFromMessageAsync(
                    originalMessage);

            if (!resolvedClaimId.HasValue)
            {
                return new AiChatResponse
                {
                    Success = false,

                    Message =
                        "Please provide the Claim Number. " +
                        "For example: Approve claim CLM3480FEF7.",

                    Intent =
                        "APPROVE_CLAIM"
                };
            }

            var claim =
                await _claimService.GetClaimByIdAsync(
                    resolvedClaimId.Value);

            if (claim == null)
            {
                return ClaimNotFound(
                    "APPROVE_CLAIM");
            }

            var statusId =
                Convert.ToInt32(
                    claim.StatusId);

            if (statusId != ClaimStatusConstants.RepairInProgress)
            {
                return new AiChatResponse
                {
                    Success = false,

                    Message =
                        $"Claim {claim.ClaimNumber} cannot be approved " +
                        $"because its current status is " +
                        $"{GetClaimStatusName(statusId)}.",

                    Intent =
                        "APPROVE_CLAIM",

                    ClaimId =
                        claim.ClaimId
                };
            }

            var customerName =
                await GetCustomerNameAsync(
                    claim.CustomerId);

            var amount =
                claim.EstimatedLossAmount
                ?? 0m;

            return new AiChatResponse
            {
                Success = true,

                RequiresConfirmation = true,

                Message =
                    $"Claim {claim.ClaimNumber} for {customerName} " +
                    $"is ready for approval. " +
                    $"The estimated loss amount is ₹ {amount:N2}. " +
                    $"If approved, the claim status will change " +
                    $"to Approved and the approved amount will be " +
                    $"₹ {amount:N2}. " +
                    "Please explicitly confirm if you want to proceed.",

                Intent =
                    "APPROVE_CLAIM",

                Action =
                    "APPROVE_CLAIM",

                ClaimId =
                    claim.ClaimId
            };
        }

        // =========================================================
        // CONFIRMED ACTION
        // =========================================================

        private async Task<AiChatResponse>
            HandleConfirmedActionAsync(
                Guid? claimId)
        {
            if (!claimId.HasValue)
            {
                return ClaimIdRequired(
                    "Please provide the Claim ID so I can process the confirmed action.");
            }

            var currentUser =
                _httpContextAccessor.HttpContext?.User;

            if (currentUser == null)
            {
                return ClaimAccessDenied();
            }

            var currentUserId =
                _currentUserService.UserId;

            if (!currentUserId.HasValue)
            {
                return ClaimAccessDenied();
            }

            var databaseUser =
                await _userRepository.GetByIdAsync(
                    currentUserId.Value);

            // =====================================================
            // APPROVER CONFIRMATION
            // =====================================================

            if (databaseUser != null &&
                databaseUser.RoleId == RoleConstants.ApproverId)
            {
                return await ExecuteApprovalAsync(
                    claimId.Value);
            }

            // =====================================================
            // CUSTOMER CLOSURE
            // =====================================================

            var claim =
                await _claimService.GetClaimByIdAsync(
                    claimId.Value);

            if (claim == null)
            {
                return ClaimNotFound(
                    "CLOSE_CLAIM");
            }

            var statusId =
                Convert.ToInt32(
                    claim.StatusId);

            if (statusId == ClaimStatusConstants.Closed)
            {
                return new AiChatResponse
                {
                    Success = true,

                    Message =
                        $"Your claim {claim.ClaimNumber} is already Closed. " +
                        "No action is required.",

                    Intent =
                        "CLOSE_CLAIM"
                };
            }

            if (statusId != ClaimStatusConstants.Settled)
            {
                return new AiChatResponse
                {
                    Success = false,

                    Message =
                        $"Your claim {claim.ClaimNumber} is currently " +
                        $"{GetClaimStatusName(statusId)}. " +
                        "Only a Settled claim can be closed.",

                    Intent =
                        "CLOSE_CLAIM",

                    ClaimId =
                        claim.ClaimId
                };
            }

            var closeRequest =
                new CloseClaimRequest
                {
                    Remarks =
                        "Claim closed through ClaimShield AI after explicit user confirmation."
                };

            var closed =
                await _claimClosureService.CloseClaimAsync(
                    claim.ClaimId,
                    closeRequest);

            if (!closed)
            {
                return new AiChatResponse
                {
                    Success = false,

                    Message =
                        $"I could not close claim {claim.ClaimNumber}. " +
                        "The claim-closure operation was rejected.",

                    Intent =
                        "CLOSE_CLAIM",

                    Action =
                        "CLOSE_CLAIM",

                    ClaimId =
                        claim.ClaimId
                };
            }

            return new AiChatResponse
            {
                Success = true,

                RequiresConfirmation = false,

                Message =
                    $"Your claim {claim.ClaimNumber} has been successfully closed. " +
                    "Its current status is Closed.",

                Intent =
                    "CLOSE_CLAIM_COMPLETED",

                Action =
                    "CLOSE_CLAIM",

                ClaimId =
                    claim.ClaimId
            };
        }

        // =========================================================
        // EXECUTE APPROVAL
        // =========================================================

        private async Task<AiChatResponse>
            ExecuteApprovalAsync(
                Guid claimId)
        {
            var currentUser =
                _httpContextAccessor.HttpContext?.User;

            if (currentUser == null)
            {
                return ClaimAccessDenied();
            }

            var currentUserId =
                _currentUserService.UserId;

            if (!currentUserId.HasValue)
            {
                return ClaimAccessDenied();
            }

            var databaseUser =
                await _userRepository.GetByIdAsync(
                    currentUserId.Value);

            if (databaseUser == null ||
                databaseUser.RoleId != RoleConstants.ApproverId)
            {
                return new AiChatResponse
                {
                    Success = false,

                    Message =
                        "Only an Approver can approve a claim.",

                    Intent =
                        "APPROVAL_ACCESS_DENIED"
                };
            }

            var claim =
                await _claimService.GetClaimByIdAsync(
                    claimId);

            if (claim == null)
            {
                return ClaimNotFound(
                    "APPROVE_CLAIM");
            }

            var currentStatus =
                Convert.ToInt32(
                    claim.StatusId);

            // =====================================================
            // ONLY STATUS 6 CAN BE APPROVED
            // =====================================================

            if (currentStatus != ClaimStatusConstants.RepairInProgress)
            {
                return new AiChatResponse
                {
                    Success = false,

                    Message =
                        $"Claim {claim.ClaimNumber} is no longer waiting " +
                        $"for approval. Its current status is " +
                        $"{GetClaimStatusName(currentStatus)}.",

                    Intent =
                        "APPROVE_CLAIM",

                    ClaimId =
                        claim.ClaimId
                };
            }

            var approvedAmount =
                claim.EstimatedLossAmount
                ?? 0m;

            // =====================================================
            // IMPORTANT:
            //
            // UpdateClaimAsync requires UpdateClaimRequest,
            // NOT ClaimResponseDto.
            // =====================================================

            var updateRequest =
                new UpdateClaimRequest
                {
                    ClaimId =
                        claim.ClaimId,

                    PolicyId =
                        claim.PolicyId,

                    CustomerId =
                        claim.CustomerId,

                    VehicleId =
                        claim.VehicleId,

                    ClaimNumber =
                        claim.ClaimNumber,

                    IncidentDate =
                        claim.IncidentDate,

                    ReportedDate =
                        claim.ReportedDate,

                    IncidentLocation =
                        claim.IncidentLocation,

                    IncidentDescription =
                        claim.IncidentDescription,

                    EstimatedLossAmount =
                        claim.EstimatedLossAmount,

                    ApprovedAmount =
                        approvedAmount,

                    IsFraudSuspected =
                        claim.IsFraudSuspected,

                    StatusId =
                        ClaimStatusConstants.Approved
                };

            try
            {
                var updated =
                    await _claimService.UpdateClaimAsync(
                        updateRequest);

                if (!updated)
                {
                    return new AiChatResponse
                    {
                        Success = false,

                        Message =
                            $"Claim {claim.ClaimNumber} could not be approved.",

                        Intent =
                            "APPROVE_CLAIM",

                        ClaimId =
                            claim.ClaimId
                    };
                }
            }
            catch (Exception ex)
            {
                return new AiChatResponse
                {
                    Success = false,

                    Message =
                        $"The claim approval failed: {ex.Message}",

                    Intent =
                        "APPROVE_CLAIM",

                    ClaimId =
                        claim.ClaimId
                };
            }

            var customerName =
                await GetCustomerNameAsync(
                    claim.CustomerId);

            return new AiChatResponse
            {
                Success = true,

                RequiresConfirmation = false,

                Message =
                    $"Claim {claim.ClaimNumber} for {customerName} " +
                    $"has been approved for ₹ {approvedAmount:N2}. " +
                    "The current claim status is Approved.",

                Intent =
                    "APPROVE_CLAIM_COMPLETED",

                Action =
                    "APPROVE_CLAIM",

                ClaimId =
                    claim.ClaimId
            };
        }

        // =========================================================
        // CLAIM STATUS
        // =========================================================

        private async Task<AiChatResponse>
            HandleClaimStatusAsync(
                Guid? claimId)
        {
            if (!claimId.HasValue)
            {
                return ClaimIdRequired(
                    "Please provide the Claim ID so I can retrieve the claim status.");
            }

            var claim =
                await _claimService.GetClaimByIdAsync(
                    claimId.Value);

            if (claim == null)
            {
                return ClaimNotFound(
                    "GET_CLAIM_STATUS");
            }

            var customerName =
                await GetCustomerNameAsync(
                    claim.CustomerId);

            var status =
                GetClaimStatusName(
                    Convert.ToInt32(
                        claim.StatusId));

            return new AiChatResponse
            {
                Success = true,

                Message =
                    $"The claim for {customerName}, " +
                    $"claim number {claim.ClaimNumber}, " +
                    $"is currently {status}.",

                Intent =
                    "GET_CLAIM_STATUS",

                ClaimId =
                    claim.ClaimId
            };
        }

        // =========================================================
        // CLAIM DETAILS
        // =========================================================

        private async Task<AiChatResponse>
            HandleClaimDetailsAsync(
                Guid? claimId)
        {
            if (!claimId.HasValue)
            {
                return ClaimIdRequired(
                    "Please provide the Claim ID so I can retrieve your claim information.");
            }

            var claim =
                await _claimService.GetClaimByIdAsync(
                    claimId.Value);

            if (claim == null)
            {
                return ClaimNotFound(
                    "GET_CLAIM_DETAILS");
            }

            var customerName =
                await GetCustomerNameAsync(
                    claim.CustomerId);

            var status =
                GetClaimStatusName(
                    Convert.ToInt32(
                        claim.StatusId));

            var approvedAmount =
                claim.ApprovedAmount.HasValue
                    ? $"₹ {claim.ApprovedAmount.Value:N2}"
                    : "Not approved yet";

            return new AiChatResponse
            {
                Success = true,

                Message =
                    $"The claim for {customerName}, " +
                    $"claim number {claim.ClaimNumber}, " +
                    $"has an approved amount of {approvedAmount}. " +
                    $"The current claim status is {status}.",

                Intent =
                    "GET_CLAIM_DETAILS",

                ClaimId =
                    claim.ClaimId
            };
        }

        // =========================================================
        // PAYMENT STATUS
        // =========================================================

        private async Task<AiChatResponse>
            HandlePaymentStatusAsync(
                Guid? claimId)
        {
            if (!claimId.HasValue)
            {
                return ClaimIdRequired(
                    "Please provide the Claim ID so I can retrieve the payment information.");
            }

            var payments =
                await _paymentService.GetByClaimAsync(
                    claimId.Value);

            var paymentList =
                payments?.ToList();

            if (paymentList == null ||
                paymentList.Count == 0)
            {
                return new AiChatResponse
                {
                    Success = true,

                    Message =
                        "No payment record was found for this claim.",

                    Intent =
                        "GET_PAYMENT_STATUS",

                    ClaimId =
                        claimId.Value
                };
            }

            var payment =
                paymentList
                    .OrderByDescending(
                        x =>
                            x.CreatedDate)
                    .FirstOrDefault();

            if (payment == null)
            {
                return new AiChatResponse
                {
                    Success = true,

                    Message =
                        "No payment record was found for this claim.",

                    Intent =
                        "GET_PAYMENT_STATUS",

                    ClaimId =
                        claimId.Value
                };
            }

            return new AiChatResponse
            {
                Success = true,

                Message =
                    $"The latest payment for this claim is " +
                    $"₹ {payment.Amount:N2}. " +
                    $"Its current payment status is " +
                    $"{payment.PaymentStatus}.",

                Intent =
                    "GET_PAYMENT_STATUS",

                ClaimId =
                    claimId.Value
            };
        }

        // =========================================================
        // SURVEY STATUS
        // =========================================================

        private async Task<AiChatResponse>
            HandleSurveyStatusAsync(
                Guid? claimId)
        {
            if (!claimId.HasValue)
            {
                return ClaimIdRequired(
                    "Please provide the Claim ID so I can retrieve the survey information.");
            }

            var surveys =
                await _surveyAssignmentService.GetByClaimAsync(
                    claimId.Value);

            var surveyList =
                surveys?.ToList();

            if (surveyList == null ||
                surveyList.Count == 0)
            {
                return new AiChatResponse
                {
                    Success = true,

                    Message =
                        "No survey assignment was found for this claim.",

                    Intent =
                        "GET_SURVEY_STATUS",

                    ClaimId =
                        claimId.Value
                };
            }

            var survey =
                surveyList
                    .OrderByDescending(
                        x =>
                            x.AssignedDate ??
                            DateTime.MinValue)
                    .FirstOrDefault();

            if (survey == null)
            {
                return new AiChatResponse
                {
                    Success = true,

                    Message =
                        "A survey assignment exists, but its details could not be determined.",

                    Intent =
                        "GET_SURVEY_STATUS",

                    ClaimId =
                        claimId.Value
                };
            }

            var surveyor =
                await _userRepository.GetByIdAsync(
                    survey.SurveyorId);

            var surveyorName =
                GetUserDisplayName(
                    surveyor);

            var surveyStatus =
                GetAssignmentStatusName(
                    survey.AssignmentStatusId);

            return new AiChatResponse
            {
                Success = true,

                Message =
                    $"The survey for this claim is assigned to " +
                    $"{surveyorName}. " +
                    $"The current survey status is " +
                    $"{surveyStatus}.",

                Intent =
                    "GET_SURVEY_STATUS",

                ClaimId =
                    claimId.Value
            };
        }

        // =========================================================
        // REPAIR STATUS
        // =========================================================

        private async Task<AiChatResponse>
            HandleRepairStatusAsync(
                Guid? claimId)
        {
            if (!claimId.HasValue)
            {
                return ClaimIdRequired(
                    "Please provide the Claim ID so I can retrieve the repair information.");
            }

            var repairs =
                await _repairAssignmentService.GetByClaimAsync(
                    claimId.Value);

            var repairList =
                repairs?.ToList();

            if (repairList == null ||
                repairList.Count == 0)
            {
                return new AiChatResponse
                {
                    Success = true,

                    Message =
                        "No repair or service assignment was found for this claim.",

                    Intent =
                        "GET_REPAIR_STATUS",

                    ClaimId =
                        claimId.Value
                };
            }

            var repair =
                repairList
                    .OrderByDescending(
                        x =>
                            x.AssignedDate ??
                            DateTime.MinValue)
                    .FirstOrDefault();

            if (repair == null)
            {
                return new AiChatResponse
                {
                    Success = true,

                    Message =
                        "A repair assignment exists, but its details could not be determined.",

                    Intent =
                        "GET_REPAIR_STATUS",

                    ClaimId =
                        claimId.Value
                };
            }

            var repairer =
                await _userRepository.GetByIdAsync(
                    repair.RepairerId);

            var repairerName =
                GetUserDisplayName(
                    repairer);

            var repairStatus =
                GetAssignmentStatusName(
                    repair.AssignmentStatusId);

            return new AiChatResponse
            {
                Success = true,

                Message =
                    $"The repair for this claim is assigned to " +
                    $"{repairerName}. " +
                    $"The current repair status is " +
                    $"{repairStatus}.",

                Intent =
                    "GET_REPAIR_STATUS",

                ClaimId =
                    claimId.Value
            };
        }

        // =========================================================
        // DOCUMENTS
        // =========================================================

        private async Task<AiChatResponse>
            HandleDocumentsAsync(
                Guid? claimId)
        {
            if (!claimId.HasValue)
            {
                return ClaimIdRequired(
                    "Please provide the Claim ID so I can retrieve the claim documents.");
            }

            var documents =
                await _claimDocumentService.GetByClaimAsync(
                    claimId.Value);

            var documentList =
                documents?.ToList();

            if (documentList == null ||
                documentList.Count == 0)
            {
                return new AiChatResponse
                {
                    Success = true,

                    Message =
                        "No documents were found for this claim.",

                    Intent =
                        "GET_DOCUMENTS",

                    ClaimId =
                        claimId.Value
                };
            }

            var documentNames =
                string.Join(
                    ", ",
                    documentList.Select(
                        x => x.FileName));

            return new AiChatResponse
            {
                Success = true,

                Message =
                    $"I found {documentList.Count} document(s) " +
                    $"for this claim: {documentNames}.",

                Intent =
                    "GET_DOCUMENTS",

                ClaimId =
                    claimId.Value
            };
        }

        // =========================================================
        // MULTI INTENT
        // =========================================================

        private async Task<AiChatResponse>
            HandleMultiIntentAsync(
                Guid? claimId,
                List<string> intents)
        {
            if (!claimId.HasValue)
            {
                return ClaimIdRequired(
                    "Please provide the Claim ID so I can retrieve the requested claim information.");
            }

            var authorized =
                await IsClaimAccessibleByCurrentUserAsync(
                    claimId.Value);

            if (!authorized)
            {
                return ClaimAccessDenied();
            }

            var claim =
                await _claimService.GetClaimByIdAsync(
                    claimId.Value);

            if (claim == null)
            {
                return ClaimNotFound(
                    "MULTI_INTENT");
            }

            var customerName =
                await GetCustomerNameAsync(
                    claim.CustomerId);

            var sections =
                new List<string>();

            // =====================================================
            // CLAIM
            // =====================================================

            if (intents.Contains(
                    "GET_CLAIM_STATUS") ||
                intents.Contains(
                    "GET_CLAIM_DETAILS"))
            {
                var status =
                    GetClaimStatusName(
                        Convert.ToInt32(
                            claim.StatusId));

                sections.Add(
                    $"• Customer: {customerName}");

                sections.Add(
                    $"• Claim number: {claim.ClaimNumber}");

                sections.Add(
                    $"• Claim status: {status}");
            }

            // =====================================================
            // APPROVED AMOUNT
            // =====================================================

            if (intents.Contains(
                    "GET_CLAIM_DETAILS"))
            {
                var amount =
                    claim.ApprovedAmount.HasValue
                        ? $"₹ {claim.ApprovedAmount.Value:N2}"
                        : "Not approved yet";

                sections.Add(
                    $"• Approved amount: {amount}");
            }

            // =====================================================
            // PAYMENT
            // =====================================================

            if (intents.Contains(
                    "GET_PAYMENT_STATUS"))
            {
                var payments =
                    await _paymentService.GetByClaimAsync(
                        claimId.Value);

                var paymentList =
                    payments?.ToList();

                if (paymentList != null &&
                    paymentList.Count > 0)
                {
                    var payment =
                        paymentList
                            .OrderByDescending(
                                x => x.CreatedDate)
                            .FirstOrDefault();

                    if (payment != null)
                    {
                        sections.Add(
                            $"• Payment: ₹ {payment.Amount:N2}, " +
                            $"{payment.PaymentStatus}");
                    }
                }
                else
                {
                    sections.Add(
                        "• Payment: No payment record was found.");
                }
            }

            // =====================================================
            // SURVEY
            // =====================================================

            if (intents.Contains(
                    "GET_SURVEY_STATUS"))
            {
                var surveys =
                    await _surveyAssignmentService.GetByClaimAsync(
                        claimId.Value);

                var surveyList =
                    surveys?.ToList();

                if (surveyList != null &&
                    surveyList.Count > 0)
                {
                    var survey =
                        surveyList
                            .OrderByDescending(
                                x =>
                                    x.AssignedDate ??
                                    DateTime.MinValue)
                            .FirstOrDefault();

                    if (survey != null)
                    {
                        var surveyor =
                            await _userRepository.GetByIdAsync(
                                survey.SurveyorId);

                        var surveyorName =
                            GetUserDisplayName(
                                surveyor);

                        var surveyStatus =
                            GetAssignmentStatusName(
                                survey.AssignmentStatusId);

                        sections.Add(
                            $"• Surveyor: {surveyorName}, " +
                            $"{surveyStatus}");
                    }
                }
                else
                {
                    sections.Add(
                        "• Surveyor: No survey assignment was found.");
                }
            }

            // =====================================================
            // REPAIR
            // =====================================================

            if (intents.Contains(
                    "GET_REPAIR_STATUS"))
            {
                var repairs =
                    await _repairAssignmentService.GetByClaimAsync(
                        claimId.Value);

                var repairList =
                    repairs?.ToList();

                if (repairList != null &&
                    repairList.Count > 0)
                {
                    var repair =
                        repairList
                            .OrderByDescending(
                                x =>
                                    x.AssignedDate ??
                                    DateTime.MinValue)
                            .FirstOrDefault();

                    if (repair != null)
                    {
                        var repairer =
                            await _userRepository.GetByIdAsync(
                                repair.RepairerId);

                        var repairerName =
                            GetUserDisplayName(
                                repairer);

                        var repairStatus =
                            GetAssignmentStatusName(
                                repair.AssignmentStatusId);

                        sections.Add(
                            $"• Repairer: {repairerName}, " +
                            $"{repairStatus}");
                    }
                }
                else
                {
                    sections.Add(
                        "• Repairer: No repair assignment was found.");
                }
            }

            // =====================================================
            // DOCUMENTS
            // =====================================================

            if (intents.Contains(
                    "GET_DOCUMENTS"))
            {
                var documents =
                    await _claimDocumentService.GetByClaimAsync(
                        claimId.Value);

                var documentList =
                    documents?.ToList();

                if (documentList != null &&
                    documentList.Count > 0)
                {
                    var documentNames =
                        string.Join(
                            ", ",
                            documentList.Select(
                                x => x.FileName));

                    sections.Add(
                        $"• Documents: {documentNames}");
                }
                else
                {
                    sections.Add(
                        "• Documents: No documents were found.");
                }
            }

            if (sections.Count == 0)
            {
                return GeneralResponse();
            }

            return new AiChatResponse
            {
                Success = true,

                Message =
                    $"Here is the latest information about " +
                    $"claim {claim.ClaimNumber}:\n\n" +
                    string.Join(
                        "\n",
                        sections),

                Intent =
                    "MULTI_INTENT",

                ClaimId =
                    claim.ClaimId
            };
        }

        // =========================================================
        // CLOSE CLAIM
        // =========================================================

        private async Task<AiChatResponse>
            HandleCloseClaimAsync(
                Guid? claimId)
        {
            if (!claimId.HasValue)
            {
                return ClaimIdRequired(
                    "Please provide the Claim ID so I can check whether the claim can be closed.");
            }

            var authorized =
                await IsClaimAccessibleByCurrentUserAsync(
                    claimId.Value);

            if (!authorized)
            {
                return ClaimAccessDenied();
            }

            var claim =
                await _claimService.GetClaimByIdAsync(
                    claimId.Value);

            if (claim == null)
            {
                return ClaimNotFound(
                    "CLOSE_CLAIM");
            }

            var statusId =
                Convert.ToInt32(
                    claim.StatusId);

            if (statusId == ClaimStatusConstants.Closed)
            {
                return new AiChatResponse
                {
                    Success = true,

                    Message =
                        $"Your claim {claim.ClaimNumber} is already Closed. " +
                        "No action is required.",

                    Intent =
                        "CLOSE_CLAIM"
                };
            }

            if (statusId != ClaimStatusConstants.Settled)
            {
                return new AiChatResponse
                {
                    Success = false,

                    Message =
                        $"Your claim {claim.ClaimNumber} is currently " +
                        $"{GetClaimStatusName(statusId)}. " +
                        "Only a Settled claim can be closed.",

                    Intent =
                        "CLOSE_CLAIM",

                    ClaimId =
                        claim.ClaimId
                };
            }

            return new AiChatResponse
            {
                Success = true,

                RequiresConfirmation = true,

                Message =
                    $"Your claim {claim.ClaimNumber} is currently Settled. " +
                    "Closing the claim will change its status to Closed. " +
                    "Please explicitly confirm if you want to proceed.",

                Intent =
                    "CLOSE_CLAIM",

                Action =
                    "CLOSE_CLAIM",

                ClaimId =
                    claim.ClaimId
            };
        }

        // =========================================================
        // CUSTOMER NAME
        // =========================================================

        private async Task<string>
            GetCustomerNameAsync(
                Guid customerId)
        {
            var customer =
                await _customerRepository.GetByIdAsync(
                    customerId);

            if (customer == null)
            {
                return "Unknown customer";
            }

            var user =
                await _userRepository.GetByIdAsync(
                    customer.UserId);

            return GetUserDisplayName(
                user);
        }

        // =========================================================
        // USER NAME
        // =========================================================

        private static string
            GetUserDisplayName(
                User? user)
        {
            if (user == null)
            {
                return "Unknown";
            }

            var firstName =
                user.FirstName?.Trim();

            var lastName =
                user.LastName?.Trim();

            if (!string.IsNullOrWhiteSpace(firstName) &&
                !string.IsNullOrWhiteSpace(lastName))
            {
                return
                    $"{firstName} {lastName}";
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

        // =========================================================
        // CLAIM STATUS
        // =========================================================

        private static string
            GetClaimStatusName(
                int statusId)
        {
            return statusId switch
            {
                ClaimStatusConstants.Submitted => "Submitted",
                ClaimStatusConstants.UnderReview => "Under Review",
                ClaimStatusConstants.SurveyAssigned => "Survey Assigned",
                ClaimStatusConstants.SurveyCompleted => "Survey Completed",
                ClaimStatusConstants.RepairAssigned => "Repair Assigned",
                ClaimStatusConstants.RepairInProgress => "Repair In Progress",
                ClaimStatusConstants.Approved => "Approved",
                ClaimStatusConstants.Rejected => "Rejected",
                ClaimStatusConstants.Settled => "Settled",
                ClaimStatusConstants.Closed => "Closed",
                _ => "Unknown"
            };
        }

        // =========================================================
        // ASSIGNMENT STATUS
        // =========================================================

        private static string
            GetAssignmentStatusName(
                int statusId)
        {
            return statusId switch
            {
                AssignmentStatusConstants.Assigned => "Assigned",
                AssignmentStatusConstants.Accepted => "Accepted",
                AssignmentStatusConstants.InProgress => "In Progress",
                AssignmentStatusConstants.Completed => "Completed",
                AssignmentStatusConstants.Cancelled => "Cancelled",
                _ => "Unknown"
            };
        }

        // =========================================================
        // INTENT DETECTION
        // =========================================================

        private static List<string>
            DetectAllIntents(
                string message)
        {
            var intents =
                new List<string>();

            // =====================================================
            // PAYMENT
            // =====================================================

            if (
                message.Contains("payment") ||
                message.Contains("paid") ||
                message.Contains("payout") ||
                message.Contains("payment status") ||
                message.Contains("status of my payment") ||
                message.Contains("status of payment") ||
                message.Contains("claim payment") ||
                message.Contains("payment received") ||
                message.Contains("payment done") ||
                message.Contains("payment completed") ||
                message.Contains("have i been paid") ||
                message.Contains("did i get paid") ||
                message.Contains("did i receive my payment") ||
                message.Contains("when will i get paid") ||
                message.Contains("when will i get my payment") ||
                message.Contains("how much was paid") ||
                message.Contains("how much did i receive") ||
                message.Contains("payment amount"))
            {
                intents.Add(
                    "GET_PAYMENT_STATUS");
            }

            // =====================================================
            // SURVEY
            // =====================================================

            if (
                message.Contains("survey") ||
                message.Contains("surveyor") ||
                message.Contains("survey status") ||
                message.Contains("status of survey") ||
                message.Contains("status of my survey") ||
                message.Contains("who is my surveyor") ||
                message.Contains("who's my surveyor") ||
                message.Contains("who is handling my survey") ||
                message.Contains("who's handling my survey") ||
                message.Contains("inspection") ||
                message.Contains("inspector"))
            {
                intents.Add(
                    "GET_SURVEY_STATUS");
            }

            // =====================================================
            // REPAIR
            // =====================================================

            if (
                message.Contains("repair") ||
                message.Contains("repairer") ||
                message.Contains("who is my repairer") ||
                message.Contains("who's my repairer") ||
                message.Contains("garage") ||
                message.Contains("workshop") ||
                message.Contains("service") ||
                message.Contains("servicing") ||
                message.Contains("service status") ||
                message.Contains("vehicle repair") ||
                message.Contains("car repair") ||
                message.Contains("repair status") ||
                message.Contains("status of repair") ||
                message.Contains("status of my repair"))
            {
                intents.Add(
                    "GET_REPAIR_STATUS");
            }

            // =====================================================
            // DOCUMENTS
            // =====================================================

            if (
                message.Contains("document") ||
                message.Contains("documents") ||
                message.Contains("file") ||
                message.Contains("files") ||
                message.Contains("uploaded") ||
                message.Contains("upload") ||
                message.Contains("paperwork") ||
                message.Contains("attachment") ||
                message.Contains("attachments") ||
                message.Contains("what did i upload") ||
                message.Contains("what documents did i upload"))
            {
                intents.Add(
                    "GET_DOCUMENTS");
            }

            // =====================================================
            // CLAIM DETAILS
            // =====================================================

            if (
                message.Contains("claim details") ||
                message.Contains("claim detail") ||
                message.Contains("claim information") ||
                message.Contains("approved amount") ||
                message.Contains("approval amount") ||
                message.Contains("how much was approved") ||
                message.Contains("how much will i get") ||
                message.Contains("how much do i get") ||
                message.Contains("claim amount") ||
                message.Contains("approved money") ||
                message.Contains("approved payment") ||
                message.Contains("is my claim approved") ||
                message.Contains("did my claim get approved") ||
                message.Contains("has my claim been approved") ||
                message.Contains("claim approved"))
            {
                intents.Add(
                    "GET_CLAIM_DETAILS");
            }

            // =====================================================
            // CLAIM STATUS
            // =====================================================

            if (
                message.Contains("claim status") ||
                message.Contains("status of my claim") ||
                message.Contains("status of claim") ||
                message.Contains("what is my claim status") ||
                message.Contains("current claim status") ||
                message.Contains("where is my claim") ||
                message.Contains("where is claim") ||
                message.Contains("where does my claim stand") ||
                message.Contains("what happened to my claim") ||
                message.Contains("what happened to claim") ||
                message.Contains("what is happening with my claim") ||
                message.Contains("claim progress") ||
                message.Contains("progress of my claim"))
            {
                intents.Add(
                    "GET_CLAIM_STATUS");
            }

            return intents
                .Distinct()
                .ToList();
        }

        // =========================================================
        // PENDING APPROVAL INTENT
        // =========================================================

        private static bool
            IsPendingApprovalIntent(
                string message)
        {
            return
                message.Contains(
                    "which claims are waiting for my approval") ||

                message.Contains(
                    "which claims are pending approval") ||

                message.Contains(
                    "what claims are waiting for approval") ||

                message.Contains(
                    "what claims need my approval") ||

                message.Contains(
                    "claims waiting for approval") ||

                message.Contains(
                    "claims pending approval") ||

                message.Contains(
                    "pending approvals") ||

                message.Contains(
                    "show pending approvals") ||

                message.Contains(
                    "show claims waiting for approval");
        }

        // =========================================================
        // CLOSE INTENT
        // =========================================================

        private static bool
            IsCloseClaimIntent(
                string message)
        {
            return
                message.Contains("close my claim") ||
                message.Contains("close the claim") ||
                message.Contains("close claim") ||
                message.Contains("close this claim") ||
                message.Contains("close my case") ||
                message.Contains("close the case") ||
                message.Contains("i want to close my claim") ||
                message.Contains("i want my claim closed") ||
                message.Contains("can you close my claim") ||
                message.Contains("please close my claim") ||
                message.Contains("finish my claim") ||
                message.Contains("complete my claim");
        }

        // =========================================================
        // CONFIRMATION
        // =========================================================

        private static bool
            IsConfirmation(
                string message)
        {
            return
                message == "yes" ||
                message == "yes please" ||
                message == "yeah" ||
                message == "yeah please" ||
                message == "yep" ||
                message == "yup" ||
                message == "sure" ||
                message == "sure please" ||
                message == "confirm" ||
                message == "confirmed" ||
                message == "i confirm" ||
                message == "proceed" ||
                message == "proceed please" ||
                message == "go ahead" ||
                message == "go ahead please" ||
                message == "do it" ||
                message == "do that" ||
                message == "okay" ||
                message == "ok" ||
                message == "okay please";
        }

        // =========================================================
        // CANCELLATION
        // =========================================================

        private static bool
            IsCancellation(
                string message)
        {
            return
                message == "no" ||
                message == "no thanks" ||
                message == "no thank you" ||
                message == "cancel" ||
                message == "cancel it" ||
                message == "don't do it" ||
                message == "do not do it" ||
                message == "stop" ||
                message == "never mind" ||
                message == "nevermind";
        }

        // =========================================================
        // CLAIM ID REQUIRED
        // =========================================================

        private static AiChatResponse
            ClaimIdRequired(
                string message)
        {
            return new AiChatResponse
            {
                Success = false,

                Message =
                    message,

                Intent =
                    "CLAIM_ID_REQUIRED"
            };
        }

        // =========================================================
        // CLAIM NOT FOUND
        // =========================================================

        private static AiChatResponse
            ClaimNotFound(
                string intent)
        {
            return new AiChatResponse
            {
                Success = false,

                Message =
                    "I could not find a claim with the provided Claim ID or Claim Number.",

                Intent =
                    intent
            };
        }

        // =========================================================
        // ACCESS DENIED
        // =========================================================

        private static AiChatResponse
            ClaimAccessDenied()
        {
            return new AiChatResponse
            {
                Success = false,

                Message =
                    "You are not authorized to access this claim.",

                Intent =
                    "CLAIM_ACCESS_DENIED"
            };
        }

        // =========================================================
        // GENERAL RESPONSE
        // =========================================================

        private static AiChatResponse
            GeneralResponse()
        {
            return new AiChatResponse
            {
                Success = true,

                Message =
                    "Hello! I am the ClaimShield development AI. " +
                    "I can help with claim status, claim details, " +
                    "payments, documents, surveys, repairs, " +
                    "approvals, and claim closure. " +
                    "For claim-specific questions, please provide the Claim ID or Claim Number.",

                Intent =
                    "GENERAL_CHAT"
            };
        }
    }
}