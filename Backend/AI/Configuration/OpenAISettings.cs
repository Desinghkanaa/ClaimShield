namespace ClaimShield.Api.AI.Configuration
{
    public class OpenAISettings
    {
        public string ApiKey { get; set; } = string.Empty;

        public string Model { get; set; } = "gpt-5-mini";
    }
}