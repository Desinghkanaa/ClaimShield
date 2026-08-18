namespace ClaimShield.Api.AI.Models
{
    public class AiActionResponse
    {
        public bool Success { get; set; }

        public bool RequiresConfirmation { get; set; }

        public string? Message { get; set; }

        public string? Intent { get; set; }

        public string? Action { get; set; }

        public Guid? ClaimId { get; set; }
    }
}