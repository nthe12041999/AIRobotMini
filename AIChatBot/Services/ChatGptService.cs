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
                    AIProvider.Qwen => await ProcessWithQwenAsync(prompt, settings),
                    AIProvider.OpenRouter => await ProcessWithOpenRouterAsync(prompt, settings),
                    AIProvider.OpenAI => await ProcessWithOpenAIAsync(prompt, settings),
                    _ => throw new NotSupportedException($"Provider {settings.Provider} is not supported")
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

            var messages = new List<ChatMessage>();
            
            // Add conversation history
            foreach (var historyMsg in settings.History)
            {
                if (historyMsg.Role.ToLower() == "user")
                    messages.Add(new UserChatMessage(historyMsg.Content));
                else if (historyMsg.Role.ToLower() == "assistant")
                    messages.Add(new AssistantChatMessage(historyMsg.Content));
            }
            
            // Add current message
            messages.Add(new UserChatMessage(prompt));

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

            // Build conversation history for Gemini
            var contentsList = new List<object>();
            
            foreach (var historyMsg in settings.History)
            {
                contentsList.Add(new
                {
                    role = historyMsg.Role.ToLower() == "assistant" ? "model" : "user",
                    parts = new[] { new { text = historyMsg.Content } }
                });
            }
            
            // Add current message
            contentsList.Add(new
            {
                role = "user",
                parts = new[] { new { text = prompt } }
            });

            var requestBody = new
            {
                contents = contentsList,
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

            // Build message list with history
            var messagesList = new List<object>
            {
                new { role = "system", content = "You are a helpful assistant." }
            };
            
            foreach (var historyMsg in settings.History)
            {
                messagesList.Add(new { role = historyMsg.Role, content = historyMsg.Content });
            }
            
            messagesList.Add(new { role = "user", content = prompt });

            var requestBody = new
            {
                model = settings.Model,
                messages = messagesList,
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

        private async Task<ChatGptResult> ProcessWithQwenAsync(string prompt, ChatGptSettings settings)
        {
            // Qwen API endpoint (Alibaba Cloud DashScope)
            var endpoint = "https://dashscope.aliyuncs.com/api/v1/services/aigc/text-generation/generation";

            // Build message list with history
            var messagesList = new List<object>
            {
                new { role = "system", content = "You are a helpful assistant." }
            };
            
            foreach (var historyMsg in settings.History)
            {
                messagesList.Add(new { role = historyMsg.Role, content = historyMsg.Content });
            }
            
            messagesList.Add(new { role = "user", content = prompt });

            var requestBody = new
            {
                model = settings.Model,
                input = new
                {
                    messages = messagesList
                },
                parameters = new
                {
                    temperature = settings.Temperature,
                    max_tokens = settings.MaxTokens,
                    result_format = "message"
                }
            };

            // Use InvariantCulture to ensure correct decimal format
            var jsonSettings = new JsonSerializerSettings { Culture = System.Globalization.CultureInfo.InvariantCulture };
            var json = JsonConvert.SerializeObject(requestBody, jsonSettings);
            using var httpContent = new StringContent(json, Encoding.UTF8, "application/json");
            using var request = new HttpRequestMessage(HttpMethod.Post, endpoint);
            
            request.Headers.TryAddWithoutValidation("Authorization", $"Bearer {settings.ApiKey}");
            request.Headers.TryAddWithoutValidation("Content-Type", "application/json");
            request.Content = httpContent;

            var response = await _httpClient.SendAsync(request);
            var responseString = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                return new ChatGptResult
                {
                    Success = false,
                    ErrorMessage = $"Qwen API error: {response.StatusCode} - {responseString}"
                };
            }

            var result = JsonConvert.DeserializeObject<QwenResponse>(responseString);
            var output = result?.Output?.Choices?.FirstOrDefault()?.Message?.Content;

            if (string.IsNullOrEmpty(output))
            {
                return new ChatGptResult
                {
                    Success = false,
                    ErrorMessage = "Qwen không trả về kết quả."
                };
            }

            return new ChatGptResult
            {
                Success = true,
                Content = output
            };
        }

        private async Task<ChatGptResult> ProcessWithOpenRouterAsync(string prompt, ChatGptSettings settings)
        {
            // OpenRouter API endpoint (compatible with OpenAI format)
            var endpoint = "https://openrouter.ai/api/v1/chat/completions";

            // Build message list with history
            var messagesList = new List<object>();
            
            foreach (var historyMsg in settings.History)
            {
                messagesList.Add(new { role = historyMsg.Role, content = historyMsg.Content });
            }
            
            messagesList.Add(new { role = "user", content = prompt });

            var requestBody = new
            {
                model = settings.Model,
                messages = messagesList,
                temperature = settings.Temperature,
                max_tokens = settings.MaxTokens
            };

            // Use InvariantCulture to ensure correct decimal format
            var jsonSettings = new JsonSerializerSettings { Culture = System.Globalization.CultureInfo.InvariantCulture };
            var json = JsonConvert.SerializeObject(requestBody, jsonSettings);
            using var httpContent = new StringContent(json, Encoding.UTF8, "application/json");
            using var request = new HttpRequestMessage(HttpMethod.Post, endpoint);
            
            request.Headers.TryAddWithoutValidation("Authorization", $"Bearer {settings.ApiKey}");
            request.Headers.TryAddWithoutValidation("HTTP-Referer", "https://github.com/nthe12041999/AIRobotMini");
            request.Headers.TryAddWithoutValidation("X-Title", "AI Robot Chatbot");
            request.Content = httpContent;

            var response = await _httpClient.SendAsync(request);
            var responseString = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                return new ChatGptResult
                {
                    Success = false,
                    ErrorMessage = $"OpenRouter API error: {response.StatusCode} - {responseString}"
                };
            }

            var result = JsonConvert.DeserializeObject<OpenRouterResponse>(responseString);
            var output = result?.Choices?.FirstOrDefault()?.Message?.Content;

            if (string.IsNullOrEmpty(output))
            {
                return new ChatGptResult
                {
                    Success = false,
                    ErrorMessage = "OpenRouter không trả về kết quả."
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
