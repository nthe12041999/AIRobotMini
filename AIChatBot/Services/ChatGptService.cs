using System.ClientModel;
using System.Net.Http;
using System.Text;
using AIChatBot.Models;
using Newtonsoft.Json;
using OpenAI;
using OpenAI.Chat;

namespace AIChatBot.Services
{
    public class ChatGptService : IChatGptService
    {
        private static readonly HttpClient _httpClient = new HttpClient();

        public async Task<ChatGptResult> ProcessContentAsync(string prompt, ChatGptSettings settings)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(settings.ApiKey))
                {
                    return new ChatGptResult
                    {
                        Success = false,
                        ErrorMessage = "API Key không được để trống."
                    };
                }

                return settings.Provider switch
                {
                    AIProvider.Gemini => await ProcessWithGeminiAsync(prompt, settings),
                    AIProvider.DeepSeek => await ProcessWithDeepSeekAsync(prompt, settings),
                    AIProvider.OpenAI => await ProcessWithOpenAIAsync(prompt, settings)
                };
            }
            catch (Exception ex)
            {
                return new ChatGptResult
                {
                    Success = false,
                    ErrorMessage = $"Lỗi khi gọi AI API: {ex.Message}"
                };
            }
        }

        private async Task<ChatGptResult> ProcessWithOpenAIAsync(string prompt, ChatGptSettings settings)
        {
            // GPT-5.x dùng /v1/responses endpoint, các model cũ dùng /v1/chat/completions
            var isGpt5Model = settings.Model.StartsWith("gpt-5") || 
                              settings.Model.StartsWith("o");

            if (isGpt5Model)
            {
                return await ProcessWithOpenAIResponsesAsync(prompt, settings);
            }
            else
            {
                return await ProcessWithOpenAIChatAsync(prompt, settings);
            }
        }

        private async Task<ChatGptResult> ProcessWithOpenAIChatAsync(string prompt, ChatGptSettings settings)
        {
            var client = new OpenAIClient(new ApiKeyCredential(settings.ApiKey));
            var chatClient = client.GetChatClient(settings.Model);

            var messages = new List<ChatMessage>
            {
                new UserChatMessage(prompt)
            };

            var options = new ChatCompletionOptions
            {
                Temperature = (float)settings.Temperature,
                MaxOutputTokenCount = settings.MaxTokens
            };

            var response = await chatClient.CompleteChatAsync(messages, options);
            var result = response.Value;
            var responseContent = result.Content[0].Text;

            return new ChatGptResult
            {
                Success = true,
                Content = responseContent
            };
        }

        private async Task<ChatGptResult> ProcessWithOpenAIResponsesAsync(string prompt, ChatGptSettings settings)
        {
            var endpoint = "https://api.openai.com/v1/responses";

            var requestBody = new
            {
                model = settings.Model,
                input = prompt,
                temperature = settings.Temperature,
                max_output_tokens = settings.MaxTokens
            };

            var jsonSettings = new JsonSerializerSettings { Culture = System.Globalization.CultureInfo.InvariantCulture };
            var json = JsonConvert.SerializeObject(requestBody, jsonSettings);
            using var httpContent = new StringContent(json, Encoding.UTF8, "application/json");
            using var request = new HttpRequestMessage(HttpMethod.Post, endpoint);
            
            request.Headers.TryAddWithoutValidation("Authorization", $"Bearer {settings.ApiKey}");
            request.Content = httpContent;

            var response = await _httpClient.SendAsync(request);
            var responseString = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                return new ChatGptResult
                {
                    Success = false,
                    ErrorMessage = $"OpenAI API error: {response.StatusCode} - {responseString}"
                };
            }

            var result = JsonConvert.DeserializeObject<OpenAIResponsesResult>(responseString);
            var output = result?.Output?.FirstOrDefault()?.Content?.FirstOrDefault()?.Text;

            if (string.IsNullOrEmpty(output))
            {
                return new ChatGptResult
                {
                    Success = false,
                    ErrorMessage = "OpenAI không trả về kết quả."
                };
            }

            return new ChatGptResult
            {
                Success = true,
                Content = output
            };
        }

        private async Task<ChatGptResult> ProcessWithGeminiAsync(string prompt, ChatGptSettings settings)
        {
            var endpoint = $"https://generativelanguage.googleapis.com/v1beta/models/{settings.Model}:generateContent";

            // Gemini temperature must be in range [0.0, 2.0]
            var temperature = Math.Clamp(settings.Temperature, 0.0, 2.0);

            var requestBody = new
            {
                contents = new[]
                {
                    new {
                        parts = new[]
                        {
                            new { text = prompt }
                        }
                    }
                },
                generationConfig = new
                {
                    temperature,
                    maxOutputTokens = settings.MaxTokens
                }
            };

            // Use InvariantCulture to ensure correct decimal format (1.0 not 1,0)
            var jsonSettings = new JsonSerializerSettings { Culture = System.Globalization.CultureInfo.InvariantCulture };
            var json = JsonConvert.SerializeObject(requestBody, jsonSettings);
            using var httpContent = new StringContent(json, Encoding.UTF8, "application/json");
            using var request = new HttpRequestMessage(HttpMethod.Post, endpoint);
            
            request.Headers.TryAddWithoutValidation("x-goog-api-key", settings.ApiKey);
            request.Content = httpContent;

            var response = await _httpClient.SendAsync(request);
            var responseString = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                return new ChatGptResult
                {
                    Success = false,
                    ErrorMessage = $"Gemini API error: {response.StatusCode} - {responseString}"
                };
            }

            var result = JsonConvert.DeserializeObject<GeminiResponse>(responseString);
            var output = result?.Candidates?.FirstOrDefault()?.Content?.Parts?.FirstOrDefault()?.Text;

            if (string.IsNullOrEmpty(output))
            {
                return new ChatGptResult
                {
                    Success = false,
                    ErrorMessage = "Gemini không trả về kết quả."
                };
            }

            return new ChatGptResult
            {
                Success = true,
                Content = output
            };
        }

        private async Task<ChatGptResult> ProcessWithDeepSeekAsync(string prompt, ChatGptSettings settings)
        {
            var endpoint = "https://api.deepseek.com/chat/completions";

            var requestBody = new
            {
                model = settings.Model,
                messages = new[]
                {
                    new { role = "system", content = prompt },
                    new { role = "user", content = prompt }
                },
                temperature = settings.Temperature,
                max_tokens = settings.MaxTokens,
                stream = false
            };

            // Use InvariantCulture to ensure correct decimal format
            var jsonSettings = new JsonSerializerSettings { Culture = System.Globalization.CultureInfo.InvariantCulture };
            var json = JsonConvert.SerializeObject(requestBody, jsonSettings);
            using var httpContent = new StringContent(json, Encoding.UTF8, "application/json");
            using var request = new HttpRequestMessage(HttpMethod.Post, endpoint);
            
            request.Headers.TryAddWithoutValidation("Authorization", $"Bearer {settings.ApiKey}");
            request.Content = httpContent;

            var response = await _httpClient.SendAsync(request);
            var responseString = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                return new ChatGptResult
                {
                    Success = false,
                    ErrorMessage = $"DeepSeek API error: {response.StatusCode} - {responseString}"
                };
            }

            var result = JsonConvert.DeserializeObject<DeepSeekResponse>(responseString);
            var output = result?.Choices?.FirstOrDefault()?.Message?.Content;

            if (string.IsNullOrEmpty(output))
            {
                return new ChatGptResult
                {
                    Success = false,
                    ErrorMessage = "DeepSeek không trả về kết quả."
                };
            }

            return new ChatGptResult
            {
                Success = true,
                Content = output
            };
        }
    }
}
