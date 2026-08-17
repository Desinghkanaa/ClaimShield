using ClaimShield.Api.AI.Interfaces;
using ClaimShield.Api.AI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClaimShield.Api.AI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AiController : ControllerBase
    {
        private readonly IAiService _aiService;

        public AiController(
            IAiService aiService)
        {
            _aiService = aiService;
        }

        // =========================================================
        // AI CHAT
        // POST: api/Ai/chat
        // =========================================================

        [HttpPost("chat")]
        public async Task<IActionResult> Chat(
            [FromBody] AiChatRequest request)
        {
            // -----------------------------------------------------
            // VALIDATE REQUEST
            // -----------------------------------------------------

            if (request == null)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Request cannot be null."
                });
            }

            if (string.IsNullOrWhiteSpace(request.Message))
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Message is required."
                });
            }

            // -----------------------------------------------------
            // CALL CLAIMSHIELD AI SERVICE
            // -----------------------------------------------------

            try
            {
                var response =
                    await _aiService.ChatAsync(request);

                return Ok(response);
            }
            catch (InvalidOperationException ex)
            {
                return StatusCode(
                    StatusCodes.Status502BadGateway,
                    new
                    {
                        success = false,
                        message =
                            "AI service request failed.",
                        details = ex.Message
                    });
            }
            catch (HttpRequestException)
            {
                return StatusCode(
                    StatusCodes.Status502BadGateway,
                    new
                    {
                        success = false,
                        message =
                            "Unable to communicate with the AI service."
                    });
            }
            catch (Exception)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new
                    {
                        success = false,
                        message =
                            "An unexpected error occurred while processing the AI request."
                    });
            }
        }
    }
}