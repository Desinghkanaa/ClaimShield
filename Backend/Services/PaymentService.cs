using ClaimShield.Api.Constants;
using ClaimShield.Api.Data.Context;
using ClaimShield.Api.Interfaces.Repositories;
using ClaimShield.Api.Interfaces.Services;
using ClaimShield.Api.Models.DTOs.Payments;
using ClaimShield.Api.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace ClaimShield.Api.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IPaymentRepository _paymentRepository;
        private readonly ClaimShieldDbContext _context;

        public PaymentService(
            IPaymentRepository paymentRepository,
            ClaimShieldDbContext context)
        {
            _paymentRepository = paymentRepository;
            _context = context;
        }

        // =========================================================
        // GET ALL
        // =========================================================

        public async Task<IEnumerable<PaymentResponseDto>> GetAllAsync()
        {
            var payments =
                await _paymentRepository.GetAllAsync();

            return payments.Select(MapToDto);
        }

        // =========================================================
        // GET BY ID
        // =========================================================

        public async Task<PaymentResponseDto?> GetByIdAsync(
            Guid paymentId)
        {
            var payment =
                await _paymentRepository.GetByIdAsync(
                    paymentId);

            if (payment == null)
            {
                return null;
            }

            return MapToDto(payment);
        }

        // =========================================================
        // GET BY CLAIM
        // =========================================================

        public async Task<IEnumerable<PaymentResponseDto>> GetByClaimAsync(
            Guid claimId)
        {
            var payments =
                await _paymentRepository.GetByClaimAsync(
                    claimId);

            return payments.Select(MapToDto);
        }

        // =========================================================
        // CREATE PAYMENT
        // =========================================================

        public async Task<PaymentResponseDto> CreateAsync(
            CreatePaymentRequest request)
        {
            var claim = await _context.Claims
                .FirstOrDefaultAsync(
                    x => x.ClaimId == request.ClaimId);

            if (claim == null)
            {
                throw new InvalidOperationException(
                    "Claim not found.");
            }

            if (claim.StatusId != ClaimStatusConstants.Approved)
            {
                throw new InvalidOperationException(
                    "Payment can only be created for an approved claim.");
            }

            if (!claim.ApprovedAmount.HasValue)
            {
                throw new InvalidOperationException(
                    "Claim does not have an approved amount.");
            }

            if (request.Amount > claim.ApprovedAmount.Value)
            {
                throw new InvalidOperationException(
                    "Payment amount cannot exceed the approved claim amount.");
            }

            if (request.Amount <= 0)
            {
                throw new InvalidOperationException(
                    "Payment amount must be greater than zero.");
            }

            var existingPayments =
                await _paymentRepository.GetByClaimAsync(
                    request.ClaimId);

            var activePayment =
                existingPayments.FirstOrDefault(
                    x =>
                        x.PaymentStatusId == PaymentStatusConstants.Pending ||
                        x.PaymentStatusId == PaymentStatusConstants.Processing ||
                        x.PaymentStatusId == PaymentStatusConstants.Paid);

            if (activePayment != null)
            {
                throw new InvalidOperationException(
                    "An active or completed payment already exists for this claim.");
            }

            var payment = new Payment
            {
                PaymentId = Guid.NewGuid(),

                ClaimId =
                    request.ClaimId,

                Amount =
                    request.Amount,

                PaymentStatusId = PaymentStatusConstants.Pending,

                TransactionReference =
                    request.TransactionReference,

                PaymentDate =
                    request.PaymentDate,

                Remarks =
                    request.Remarks,

                CreatedDate =
                    DateTime.UtcNow
            };

            await _paymentRepository.AddAsync(
                payment);

            return MapToDto(payment);
        }

        // =========================================================
        // PROCESS
        // =========================================================

        public async Task<bool> ProcessAsync(
            Guid paymentId)
        {
            var payment =
                await _paymentRepository.GetByIdAsync(
                    paymentId);

            if (payment == null)
            {
                return false;
            }

            if (payment.PaymentStatusId != PaymentStatusConstants.Pending)
            {
                return false;
            }

            payment.PaymentStatusId = PaymentStatusConstants.Processing;

            await _paymentRepository.UpdateAsync(
                payment);

            return true;
        }

        // =========================================================
        // COMPLETE
        // =========================================================

        public async Task<bool> CompleteAsync(
            Guid paymentId)
        {
            var payment =
                await _paymentRepository.GetByIdAsync(
                    paymentId);

            if (payment == null)
            {
                return false;
            }

            if (payment.PaymentStatusId != PaymentStatusConstants.Processing)
            {
                return false;
            }

            payment.PaymentStatusId = PaymentStatusConstants.Paid;

            payment.PaymentDate =
                DateTime.UtcNow;

            if (string.IsNullOrWhiteSpace(
                payment.TransactionReference))
            {
                payment.TransactionReference =
                    $"CS-PAY-{DateTime.UtcNow:yyyyMMddHHmmss}";
            }

            await _paymentRepository.UpdateAsync(
                payment);

            var claim = await _context.Claims
                .FirstOrDefaultAsync(
                    x => x.ClaimId == payment.ClaimId);

            if (claim != null)
            {
                claim.StatusId = ClaimStatusConstants.Settled;

                claim.UpdatedDate =
                    DateTime.UtcNow;

                await _context.SaveChangesAsync();
            }

            return true;
        }

        // =========================================================
        // FAIL
        // =========================================================

        public async Task<bool> FailAsync(
            Guid paymentId,
            string? remarks)
        {
            var payment =
                await _paymentRepository.GetByIdAsync(
                    paymentId);

            if (payment == null)
            {
                return false;
            }

            // Only Pending or Processing can fail
            if (payment.PaymentStatusId != PaymentStatusConstants.Pending &&
                payment.PaymentStatusId != PaymentStatusConstants.Processing)
            {
                return false;
            }

            payment.PaymentStatusId = PaymentStatusConstants.Failed;

            if (!string.IsNullOrWhiteSpace(remarks))
            {
                payment.Remarks = remarks;
            }

            await _paymentRepository.UpdateAsync(
                payment);

            return true;
        }

        // =========================================================
        // CANCEL
        // =========================================================

        public async Task<bool> CancelAsync(
            Guid paymentId,
            string? remarks)
        {
            var payment =
                await _paymentRepository.GetByIdAsync(
                    paymentId);

            if (payment == null)
            {
                return false;
            }

            // Only Pending or Processing can be cancelled
            if (payment.PaymentStatusId != PaymentStatusConstants.Pending &&
                payment.PaymentStatusId != PaymentStatusConstants.Processing)
            {
                return false;
            }

            payment.PaymentStatusId = PaymentStatusConstants.Cancelled;

            if (!string.IsNullOrWhiteSpace(remarks))
            {
                payment.Remarks = remarks;
            }

            await _paymentRepository.UpdateAsync(
                payment);

            return true;
        }

        // =========================================================
        // DELETE
        // =========================================================

        public async Task<bool> DeleteAsync(
            Guid paymentId)
        {
            var payment =
                await _paymentRepository.GetByIdAsync(
                    paymentId);

            if (payment == null)
            {
                return false;
            }

            // Do not delete Paid payments
            if (payment.PaymentStatusId == PaymentStatusConstants.Paid)
            {
                return false;
            }

            await _paymentRepository.DeleteAsync(
                paymentId);

            return true;
        }

        // =========================================================
        // MAP TO DTO
        // =========================================================

        private static PaymentResponseDto MapToDto(
            Payment payment)
        {
            return new PaymentResponseDto
            {
                PaymentId =
                    payment.PaymentId,

                ClaimId =
                    payment.ClaimId,

                Amount =
                    payment.Amount,

                PaymentStatusId =
                    payment.PaymentStatusId,

                PaymentStatus =
                    GetPaymentStatusName(
                        payment.PaymentStatusId),

                TransactionReference =
                    payment.TransactionReference,

                PaymentDate =
                    payment.PaymentDate,

                Remarks =
                    payment.Remarks,

                CreatedDate =
                    payment.CreatedDate
            };
        }

        // =========================================================
        // STATUS NAME
        // =========================================================

        private static string GetPaymentStatusName(
            int paymentStatusId)
        {
            return paymentStatusId switch
            {
                PaymentStatusConstants.Pending => "Pending",
                PaymentStatusConstants.Processing => "Processing",
                PaymentStatusConstants.Paid => "Paid",
                PaymentStatusConstants.Failed => "Failed",
                PaymentStatusConstants.Cancelled => "Cancelled",
                _ => "Unknown"
            };
        }
    }
}