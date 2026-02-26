using System.Diagnostics;
using System.IO;
using System.Speech.Synthesis;
using AIChatBot.Models;
using NAudio.Wave;
using Newtonsoft.Json;

namespace AIChatBot.Services
{
    public class VoiceConfig
    {
        public string Name { get; set; } = "";
        public string VoiceId { get; set; } = "";
        public string Gender { get; set; } = "";
    }

    public class TextToSpeechService : ITextToSpeechService
    {
        private Dictionary<string, List<VoiceConfig>> _voiceMapping = new();

        public TextToSpeechService()
        {
            LoadVoiceConfig();
        }

        private void LoadVoiceConfig()
        {
            try
            {
                var configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config", "voices.json");
                if (File.Exists(configPath))
                {
                    var json = File.ReadAllText(configPath);
                    _voiceMapping = JsonConvert.DeserializeObject<Dictionary<string, List<VoiceConfig>>>(json) ?? new();
                }
            }
            catch
            {
                // Fallback to default
                _voiceMapping = new Dictionary<string, List<VoiceConfig>>
                {
                    ["Vietnamese"] = new()
                    {
                        new() { Name = "HoaiMy", VoiceId = "vi-VN-HoaiMyNeural", Gender = "Female" },
                        new() { Name = "NamMinh", VoiceId = "vi-VN-NamMinhNeural", Gender = "Male" }
                    },
                    ["English"] = new()
                    {
                        new() { Name = "Jenny", VoiceId = "en-US-JennyNeural", Gender = "Female" },
                        new() { Name = "Christopher", VoiceId = "en-US-ChristopherNeural", Gender = "Male" }
                    }
                };
            }
        }

        public List<VoiceInfoAI> GetAvailableVoices(string language)
        {
            if (_voiceMapping.TryGetValue(language, out var voices))
            {
                return voices.Select(v => new VoiceInfoAI
                {
                    Name = v.Name,
                    VoiceId = v.VoiceId,
                    Language = language
                }).ToList();
            }
            return new List<VoiceInfoAI>();
        }

        public List<string> GetAvailableLanguages()
        {
            return _voiceMapping.Keys.ToList();
        }

        private string GetVoiceId(TtsSettings settings)
        {
            if (_voiceMapping.TryGetValue(settings.Language, out var voices))
            {
                var voice = voices.FirstOrDefault(v => v.Name == settings.Voice);
                if (voice != null)
                    return voice.VoiceId;
                return voices.First().VoiceId;
            }
            return "vi-VN-HoaiMyNeural";
        }

        public async Task<TtsResult> ConvertToAudioAsync(string text, TtsSettings settings, string outputPath)
        {
            try
            {
                var voiceId = GetVoiceId(settings);
                var rate = (int)((settings.Speed - 1) * 100);
                var pitch = (int)((settings.Pitch - 1) * 50);
                var volume = (int)((settings.Volume - 1) * 100);

                // Use edge-tts via Python
                var result = await ConvertWithEdgeTtsPythonAsync(text, voiceId, rate, pitch, volume, outputPath);
                
                if (result.Success)
                    return result;

                // Fallback to Windows Speech Synthesis
                return await ConvertWithWindowsTtsAsync(text, settings, outputPath);
            }
            catch
            {
                return await ConvertWithWindowsTtsAsync(text, settings, outputPath);
            }
        }

        private async Task<TtsResult> ConvertWithEdgeTtsPythonAsync(string text, string voice, int rate, int pitch, int volume, string outputPath)
        {
            try
            {
                // Build rate/pitch/volume strings
                var rateStr = rate >= 0 ? $"+{rate}%" : $"{rate}%";
                var pitchStr = pitch >= 0 ? $"+{pitch}Hz" : $"{pitch}Hz";
                var volumeStr = volume >= 0 ? $"+{volume}%" : $"{volume}%";

                var baseDir = AppDomain.CurrentDomain.BaseDirectory;
                var exePath = Path.Combine(baseDir, "Tools", "edge_tts_convert.exe");
                
                // Write text to temp file (to handle special characters)
                var tempTextFile = Path.Combine(Path.GetTempPath(), $"tts_text_{Guid.NewGuid()}.txt");
                await File.WriteAllTextAsync(tempTextFile, text, System.Text.Encoding.UTF8);

                ProcessStartInfo processInfo;
                
                if (File.Exists(exePath))
                {
                    // Use standalone .exe
                    // Args: <text_file> <voice> <rate> <pitch> <volume> <output_path>
                    processInfo = new ProcessStartInfo
                    {
                        FileName = exePath,
                        Arguments = $"\"{tempTextFile}\" \"{voice}\" \"{rateStr}\" \"{pitchStr}\" \"{volumeStr}\" \"{outputPath}\"",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true
                    };
                }
                else
                {
                    // Fallback to Python script
                    var escapedText = text.Replace("\"", "\\\"").Replace("\n", " ").Replace("\r", "");
                    
                    var pythonCode = $@"
import asyncio
import edge_tts

async def main():
    communicate = edge_tts.Communicate(
        text=r'''{escapedText}''',
        voice='{voice}',
        rate='{rateStr}',
        pitch='{pitchStr}',
        volume='{volumeStr}'
    )
    await communicate.save(r'{outputPath}')
    print('SUCCESS')

asyncio.run(main())
";
                    
                    var tempPyFile = Path.Combine(Path.GetTempPath(), $"tts_{Guid.NewGuid()}.py");
                    await File.WriteAllTextAsync(tempPyFile, pythonCode, System.Text.Encoding.UTF8);

                    processInfo = new ProcessStartInfo
                    {
                        FileName = "python",
                        Arguments = $"\"{tempPyFile}\"",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true
                    };
                }

                using var process = Process.Start(processInfo);
                if (process != null)
                {
                    var output = await process.StandardOutput.ReadToEndAsync();
                    var error = await process.StandardError.ReadToEndAsync();
                    await process.WaitForExitAsync();
                    
                    // Clean up temp files
                    try { File.Delete(tempTextFile); } catch { }

                    if (process.ExitCode == 0 && File.Exists(outputPath))
                    {
                        return new TtsResult
                        {
                            Success = true,
                            OutputPath = outputPath
                        };
                    }
                    
                    return new TtsResult
                    {
                        Success = false,
                        ErrorMessage = $"Edge TTS error: {error}"
                    };
                }

                return new TtsResult
                {
                    Success = false,
                    ErrorMessage = "Không thể khởi động process"
                };
            }
            catch (Exception ex)
            {
                return new TtsResult
                {
                    Success = false,
                    ErrorMessage = $"Lỗi Edge TTS: {ex.Message}"
                };
            }
        }

        private async Task<TtsResult> ConvertWithWindowsTtsAsync(string text, TtsSettings settings, string outputPath)
        {
            try
            {
                await Task.Run(() =>
                {
                    using var synth = new SpeechSynthesizer();
                    synth.Rate = Math.Clamp((int)((settings.Speed - 1) * 10), -10, 10);
                    synth.Volume = Math.Clamp((int)(settings.Volume * 100), 0, 100);
                    synth.SetOutputToWaveFile(outputPath);
                    synth.Speak(text);
                });

                return new TtsResult
                {
                    Success = true,
                    OutputPath = outputPath
                };
            }
            catch (Exception ex)
            {
                return new TtsResult
                {
                    Success = false,
                    ErrorMessage = $"Lỗi TTS: {ex.Message}"
                };
            }
        }

        public async Task<TtsResult> ConvertToAudioBySegmentsAsync(string text, TtsSettings settings, string outputFolder, Action<int, int>? onProgress = null)
        {
            try
            {
                Directory.CreateDirectory(outputFolder);
                
                // Normalize: nhiều xuống dòng liên tiếp → 1 xuống dòng đôi
                var normalizedText = System.Text.RegularExpressions.Regex.Replace(
                    text.Replace("\r\n", "\n").Replace("\r", "\n"),
                    @"\n{2,}", // 2 hoặc nhiều hơn xuống dòng
                    "\n\n"     // thành 1 xuống dòng đôi
                );
                
                // Split by double newline (paragraph)
                var segments = normalizedText
                    .Split(new[] { "\n\n" }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => s.Trim().Replace("\n", " ")) // Gộp các dòng trong 1 đoạn thành 1 câu
                    .Where(s => !string.IsNullOrWhiteSpace(s))
                    .ToList();

                if (segments.Count == 0)
                {
                    return new TtsResult
                    {
                        Success = false,
                        ErrorMessage = "Không tìm thấy đoạn nào để xuất (phân tách bằng xuống dòng đôi)"
                    };
                }

                for (int i = 0; i < segments.Count; i++)
                {
                    // Report progress
                    onProgress?.Invoke(i + 1, segments.Count);
                    
                    var outputPath = Path.Combine(outputFolder, $"segment_{i + 1:D3}.mp3");
                    var result = await ConvertToAudioAsync(segments[i], settings, outputPath);
                    
                    if (!result.Success)
                        return result;
                }

                return new TtsResult
                {
                    Success = true,
                    OutputPath = outputFolder
                };
            }
            catch (Exception ex)
            {
                return new TtsResult
                {
                    Success = false,
                    ErrorMessage = $"Lỗi khi xuất audio theo đoạn: {ex.Message}"
                };
            }
        }

        public async Task PlayPreviewAsync(TtsSettings settings)
        {
            var previewText = settings.Language switch
            {
                "Vietnamese" => "Xin chào, đây là giọng đọc mẫu bằng tiếng Việt.",
                "Japanese" => "こんにちは、これはサンプルの音声です。",
                "Korean" => "안녕하세요, 이것은 샘플 음성입니다.",
                "Chinese" => "你好，这是示例语音。",
                _ => "Hello, this is a sample voice preview."
            };

            var tempFile = Path.Combine(Path.GetTempPath(), $"preview_{Guid.NewGuid()}.mp3");
            
            try
            {
                var result = await ConvertToAudioAsync(previewText, settings, tempFile);

                if (result.Success && File.Exists(tempFile))
                {
                    await Task.Run(() =>
                    {
                        using var audioFile = new AudioFileReader(tempFile);
                        using var outputDevice = new WaveOutEvent();
                        outputDevice.Init(audioFile);
                        outputDevice.Play();
                        while (outputDevice.PlaybackState == PlaybackState.Playing)
                        {
                            Thread.Sleep(100);
                        }
                    });
                }
                else
                {
                    throw new Exception(result.ErrorMessage ?? "Không thể tạo audio");
                }
            }
            finally
            {
                try { if (File.Exists(tempFile)) File.Delete(tempFile); } catch { }
            }
        }
    }
}
