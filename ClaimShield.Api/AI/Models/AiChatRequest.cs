namespace ClaimShield.Api.AI.Models
{
    public class AiChatRequest
    {
        public string Message { get; set; } = string.Empty;

        public Guid? ClaimId { get; set; }

        // -----------------------------------------------------
        // Confirmation
        // -----------------------------------------------------
        //
        // Used when the AI has previously requested
        // confirmation for a sensitive action.
        //

        public bool Confirmed { get; set; }
    }
}