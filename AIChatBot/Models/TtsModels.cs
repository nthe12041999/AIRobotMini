namespace AIChatBot.Models
{
    public class TtsSettings
    {
        public string Language { get; set; } = "English";
        public string Voice { get; set; } = "Christopher";
        public double Pitch { get; set; } = 1.0;
        public double Speed { get; set; } = 1.0;
        public double Volume { get; set; } = 1.0;
    }

    public class TtsResult
    {
        public bool Success { get; set; }
        public string OutputPath { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;
    }

    public class VoiceInfoAI
    {
        public string Name { get; set; } = string.Empty;
        public string VoiceId { get; set; } = string.Empty;
        public string Language { get; set; } = string.Empty;
    }
}
