using AIChatBot.Models;

namespace AIChatBot.Services
{
    public interface IChatGptService
    {
        Task<ChatGptResult> ProcessContentAsync(string prompt, ChatGptSettings settings);
    }
}
