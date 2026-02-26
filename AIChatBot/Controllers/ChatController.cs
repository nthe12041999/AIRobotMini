using Microsoft.AspNetCore.Mvc;
using AIChatBot.Services;
using AIChatBot.Models;

namespace AIChatBot.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly IChatGptService _chatGptService;

        public ChatController(IConfiguration configuration, IChatGptService chatGptService)
        {
            _configuration = configuration;
            _chatGptService = chatGptService;
        }

        [HttpPost]
        public async Task<IActionResult> Chat([FromBody] ChatRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Message))
                return BadRequest(new { error = "Message is required" });

            try
            {
                AIProvider provider = request.Provider;
                string apiKey = "";

                switch (provider)
                {
                    case AIProvider.OpenAI:
                        apiKey = _configuration["OpenAIApiKey"] ?? "";
                        break;
                    case AIProvider.Gemini:
                        apiKey = _configuration["GeminiApiKey"] ?? "";
                        break;
                    case AIProvider.DeepSeek:
                        apiKey = _configuration["DeepSeekApiKey"] ?? "";
                        break;
                    case AIProvider.Qwen:
                        apiKey = _configuration["QwenApiKey"] ?? "";
                        break;
                    case AIProvider.OpenRouter:
                        apiKey = _configuration["OpenRouterApiKey"] ?? "";
                        break;
                }

                if (string.IsNullOrEmpty(apiKey))
                    return StatusCode(500, new { error = $"{provider} API Key not configured" });

                var langNames = new Dictionary<string, string>
                {
                    ["vi"] = "tiếng Việt",
                    ["en"] = "English",
                    ["ja"] = "日本語",
                    ["ko"] = "한국어",
                    ["zh-CN"] = "中文"
                };

                var language = langNames.GetValueOrDefault(request.Language, "tiếng Việt");
                var model = string.IsNullOrEmpty(request.Model) ? "qwen-max" : request.Model;

                var prompt = $"Bạn là một robot thông minh và thân thiện. Hãy trả lời ngắn gọn bằng {language}. Câu hỏi: {request.Message}";

                var settings = new ChatGptSettings
                {
                    Provider = provider,
                    ApiKey = apiKey,
                    Model = model,
                    Temperature = 0.7,
                    MaxTokens = 1000,
                    History = request.History?.Select(h => new ChatHistoryMessage 
                    { 
                        Role = h.Role, 
                        Content = h.Content 
                    }).ToList() ?? new List<ChatHistoryMessage>()
                };

                var result = await _chatGptService.ProcessContentAsync(prompt, settings);

                if (!result.Success)
                {
                    return StatusCode(500, new { error = result.ErrorMessage });
                }

                return Ok(new { response = result.Content });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Internal server error", message = ex.Message });
            }
        }
    }

    public class ChatRequest
    {
        public string Message { get; set; } = string.Empty;
        public string Language { get; set; } = "vi";
        public AIProvider Provider { get; set; } = AIProvider.Gemini;
        public string Model { get; set; } = "gemini-2.0-flash-exp";
        public List<ChatMessage> History { get; set; } = new List<ChatMessage>();
    }

    public class ChatMessage
    {
        public string Role { get; set; } = "user"; // "user" or "assistant"
        public string Content { get; set; } = string.Empty;
    }
}
