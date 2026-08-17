namespace ClaimShield.Api.Models.DTOs.ClaimScoring
{
    // =================================================================
    // Result of a role-gated scoring lookup. Central place for the
    // Customer/Surveyor/Approver/Admin visibility rule so both
    // ClaimsController and the backward-compatible ClaimAiInsightsController
    // alias enforce identically - never duplicated per-endpoint, since a
    // divergence here would be a real leakage risk, not just a UI bug.
    // =================================================================

    public class ScoringAccessResult
    {
        public bool ClaimFound { get; set; }

        public bool Authorized { get; set; }

        public CustomerClaimScoreDto? CustomerView { get; set; }

        public InternalClaimScoringDto? InternalView { get; set; }
    }
}
