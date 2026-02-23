namespace AIChatBot.Models
{
    public enum AIProvider
    {
        OpenAI,
        Gemini,
        DeepSeek
    }

    public class ChatGptSettings
    {
        public AIProvider Provider { get; set; } = AIProvider.OpenAI;
        public string ApiKey { get; set; } = string.Empty;
        public string Model { get; set; } = "gpt-4o-2024-08-06";
        public double Temperature { get; set; } = 1.0;
        public int MaxTokens { get; set; } = 5000;
    }

    public class ChatGptResult
    {
        public bool Success { get; set; }
        public string Content { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;
    }

    // Response models for Gemini
    public class GeminiResponse
    {
        public List<GeminiCandidate>? Candidates { get; set; }
    }

    public class GeminiCandidate
    {
        public GeminiContent? Content { get; set; }
    }

    public class GeminiContent
    {
        public List<GeminiPart>? Parts { get; set; }
    }

    public class GeminiPart
    {
        public string? Text { get; set; }
    }

    // Response models for DeepSeek
    public class DeepSeekResponse
    {
        public List<DeepSeekChoice>? Choices { get; set; }
    }

    public class DeepSeekChoice
    {
        public DeepSeekMessage? Message { get; set; }
    }

    public class DeepSeekMessage
    {
        public string? Content { get; set; }
    }

    // Response models for OpenAI /v1/responses (GPT-5.x)
    public class OpenAIResponsesResult
    {
        public List<OpenAIResponseOutput>? Output { get; set; }
    }

    public class OpenAIResponseOutput
    {
        public string? Type { get; set; }
        public List<OpenAIResponseContent>? Content { get; set; }
    }

    public class OpenAIResponseContent
    {
        public string? Type { get; set; }
        public string? Text { get; set; }
    }
}
