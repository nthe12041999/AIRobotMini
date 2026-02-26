using AIChatBot.Models;

namespace AIChatBot.Services
{
    public interface ITextToSpeechService
    {
        Task<TtsResult> ConvertToAudioAsync(string text, TtsSettings settings, string outputPath);
        Task<TtsResult> ConvertToAudioBySegmentsAsync(string text, TtsSettings settings, string outputFolder, Action<int, int>? onProgress = null);
        Task PlayPreviewAsync(TtsSettings settings);
        List<VoiceInfoAI> GetAvailableVoices(string language);
    }
}
