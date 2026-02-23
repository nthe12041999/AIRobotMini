namespace AIChatBot.Models
{
    public class AppSettings
    {
        public AIProvider Provider { get; set; } = AIProvider.OpenAI;
        public string ApiKey { get; set; } = "";
        public string Model { get; set; } = "gpt-4o";
        public double Temperature { get; set; } = 1.0;
        public int MaxTokens { get; set; } = 5000;
        public int WordsPerChunk { get; set; } = 0; // 0 = không cắt, xử lý toàn bộ
        public string Prompt { get; set; } = "Tóm tắt ngắn gọn các ý chính";
        public string Language { get; set; } = "Vietnamese";
        public string Voice { get; set; } = "HoaiMy (Nữ)";
        public double Pitch { get; set; } = 1.0;
        public double Speed { get; set; } = 1.0;
        public double Volume { get; set; } = 1.0;
        public bool ExportAudioBySegment { get; set; } = false;
    }
}
