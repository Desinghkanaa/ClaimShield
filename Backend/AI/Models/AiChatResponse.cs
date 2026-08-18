namespace ClaimShield.Api.AI.Models
{
    public class AiChatResponse
    {
        // =====================================================
        // RESULT
        // =====================================================

        public bool Success { get; set; }

        // =====================================================
        // MESSAGE
        // =====================================================

        public string Message { get; set; } = string.Empty;

        // =====================================================
        // INTENT
        // =====================================================

        public string? Intent { get; set; }

        // =====================================================
        // CONFIRMATION
        // =====================================================
        //
        // True when the AI requires the user to explicitly
        // confirm a sensitive operation.
        //

        public bool RequiresConfirmation { get; set; }

        // =====================================================
        // ACTION
        // =====================================================
        //
        // Example:
        //     CLOSE_CLAIM
        //

        public string? Action { get; set; }

        // =====================================================
        // CLAIM ID
        // =====================================================
        //
        // Identifies the claim associated with the response.
        //

        public Guid? ClaimId { get; set; }
    }
}