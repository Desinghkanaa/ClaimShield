using ClaimShield.Api.AI.Models;

namespace ClaimShield.Api.AI.Interfaces
{
    public interface IAiService
    {
        Task<AiChatResponse> ChatAsync(
            AiChatRequest request);
    }
}