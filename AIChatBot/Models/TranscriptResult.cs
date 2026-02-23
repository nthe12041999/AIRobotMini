namespace AIChatBot.Models
{
    public class TranscriptResult
    {
        public bool Success { get; set; }
        public string Transcript { get; set; } = string.Empty;
        public string VideoTitle { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;
    }
}
