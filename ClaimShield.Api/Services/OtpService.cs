using System.Security.Cryptography;
using System.Text;

using ClaimShield.Api.Constants;
using ClaimShield.Api.Data.Context;
using ClaimShield.Api.Interfaces.Services;
using ClaimShield.Api.Models.DTOs.Otp;
using ClaimShield.Api.Models.Entities;

using Microsoft.EntityFrameworkCore;

namespace ClaimShield.Api.Services
{
    // =================================================================
    // Shared by both OTP checkpoints (Login and InstantClaimAccept).
    // Codes are real (random, expiring, single-use, hashed at rest) -
    // there is no SMS vendor configured in this environment, so the
    // delivery channel is dev-mode (the code is returned in the send
    // response only when IsDevelopment()). This is a genuine working
    // local equivalent per the Phase 12 Build Directive, not a bypass:
    // the code still must be correctly entered, and verification is
    // fully enforced (expiry, attempt limits, single-use).
    // =================================================================

    public class OtpService : IOtpService
    {
        private const int CodeLength = 6;
        private static readonly TimeSpan ExpiryWindow = TimeSpan.FromMinutes(5);
        private const int MaxAttempts = 5;

        private readonly ClaimShieldDbContext _context;
        private readonly IAuditLogService _auditLogService;
        private readonly IWebHostEnvironment _environment;

        public OtpService(
            ClaimShieldDbContext context,
            IAuditLogService auditLogService,
            IWebHostEnvironment environment)
        {
            _context = context;
            _auditLogService = auditLogService;
            _environment = environment;
        }

        public async Task<OtpSendResultDto> SendAsync(
            string purpose,
            Guid subjectId)
        {
            // Invalidate any prior unverified code for this
            // purpose+subject so only the latest one is valid.
            var pending =
                await _context.OtpVerifications
                    .Where(
                        x =>
                            x.Purpose == purpose &&
                            x.SubjectId == subjectId &&
                            x.VerifiedAt == null)
                    .ToListAsync();

            foreach (var p in pending)
            {
                p.ExpiresAt = DateTime.UtcNow;
            }

            var code = GenerateCode();

            var otp = new OtpVerification
            {
                OtpVerificationId = Guid.NewGuid(),
                Purpose = purpose,
                SubjectId = subjectId,
                CodeHash = Hash(code),
                ExpiresAt = DateTime.UtcNow.Add(ExpiryWindow),
                AttemptCount = 0,
                CreatedDate = DateTime.UtcNow
            };

            _context.OtpVerifications.Add(otp);

            await _context.SaveChangesAsync();

            await _auditLogService.LogAsync(
                null,
                $"Otp.Sent.{purpose}",
                "OtpVerification",
                otp.OtpVerificationId,
                null,
                new { purpose, subjectId, otp.ExpiresAt });

            return new OtpSendResultDto
            {
                Success = true,
                Message = _environment.IsDevelopment()
                    ? "OTP generated (dev mode - shown below, no SMS provider configured)."
                    : "OTP sent to your registered mobile number.",
                ExpiresAt = otp.ExpiresAt,
                DevModeCode = _environment.IsDevelopment() ? code : null
            };
        }

        public async Task<OtpVerifyResultDto> VerifyAsync(
            string purpose,
            Guid subjectId,
            string code)
        {
            var otp =
                await _context.OtpVerifications
                    .Where(
                        x =>
                            x.Purpose == purpose &&
                            x.SubjectId == subjectId &&
                            x.VerifiedAt == null)
                    .OrderByDescending(x => x.CreatedDate)
                    .FirstOrDefaultAsync();

            if (otp == null)
            {
                return Result(
                    OtpVerifyResultConstants.NotFound,
                    "No OTP request found. Please request a new code.");
            }

            if (otp.AttemptCount >= MaxAttempts)
            {
                return Result(
                    OtpVerifyResultConstants.LockedOut,
                    "Too many incorrect attempts. Please request a new code.");
            }

            if (otp.ExpiresAt < DateTime.UtcNow)
            {
                return Result(
                    OtpVerifyResultConstants.Expired,
                    "This code has expired. Please request a new one.");
            }

            otp.AttemptCount += 1;

            if (otp.CodeHash != Hash(code))
            {
                await _context.SaveChangesAsync();

                return Result(
                    OtpVerifyResultConstants.Incorrect,
                    "Incorrect code. Please try again.");
            }

            otp.VerifiedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            await _auditLogService.LogAsync(
                null,
                $"Otp.Verified.{purpose}",
                "OtpVerification",
                otp.OtpVerificationId,
                null,
                new { purpose, subjectId });

            return Result(
                OtpVerifyResultConstants.Success,
                "OTP verified successfully.");
        }

        public async Task<bool> ConsumeFreshVerificationAsync(
            string purpose,
            Guid subjectId,
            TimeSpan freshWithin)
        {
            var otp =
                await _context.OtpVerifications
                    .Where(
                        x =>
                            x.Purpose == purpose &&
                            x.SubjectId == subjectId &&
                            x.VerifiedAt != null &&
                            x.ConsumedAt == null)
                    .OrderByDescending(x => x.VerifiedAt)
                    .FirstOrDefaultAsync();

            if (otp == null ||
                otp.VerifiedAt == null ||
                otp.VerifiedAt.Value.Add(freshWithin) < DateTime.UtcNow)
            {
                return false;
            }

            otp.ConsumedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return true;
        }

        private static string GenerateCode()
        {
            var value = RandomNumberGenerator.GetInt32(0, 1_000_000);
            return value.ToString($"D{CodeLength}");
        }

        private static string Hash(
            string code)
        {
            var bytes = Encoding.UTF8.GetBytes(code);
            var hash = SHA256.HashData(bytes);
            return Convert.ToHexString(hash);
        }

        private static OtpVerifyResultDto Result(
            int resultCode,
            string message)
        {
            return new OtpVerifyResultDto
            {
                Success = resultCode == OtpVerifyResultConstants.Success,
                ResultCode = resultCode,
                Message = message
            };
        }
    }
}
