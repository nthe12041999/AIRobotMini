using AIChatBot.Models;
using AIChatBot.Services;
using Microsoft.AspNetCore.Mvc;

namespace AIChatBot.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TtsController : ControllerBase
    {
        private readonly ITextToSpeechService _ttsService;

        public TtsController(ITextToSpeechService ttsService)
        {
            _ttsService = ttsService;
        }

        [HttpPost]
        public async Task<IActionResult> GenerateSpeech([FromBody] TtsRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Text))
                return BadRequest(new { error = "Text is required" });

            try
            {
                // Map voice name to language
                var language = GetLanguageFromVoice(request.Voice);
                var voiceName = GetVoiceNameFromId(request.Voice);

                var settings = new TtsSettings
                {
                    Language = language,
                    Voice = voiceName,
                    Speed = 1.0,
                    Pitch = 1.0,
                    Volume = 1.0
                };

                var tempFile = Path.Combine(Path.GetTempPath(), $"tts_{Guid.NewGuid()}.mp3");
                var result = await _ttsService.ConvertToAudioAsync(request.Text, settings, tempFile);

                if (!result.Success)
                    return StatusCode(500, new { error = result.ErrorMessage });

                var audioBytes = await System.IO.File.ReadAllBytesAsync(tempFile);
                
                // Clean up temp file
                try { System.IO.File.Delete(tempFile); } catch { }

                return File(audioBytes, "audio/mpeg");
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        private string GetLanguageFromVoice(string voiceId)
        {
            if (voiceId.StartsWith("vi-VN")) return "Vietnamese";
            if (voiceId.StartsWith("en-US")) return "English";
            if (voiceId.StartsWith("ja-JP")) return "Japanese";
            if (voiceId.StartsWith("ko-KR")) return "Korean";
            if (voiceId.StartsWith("zh-CN")) return "Chinese";
            return "Vietnamese";
        }

        private string GetVoiceNameFromId(string voiceId)
        {
            var mapping = new Dictionary<string, string>
            {
                ["vi-VN-HoaiMyNeural"] = "HoaiMy",
                ["vi-VN-NamMinhNeural"] = "NamMinh",
                ["en-US-JennyNeural"] = "Jenny",
                ["en-US-GuyNeural"] = "Guy",
                ["ja-JP-NanamiNeural"] = "Nanami",
                ["ko-KR-SunHiNeural"] = "SunHi",
                ["zh-CN-XiaoxiaoNeural"] = "Xiaoxiao"
            };

            return mapping.TryGetValue(voiceId, out var name) ? name : "HoaiMy";
        }
    }

    public class TtsRequest
    {
        public string Text { get; set; } = string.Empty;
        public string Voice { get; set; } = "vi-VN-HoaiMyNeural";
    }
}
