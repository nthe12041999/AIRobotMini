// Import Edge TTS library - KHÔNG dùng nữa, dùng server API
// import * as EdgeTTSLib from 'edge-tts-universal';

// Load config
const CONFIG = window.CONFIG || { GEMINI_API_KEY: '' };

// Lấy các elements
const leftPupil = document.querySelector('.left-eye .pupil');
const rightPupil = document.querySelector('.right-eye .pupil');
const mouth = document.querySelector('.mouth');
const textInput = document.getElementById('textInput');
const micBtn = document.getElementById('micBtn');
const sendBtn = document.getElementById('sendBtn');
const speakBtn = document.getElementById('speakBtn');
const clearBtn = document.getElementById('clearBtn');
const apiKeyInput = document.getElementById('apiKey');
const saveKeyBtn = document.getElementById('saveKeyBtn');
const conversationLang = document.getElementById('conversationLang');
const conversation = document.getElementById('conversation');
const voiceSelect = document.getElementById('voiceSelect');
const rateSlider = document.getElementById('rateSlider');
const pitchSlider = document.getElementById('pitchSlider');
const rateValue = document.getElementById('rateValue');
const pitchValue = document.getElementById('pitchValue');
const modelSelect = document.getElementById('modelSelect');
const ttsEngine = document.getElementById('ttsEngine');
const edgeVoiceSelect = document.getElementById('edgeVoiceSelect');
const edgeVoiceSection = document.getElementById('edgeVoiceSection');
const browserVoiceSection = document.getElementById('browserVoiceSection');

// Biến toàn cục
let typingTimeout;
let apiKey = CONFIG.GEMINI_API_KEY || localStorage.getItem('geminiApiKey') || '';
let conversationHistory = [];
const synth = window.speechSynthesis;
let currentUtterance = null;
let voices = [];
let selectedVoice = null;
let lastRobotResponse = '';

// Khởi tạo Speech Recognition
const SpeechRecognition = window.SpeechRecognition || window.webkitSpeechRecognition;
const recognition = SpeechRecognition ? new SpeechRecognition() : null;

if(recognition) {
    recognition.continuous = false;
    recognition.interimResults = false;
}

// Load API key
if(apiKey) {
    apiKeyInput.value = '••••••••••••••••••••';
    apiKeyInput.disabled = true;
    saveKeyBtn.textContent = 'Đã có Key';
    saveKeyBtn.disabled = true;
}

saveKeyBtn.addEventListener('click', () => {
    apiKey = apiKeyInput.value.trim();
    if(apiKey) {
        localStorage.setItem('geminiApiKey', apiKey);
        alert('API Key đã được lưu!');
    }
});

// Di chuyển mắt
function moveEyes() {
    const maxMove = 15;
    const randomX = (Math.random() - 0.5) * maxMove;
    const randomY = (Math.random() - 0.5) * maxMove;
    leftPupil.style.transform = `translate(calc(-50% + ${randomX}px), calc(-50% + ${randomY}px))`;
    rightPupil.style.transform = `translate(calc(-50% + ${randomX}px), calc(-50% + ${randomY}px))`;
    setTimeout(moveEyes, 2000 + Math.random() * 2000);
}
moveEyes();

// Chớp mắt
function blinkEyes() {
    const eyes = document.querySelectorAll('.eye');
    eyes.forEach(eye => eye.style.transform = 'scaleY(0.1)');
    setTimeout(() => {
        eyes.forEach(eye => eye.style.transform = 'scaleY(1)');
    }, 150);
    setTimeout(blinkEyes, 3000 + Math.random() * 3000);
}
document.querySelectorAll('.eye').forEach(eye => {
    eye.style.transition = 'transform 0.1s ease';
});
blinkEyes();

// Miệng mấp máy khi typing
textInput.addEventListener('input', () => {
    mouth.classList.add('talking');
    clearTimeout(typingTimeout);
    typingTimeout = setTimeout(() => mouth.classList.remove('talking'), 200);
});

// Thêm tin nhắn
function addMessage(text, isUser) {
    const messageDiv = document.createElement('div');
    messageDiv.className = `message ${isUser ? 'user' : 'robot'}`;
    messageDiv.innerHTML = `<strong>${isUser ? 'Bạn' : 'Robot'}:</strong>${text}`;
    conversation.appendChild(messageDiv);
    conversation.scrollTop = conversation.scrollHeight;
}

// Gọi Gemini API
async function callGeminiAPI(userMessage) {
    if(!apiKey) return 'Vui lòng nhập Gemini API Key trước!';

    const lang = conversationLang.value;
    const langNames = {
        'vi': 'tiếng Việt', 'en': 'English', 'ja': '日本語',
        'ko': '한국어', 'zh-CN': '中文'
    };
    const model = modelSelect.value;

    try {
        const response = await fetch(`https://generativelanguage.googleapis.com/v1beta/models/${model}:generateContent`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'x-goog-api-key': apiKey
            },
            body: JSON.stringify({
                contents: [{
                    parts: [{
                        text: `Bạn là một robot thông minh và thân thiện. Hãy trả lời ngắn gọn bằng ${langNames[lang]}. Câu hỏi: ${userMessage}`
                    }]
                }]
            })
        });

        if(!response.ok) {
            const errorData = await response.json();
            return `Lỗi API: ${errorData.error?.message || 'Không xác định'}`;
        }

        const data = await response.json();
        return data.candidates?.[0]?.content?.parts?.[0]?.text || 'Xin lỗi, tôi không thể trả lời.';
    } catch(error) {
        return 'Đã xảy ra lỗi khi kết nối với Gemini API.';
    }
}

// Edge TTS
// Edge TTS - Call C# backend API
async function speakWithEdgeTTS(text, voiceName) {
    if(!text.trim()) return;
    mouth.classList.add('talking');

    try {
        console.log(`Calling C# backend TTS: "${text}" with voice: ${voiceName}`);

        const response = await fetch('http://localhost:44246/api/tts', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({
                text: text,
                voice: voiceName
            })
        });

        if(!response.ok) {
            throw new Error(`Backend error: ${response.status}`);
        }

        const audioBlob = await response.blob();
        const audioUrl = URL.createObjectURL(audioBlob);
        const audio = new Audio(audioUrl);

        audio.onended = () => {
            mouth.classList.remove('talking');
            URL.revokeObjectURL(audioUrl);
        };

        audio.onerror = (error) => {
            mouth.classList.remove('talking');
            console.error('Audio playback error:', error);
            speakWithBrowserTTS(text);
        };

        await audio.play();
        console.log('✓ Playing Edge TTS audio from C# backend');

    } catch(error) {
        console.error('Edge TTS Error:', error);
        mouth.classList.remove('talking');
        alert('⚠️ Không thể kết nối C# backend!\n\nĐảm bảo AIChatBot đang chạy:\n  cd AIChatBot\n  dotnet run\n\nĐang dùng Browser TTS...');
        speakWithBrowserTTS(text);
    }
}



// Browser TTS
function speakWithBrowserTTS(text, lang = 'vi') {
    if(!text.trim()) return;
    if(synth.speaking) synth.cancel();

    currentUtterance = new SpeechSynthesisUtterance(text);
    if(selectedVoice) {
        currentUtterance.voice = selectedVoice;
    } else {
        const voice = voices.find(v => v.lang.startsWith(lang));
        if(voice) currentUtterance.voice = voice;
    }

    currentUtterance.rate = parseFloat(rateSlider.value);
    currentUtterance.pitch = parseFloat(pitchSlider.value);
    currentUtterance.lang = lang;
    currentUtterance.onstart = () => mouth.classList.add('talking');
    currentUtterance.onend = () => mouth.classList.remove('talking');
    currentUtterance.onerror = () => mouth.classList.remove('talking');
    synth.speak(currentUtterance);
}

// Wrapper speak
async function speak(text, lang = 'vi') {
    if(!text.trim()) return;
    const engine = ttsEngine.value;
    if(engine === 'edge') {
        await speakWithEdgeTTS(text, edgeVoiceSelect.value);
    } else {
        speakWithBrowserTTS(text, lang);
    }
}

// Gửi tin nhắn
async function sendMessage() {
    const userMessage = textInput.value.trim();
    if(!userMessage) return;

    addMessage(userMessage, true);
    textInput.value = '';
    sendBtn.disabled = true;
    sendBtn.textContent = 'Đang xử lý...';

    const robotResponse = await callGeminiAPI(userMessage);
    addMessage(robotResponse, false);
    lastRobotResponse = robotResponse;

    const lang = conversationLang.value;
    speak(robotResponse, lang);

    sendBtn.disabled = false;
    sendBtn.textContent = 'Gửi';
}

sendBtn.addEventListener('click', sendMessage);
textInput.addEventListener('keydown', (e) => {
    if(e.key === 'Enter' && !e.shiftKey) {
        e.preventDefault();
        sendMessage();
    }
});

clearBtn.addEventListener('click', () => {
    conversation.innerHTML = '';
    conversationHistory = [];
});

speakBtn.addEventListener('click', () => {
    const textToSpeak = textInput.value.trim();
    if(textToSpeak) {
        speak(textToSpeak, conversationLang.value);
    } else {
        alert('Vui lòng nhập nội dung cần nói!');
    }
});

// Mic
if(recognition) {
    micBtn.addEventListener('click', () => {
        recognition.lang = conversationLang.value;
        if(micBtn.classList.contains('listening')) {
            recognition.stop();
        } else {
            recognition.start();
            micBtn.classList.add('listening');
            micBtn.querySelector('.mic-text').textContent = 'Đang nghe...';
        }
    });

    recognition.onresult = (event) => {
        textInput.value = event.results[0][0].transcript;
        micBtn.classList.remove('listening');
        micBtn.querySelector('.mic-text').textContent = 'Nhấn để nói';
        setTimeout(sendMessage, 500);
    };

    recognition.onerror = () => {
        micBtn.classList.remove('listening');
        micBtn.querySelector('.mic-text').textContent = 'Nhấn để nói';
    };

    recognition.onend = () => {
        micBtn.classList.remove('listening');
        micBtn.querySelector('.mic-text').textContent = 'Nhấn để nói';
    };
} else {
    micBtn.disabled = true;
    micBtn.querySelector('.mic-text').textContent = 'Không hỗ trợ';
}

// Load voices
function loadVoices() {
    voices = synth.getVoices();
    voiceSelect.innerHTML = '';
    const lang = conversationLang.value;
    const matchingVoices = voices.filter(v => v.lang.includes(lang));
    matchingVoices.forEach(voice => {
        const option = document.createElement('option');
        option.value = voice.name;
        option.textContent = `${voice.name} (${voice.lang})`;
        voiceSelect.appendChild(option);
    });
}

window.speechSynthesis.onvoiceschanged = loadVoices;
loadVoices();

voiceSelect.addEventListener('change', () => {
    selectedVoice = voices.find(v => v.name === voiceSelect.value);
});

conversationLang.addEventListener('change', loadVoices);

rateSlider.addEventListener('input', () => {
    rateValue.textContent = rateSlider.value;
});

pitchSlider.addEventListener('input', () => {
    pitchValue.textContent = pitchSlider.value;
});

ttsEngine.addEventListener('change', () => {
    if(ttsEngine.value === 'edge') {
        edgeVoiceSection.style.display = 'block';
        browserVoiceSection.style.display = 'none';
    } else {
        edgeVoiceSection.style.display = 'none';
        browserVoiceSection.style.display = 'block';
    }
});
