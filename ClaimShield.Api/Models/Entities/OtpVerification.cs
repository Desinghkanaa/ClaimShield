using System.ComponentModel.DataAnnotations;

namespace ClaimShield.Api.Models.Entities
{
    // Shared by both OTP checkpoints (Login and InstantClaimAccept) -
    // one reusable mechanism, per the Phase 12 spec's own "reuse the
    // pattern" instruction. CodeHash is a SHA-256 hash - plaintext
    // codes are never persisted.
    public class OtpVerification
    {
        [Key]
        public Guid OtpVerificationId { get; set; }

        [MaxLength(50)]
        public string Purpose { get; set; } = string.Empty;

        // UserId for Purpose=Login, ClaimId for Purpose=InstantClaimAccept.
        public Guid SubjectId { get; set; }

        [MaxLength(200)]
        public string CodeHash { get; set; } = string.Empty;

        public DateTime ExpiresAt { get; set; }

        public DateTime? VerifiedAt { get; set; }

        // Set when a verified OTP is actually consumed by the action it
        // gated (e.g. Instant Claim Accept) - prevents replaying the
        // same verified code across two accept attempts.
        public DateTime? ConsumedAt { get; set; }

        public int AttemptCount { get; set; }

        public DateTime CreatedDate { get; set; }
    }
}
