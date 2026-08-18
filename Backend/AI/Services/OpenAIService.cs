using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using ClaimShield.Api.AI.Configuration;
using ClaimShield.Api.AI.Interfaces;
using ClaimShield.Api.AI.Models;
using Microsoft.Extensions.Options;

namespace ClaimShield.Api.AI.Services
{
    public class OpenAIService : IAiService
    {
        private readonly HttpClient _httpClient;
        private readonly OpenAISettings _settings;

        public OpenAIService(
            HttpClient httpClient,
            IOptions<OpenAISettings> settings)
        {
            _httpClient = httpClient;
            _settings = settings.Value;
        }

        public async Task<AiChatResponse> ChatAsync(
            AiChatRequest request)
        {
            if (string.IsNullOrWhiteSpace(
                _settings.ApiKey))
            {
                throw new InvalidOperationException(
                    "OpenAI API key is not configured.");
            }

            if (string.IsNullOrWhiteSpace(
                request.Message))
            {
                return new AiChatResponse
                {
                    Success = false,
                    Message = "Please provide a message.",
                    Intent = null
                };
            }

            var payload = new
            {
                model = _settings.Model,

                input = request.Message
            };

            using var httpRequest =
                new HttpRequestMessage(
                    HttpMethod.Post,
                    "https://api.openai.com/v1/responses");

            httpRequest.Headers.Authorization =
                new AuthenticationHeaderValue(
                    "Bearer",
                    _settings.ApiKey);

            httpRequest.Content =
                new StringContent(
                    JsonSerializer.Serialize(payload),
                    Encoding.UTF8,
                    "application/json");

            using var response =
                await _httpClient.SendAsync(
                    httpRequest);

            var responseBody =
                await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException(
                    $"OpenAI API request failed. " +
                    $"Status: {(int)response.StatusCode}. " +
                    $"Details: {responseBody}");
            }

            using var json =
                JsonDocument.Parse(responseBody);

            var root = json.RootElement;

            var outputText = string.Empty;

            if (root.TryGetProperty(
                    "output",
                    out var output))
            {
                foreach (var item in output.EnumerateArray())
                {
                    if (!item.TryGetProperty(
                            "content",
                            out var content))
                    {
                        continue;
                    }

                    foreach (
                        var contentItem
                        in content.EnumerateArray())
                    {
                        if (
                            contentItem.TryGetProperty(
                                "type",
                                out var type) &&
                            type.GetString() ==
                                "output_text" &&
                            contentItem.TryGetProperty(
                                "text",
                                out var text))
                        {
                            outputText =
                                text.GetString() ?? string.Empty;

                            break;
                        }
                    }

                    if (!string.IsNullOrWhiteSpace(
                        outputText))
                    {
                        break;
                    }
                }
            }

            if (string.IsNullOrWhiteSpace(
                outputText))
            {
                outputText =
                    "The AI service returned an empty response.";
            }

            return new AiChatResponse
            {
                Success = true,
                Message = outputText,
                Intent = "GENERAL_CHAT"
            };
        }
    }
}