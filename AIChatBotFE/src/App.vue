<template>
  <n-config-provider :theme="darkTheme">
    <n-message-provider>
      <div class="container">
        <!-- Custom AI Robot (Pure CSS) -->
        <RobotCustom 
          :isTalking="isTalking" 
          :isListening="isListening"
          :isBlinking="isBlinking"
          emotion="neutral"
        />
        
        <n-card class="control-panel" :bordered="false">
          <n-space vertical :size="16">
            <!-- Settings Toggle Button -->
            <n-button 
              @click="showSettings = !showSettings" 
              type="info" 
              size="small"
              style="width: 100%"
            >
              {{ showSettings ? '🔼 Ẩn cài đặt' : '🔽 Hiện cài đặt' }}
            </n-button>

            <!-- Settings Section (Hidden by default) -->
            <div v-if="showSettings">
              <!-- AI Provider & Model -->
              <n-space vertical>
                <n-select
                  v-model:value="selectedAI"
                  :options="aiProviderOptions"
                  placeholder="Chọn AI"
                />
                <n-select
                  v-model:value="selectedModel"
                  :options="modelOptions"
                  placeholder="Chọn Model"
                />
              </n-space>

              <!-- Language -->
              <n-select
                v-model:value="conversationLang"
                :options="langOptions"
                placeholder="Ngôn ngữ"
                style="margin-top: 16px"
              />

              <!-- TTS Settings -->
              <n-space vertical style="margin-top: 16px">
                <n-select
                  v-model:value="ttsEngine"
                  :options="ttsEngineOptions"
                  placeholder="TTS Engine"
                />
                
                <n-select
                  v-if="ttsEngine === 'edge'"
                  v-model:value="edgeVoice"
                  :options="edgeVoiceOptions"
                  placeholder="Chọn giọng"
                />
                
                <n-select
                  v-else
                  v-model:value="selectedVoice"
                  :options="voiceOptions"
                  placeholder="Chọn giọng"
                />

                <n-space vertical>
                  <div>
                    <span style="color: white; font-size: 12px;">Tốc độ: {{ rate }}</span>
                    <n-slider v-model:value="rate" :min="0.5" :max="2" :step="0.1" />
                  </div>
                  <div>
                    <span style="color: white; font-size: 12px;">Cao độ: {{ pitch }}</span>
                    <n-slider v-model:value="pitch" :min="0.5" :max="2" :step="0.1" />
                  </div>
                </n-space>
              </n-space>
            </div>

            <!-- Mic Button -->
            <div style="text-align: center;">
              <n-button
                circle
                size="large"
                :type="isListening ? 'success' : 'error'"
                @click="toggleMic"
                :disabled="!recognition"
                style="width: 100px; height: 100px; font-size: 40px;"
              >
                🎤
              </n-button>
              <div style="color: white; margin-top: 8px; font-size: 12px;">
                {{ micButtonText }}
              </div>
            </div>

            <!-- Conversation -->
            <div v-if="messages.length" class="conversation-box">
              <n-scrollbar style="max-height: 300px;">
                <n-space vertical>
                  <n-card
                    v-for="(msg, index) in messages"
                    :key="index"
                    size="small"
                    :type="msg.isUser ? 'info' : 'success'"
                  >
                    <strong>{{ msg.isUser ? 'Bạn' : 'Robot' }}:</strong> {{ msg.text }}
                  </n-card>
                </n-space>
              </n-scrollbar>
            </div>

            <!-- Input -->
            <n-input
              v-model:value="textInput"
              type="textarea"
              placeholder="Gõ tin nhắn..."
              :rows="3"
              @input="onTyping"
              @keydown.enter.exact.prevent="sendMessage"
            />

            <!-- Buttons -->
            <n-space>
              <n-button 
                type="primary" 
                @click="sendMessage"
                :loading="isSending"
                style="flex: 1"
              >
                {{ isSending ? 'Đang xử lý...' : 'Gửi' }}
              </n-button>
              <n-button type="warning" @click="speakText">
                Nói
              </n-button>
              <n-button @click="clearHistory">
                Xóa
              </n-button>
            </n-space>
          </n-space>
        </n-card>
      </div>
    </n-message-provider>
  </n-config-provider>
</template>

<script setup>
import { ref, computed, onMounted, watch } from 'vue'
import { 
  NConfigProvider, NMessageProvider, NCard, NSpace, NInput, 
  NButton, NSelect, NSlider, NScrollbar, darkTheme, useMessage
} from 'naive-ui'
import RobotCustom from './components/RobotCustom.vue'

// State
const selectedAI = ref('OpenRouter')
const selectedModel = ref('anthropic/claude-3.5-sonnet')
const conversationLang = ref('vi')
const ttsEngine = ref('edge')
const edgeVoice = ref('vi-VN-HoaiMyNeural')
const selectedVoice = ref('')
const availableVoices = ref([])
const rate = ref(1.0)
const pitch = ref(1.0)
const textInput = ref('')
const messages = ref([])
const isTalking = ref(false)
const isSending = ref(false)
const isListening = ref(false)
const pupilX = ref(0)
const pupilY = ref(0)
const isBlinking = ref(false)
const showSettings = ref(false) // Toggle for showing/hiding settings

// AI Provider Options
const aiProviderOptions = [
  { label: '🤖 Gemini', value: 'Gemini' },
  { label: '🧠 OpenAI', value: 'OpenAI' },
  { label: '🔮 DeepSeek', value: 'DeepSeek' },
  { label: '☁️ Qwen', value: 'Qwen' },
  { label: '🌐 OpenRouter', value: 'OpenRouter' }
]

// Model Options by Provider
const modelOptionsByProvider = {
  Gemini: [
    { label: 'Gemini 2.5 Pro', value: 'gemini-2.5-pro' },
    { label: 'Gemini 2.5 Flash', value: 'gemini-2.5-flash' },
    { label: 'Gemini 2.5 Flash Lite', value: 'gemini-2.5-flash-lite' },
    { label: 'Gemini 3 Pro Preview', value: 'gemini-3-pro-preview' },
    { label: 'Gemini 3 Flash Preview', value: 'gemini-3-flash-preview' },
    { label: 'Gemini 2.0 Flash', value: 'gemini-2.0-flash' },
    { label: 'Gemini 2.0 Flash Exp', value: 'gemini-2.0-flash-exp' },
    { label: 'Gemini Flash Latest', value: 'gemini-flash-latest' },
    { label: 'Gemini Pro Latest', value: 'gemini-pro-latest' },
    { label: 'Gemini Exp 1206', value: 'gemini-exp-1206' }
  ],
  OpenAI: [
    { label: 'GPT-5.2 Pro', value: 'gpt-5.2-pro' },
    { label: 'GPT-5.2', value: 'gpt-5.2' },
    { label: 'GPT-5.1', value: 'gpt-5.1' },
    { label: 'GPT-5 Pro', value: 'gpt-5-pro' },
    { label: 'GPT-5', value: 'gpt-5' },
    { label: 'GPT-5 Mini', value: 'gpt-5-mini' },
    { label: 'GPT-5 Nano', value: 'gpt-5-nano' },
    { label: 'GPT-4.1', value: 'gpt-4.1' },
    { label: 'GPT-4.1 Mini', value: 'gpt-4.1-mini' },
    { label: 'GPT-4.1 Nano', value: 'gpt-4.1-nano' },
    { label: 'GPT-4o', value: 'gpt-4o' },
    { label: 'GPT-4o (Nov 2024)', value: 'gpt-4o-2024-11-20' },
    { label: 'GPT-4o (Aug 2024)', value: 'gpt-4o-2024-08-06' },
    { label: 'GPT-4o Mini', value: 'gpt-4o-mini' },
    { label: 'o4 Mini', value: 'o4-mini' },
    { label: 'o3', value: 'o3' },
    { label: 'o3 Mini', value: 'o3-mini' },
    { label: 'o1', value: 'o1' },
    { label: 'GPT-3.5 Turbo', value: 'gpt-3.5-turbo' }
  ],
  DeepSeek: [
    { label: 'DeepSeek Chat', value: 'deepseek-chat' },
    { label: 'DeepSeek Reasoner', value: 'deepseek-reasoner' }
  ],
  Qwen: [
    { label: 'Qwen Max', value: 'qwen-max' },
    { label: 'Qwen Plus', value: 'qwen-plus' },
    { label: 'Qwen Turbo', value: 'qwen-turbo' },
    { label: 'Qwen Long', value: 'qwen-long' },
    { label: 'Qwen2.5 72B', value: 'qwen2.5-72b-instruct' },
    { label: 'Qwen2.5 32B', value: 'qwen2.5-32b-instruct' },
    { label: 'Qwen2.5 14B', value: 'qwen2.5-14b-instruct' },
    { label: 'Qwen2.5 7B', value: 'qwen2.5-7b-instruct' },
    { label: 'Qwen2.5 Coder 32B', value: 'qwen2.5-coder-32b-instruct' },
    { label: 'QwQ 32B Preview', value: 'qwq-32b-preview' }
  ],
  OpenRouter: [
    { label: 'Claude 3.5 Sonnet', value: 'anthropic/claude-3.5-sonnet' },
    { label: 'Claude 3 Opus', value: 'anthropic/claude-3-opus' },
    { label: 'GPT-4o', value: 'openai/gpt-4o' },
    { label: 'GPT-4 Turbo', value: 'openai/gpt-4-turbo' },
    { label: 'Llama 3.3 70B', value: 'meta-llama/llama-3.3-70b-instruct' },
    { label: 'Llama 3.1 405B', value: 'meta-llama/llama-3.1-405b-instruct' },
    { label: 'Gemini Pro 1.5', value: 'google/gemini-pro-1.5' },
    { label: 'Gemini Flash 1.5', value: 'google/gemini-flash-1.5' },
    { label: 'DeepSeek Chat', value: 'deepseek/deepseek-chat' },
    { label: 'Mistral Large', value: 'mistralai/mistral-large' },
    { label: 'Mixtral 8x7B', value: 'mistralai/mixtral-8x7b-instruct' },
    { label: 'Grok 2', value: 'x-ai/grok-2' },
    { label: 'Qwen 2.5 72B', value: 'qwen/qwen-2.5-72b-instruct' }
  ]
}

// Computed model options based on selected AI
const modelOptions = computed(() => modelOptionsByProvider[selectedAI.value] || [])

const langOptions = [
  { label: 'Tiếng Việt', value: 'vi' },
  { label: 'English', value: 'en' },
  { label: '日本語', value: 'ja' },
  { label: '한국어', value: 'ko' },
  { label: '中文', value: 'zh-CN' }
]

const ttsEngineOptions = [
  { label: 'Edge TTS (C# Backend)', value: 'edge' },
  { label: 'Browser TTS', value: 'browser' }
]

const edgeVoiceOptions = [
  { label: 'Hoài My (Nữ - Việt)', value: 'vi-VN-HoaiMyNeural' },
  { label: 'Nam Minh (Nam - Việt)', value: 'vi-VN-NamMinhNeural' },
  { label: 'Jenny (Nữ - Mỹ)', value: 'en-US-JennyNeural' },
  { label: 'Guy (Nam - Mỹ)', value: 'en-US-GuyNeural' },
  { label: 'Nanami (Nữ - Nhật)', value: 'ja-JP-NanamiNeural' },
  { label: 'Sun-Hi (Nữ - Hàn)', value: 'ko-KR-SunHiNeural' },
  { label: 'Xiaoxiao (Nữ - Trung)', value: 'zh-CN-XiaoxiaoNeural' }
]

const voiceOptions = computed(() => 
  availableVoices.value.map(v => ({ label: `${v.name} (${v.lang})`, value: v.name }))
)

// Speech
const synth = window.speechSynthesis
const SpeechRecognition = window.SpeechRecognition || window.webkitSpeechRecognition
const recognition = SpeechRecognition ? new SpeechRecognition() : null

if (recognition) {
  recognition.continuous = false
  recognition.interimResults = false
}

// Computed
const micButtonText = computed(() => {
  if (!recognition) return 'Không hỗ trợ'
  return isListening.value ? 'Đang nghe...' : 'Nhấn để nói'
})

const pupilStyle = computed(() => ({
  transform: `translate(calc(-50% + ${pupilX.value}px), calc(-50% + ${pupilY.value}px))`
}))

const eyeStyle = computed(() => ({
  transform: isBlinking.value ? 'scaleY(0.1)' : 'scaleY(1)',
  transition: 'transform 0.1s ease'
}))

// Methods
const moveEyes = () => {
  const maxMove = 15
  pupilX.value = (Math.random() - 0.5) * maxMove
  pupilY.value = (Math.random() - 0.5) * maxMove
  setTimeout(moveEyes, 1500 + Math.random() * 1500)
}

const blinkEyes = () => {
  isBlinking.value = true
  setTimeout(() => {
    isBlinking.value = false
  }, 150)
  setTimeout(blinkEyes, 3000 + Math.random() * 3000)
}

let typingTimeout
let autoSendTimeout
const onTyping = () => {
  // Nhấp nháy mắt nhẹ khi gõ
  isBlinking.value = true
  clearTimeout(typingTimeout)
  clearTimeout(autoSendTimeout)
  
  typingTimeout = setTimeout(() => {
    isBlinking.value = false
  }, 200)
  
  // Auto-send after 2 seconds of no typing
  autoSendTimeout = setTimeout(() => {
    if (textInput.value.trim()) {
      sendMessage()
    }
  }, 2000)
}

const addMessage = (text, isUser) => {
  messages.value.push({ text, isUser })
}

const callGeminiAPI = async (userMessage) => {
  // Call backend API instead of direct Gemini API
  try {
    // Map provider string to enum number
    const providerMap = {
      'OpenAI': 0,
      'Gemini': 1,
      'DeepSeek': 2,
      'Qwen': 3,
      'OpenRouter': 4
    }

    // Build conversation history
    const history = messages.value.map(msg => ({
      role: msg.isUser ? 'user' : 'assistant',
      content: msg.text
    }))

    const response = await fetch('http://localhost:44246/api/Chat', {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json'
      },
      body: JSON.stringify({
        message: userMessage,
        language: conversationLang.value,
        provider: providerMap[selectedAI.value] ?? 1, // Default to Gemini (1)
        model: selectedModel.value,
        history: history
      })
    })

    if (!response.ok) {
      const errorData = await response.json()
      return `Lỗi: ${errorData.error || 'Không thể kết nối backend'}`
    }

    const data = await response.json()
    return data.response || 'Xin lỗi, tôi không thể trả lời.'
  } catch (error) {
    return 'Đã xảy ra lỗi khi kết nối với backend.'
  }
}

const speakWithEdgeTTS = async (text, voiceName) => {
  if (!text.trim()) return
  isTalking.value = true

  try {
    console.log('Calling TTS API with:', { text, voiceName })
    const response = await fetch('http://localhost:44246/api/Tts', {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json'
      },
      body: JSON.stringify({
        text: text,
        voice: voiceName
      })
    })

    if (!response.ok) {
      throw new Error(`Backend error: ${response.status}`)
    }

    const audioBlob = await response.blob()
    console.log('Received audio blob:', audioBlob.size, 'bytes, type:', audioBlob.type)
    
    if (audioBlob.size === 0) {
      throw new Error('Empty audio blob received')
    }
    
    const audioUrl = URL.createObjectURL(audioBlob)
    console.log('Created blob URL:', audioUrl)
    const audio = new Audio(audioUrl)

    audio.onended = () => {
      console.log('Audio playback ended')
      isTalking.value = false
      URL.revokeObjectURL(audioUrl)
      // Restart listening after speaking
      setTimeout(startContinuousListening, 500)
    }

    audio.onerror = (err) => {
      console.error('Audio playback error:', err, audio.error)
      isTalking.value = false
      URL.revokeObjectURL(audioUrl)
      // Fallback to browser TTS
      console.log('Falling back to browser TTS')
      speakWithBrowserTTS(text)
    }

    try {
      console.log('Starting audio playback...')
      await audio.play()
      console.log('Audio playing')
    } catch (playError) {
      console.error('Audio play error:', playError)
      isTalking.value = false
      URL.revokeObjectURL(audioUrl)
      setTimeout(startContinuousListening, 500)
    }
  } catch (error) {
    console.error('Edge TTS Error:', error)
    isTalking.value = false
    console.log('Falling back to browser TTS due to error')
    speakWithBrowserTTS(text)
  }
}

const speakWithBrowserTTS = (text) => {
  if (!text.trim()) return
  if (synth.speaking) synth.cancel()

  const utterance = new SpeechSynthesisUtterance(text)
  
  if (selectedVoice.value) {
    const voice = availableVoices.value.find(v => v.name === selectedVoice.value)
    if (voice) utterance.voice = voice
  }

  utterance.rate = rate.value
  utterance.pitch = pitch.value
  utterance.lang = conversationLang.value
  utterance.onstart = () => { isTalking.value = true }
  utterance.onend = () => { 
    isTalking.value = false
    // Restart listening after speaking
    setTimeout(startContinuousListening, 500)
  }
  utterance.onerror = () => { 
    isTalking.value = false
    setTimeout(startContinuousListening, 500)
  }
  
  synth.speak(utterance)
}

const speak = async (text) => {
  console.log('speak() called with:', text, 'ttsEngine:', ttsEngine.value)
  if (!text.trim()) {
    console.log('Text is empty, skipping speak')
    return
  }
  if (ttsEngine.value === 'edge') {
    console.log('Using Edge TTS')
    await speakWithEdgeTTS(text, edgeVoice.value)
  } else {
    console.log('Using Browser TTS')
    speakWithBrowserTTS(text)
  }
}

const sendMessage = async () => {
  const userMessage = textInput.value.trim()
  if (!userMessage || isSending.value) return

  // Stop listening while processing
  if (recognition && isListening.value) {
    recognition.stop()
  }

  addMessage(userMessage, true)
  textInput.value = ''
  isSending.value = true

  try {
    const robotResponse = await callGeminiAPI(userMessage)
    console.log('Robot response:', robotResponse)
    addMessage(robotResponse, false)
    console.log('Calling speak with:', robotResponse)
    await speak(robotResponse)
    console.log('Speak completed')
  } finally {
    isSending.value = false
    // Restart listening after response
    setTimeout(() => {
      if (!isTalking.value) {
        startContinuousListening()
      }
    }, 500)
  }
}

const speakText = () => {
  const text = textInput.value.trim()
  if (text) {
    speak(text)
  } else {
    alert('Vui lòng nhập nội dung cần nói!')
  }
}

const clearHistory = () => {
  messages.value = []
}

const toggleMic = () => {
  if (!recognition) return
  
  recognition.lang = conversationLang.value
  
  if (isListening.value) {
    recognition.stop()
  } else {
    recognition.start()
    isListening.value = true
  }
}

const startContinuousListening = async () => {
  if (!recognition) {
    console.log('Speech recognition not supported')
    return
  }
  
  // Don't start if already listening
  if (isListening.value) {
    console.log('Already listening, skipping start')
    return
  }
  
  // Check microphone permission
  try {
    const stream = await navigator.mediaDevices.getUserMedia({ audio: true })
    stream.getTracks().forEach(track => track.stop()) // Stop immediately, just checking permission
  } catch (err) {
    console.error('Microphone permission denied:', err)
    alert('⚠️ Cần cấp quyền microphone để sử dụng tính năng voice!')
    return
  }
  
  try {
    recognition.lang = conversationLang.value
    recognition.continuous = false
    recognition.interimResults = false
    
    console.log('Starting recognition...')
    recognition.start()
    isListening.value = true
  } catch (err) {
    console.error('Failed to start recognition:', err)
    // If already started, just set the flag
    if (err.name === 'InvalidStateError') {
      console.log('Recognition already running, setting flag')
      isListening.value = true
    }
  }
}

const loadVoices = () => {
  const voices = synth.getVoices()
  const lang = conversationLang.value
  availableVoices.value = voices.filter(v => v.lang.includes(lang))
  if (availableVoices.value.length > 0 && !selectedVoice.value) {
    selectedVoice.value = availableVoices.value[0].name
  }
}

// Lifecycle
onMounted(() => {
  moveEyes()
  blinkEyes()

  loadVoices()
  if (synth.onvoiceschanged !== undefined) {
    synth.onvoiceschanged = loadVoices
  }

  if (recognition) {
    recognition.onresult = (event) => {
      const transcript = event.results[0][0].transcript
      console.log('Voice input:', transcript)
      textInput.value = transcript
      isListening.value = false
      setTimeout(() => {
        sendMessage()
        // Restart listening after processing
        setTimeout(() => {
          if (!isTalking.value && !isSending.value) {
            console.log('Restarting listening after message sent')
            startContinuousListening()
          }
        }, 1000)
      }, 500)
    }

    recognition.onerror = (error) => {
      console.error('Recognition error:', error.error, error)
      isListening.value = false
      
      // Don't restart on certain errors
      if (error.error === 'not-allowed' || error.error === 'service-not-allowed') {
        alert('⚠️ Quyền microphone bị từ chối. Vui lòng cấp quyền trong cài đặt trình duyệt.')
        return
      }
      
      // Restart on other errors (except if user stopped it)
      if (error.error !== 'aborted') {
        console.log('Restarting after error:', error.error)
        setTimeout(startContinuousListening, 1000)
      }
    }

    recognition.onend = () => {
      console.log('Recognition ended')
      isListening.value = false
      // Auto-restart if not talking or sending
      if (!isTalking.value && !isSending.value) {
        console.log('Auto-restarting recognition')
        setTimeout(startContinuousListening, 500)
      }
    }
    
    // Start continuous listening on mount
    console.log('Initializing continuous listening...')
    setTimeout(startContinuousListening, 1000)
  }
})

watch(conversationLang, loadVoices)

// Reset model when AI provider changes
watch(selectedAI, (newAI) => {
  const models = modelOptionsByProvider[newAI]
  if (models && models.length > 0) {
    selectedModel.value = models[0].value
  }
})
</script>

<style scoped>
.container {
  text-align: center;
  width: 100%;
  max-width: 500px;
}

/* Robot Container */
.robot-container {
  position: relative;
  width: 300px;
  margin: 0 auto 30px;
  perspective: 1200px;
  filter: drop-shadow(0 30px 60px rgba(0, 0, 0, 0.4));
}

/* Robot Head */
.robot-head {
  background: linear-gradient(145deg, #1a1a2e, #0f0f1e);
  width: 160px;
  height: 160px;
  border-radius: 50%;
  position: relative;
  box-shadow: 
    0 20px 40px rgba(0, 0, 0, 0.5),
    inset 0 -15px 30px rgba(0, 0, 0, 0.6),
    inset 0 15px 30px rgba(59, 130, 246, 0.2);
  margin: 0 auto;
  transform-style: preserve-3d;
  border: 6px solid #2563eb;
  z-index: 10;
}

.robot-head::before {
  content: "";
  position: absolute;
  top: -8px;
  left: -8px;
  right: -8px;
  bottom: -8px;
  background: linear-gradient(145deg, rgba(96, 165, 250, 0.3), rgba(37, 99, 235, 0.1));
  border-radius: 50%;
  z-index: -1;
}

/* Antenna */
.antenna {
  position: absolute;
  top: -35px;
  left: 50%;
  transform: translateX(-50%);
  width: 6px;
  height: 35px;
  background: linear-gradient(to bottom, #3b82f6, #2563eb);
  border-radius: 3px;
  box-shadow: 0 0 10px rgba(59, 130, 246, 0.5);
}

.antenna-ball {
  position: absolute;
  top: -16px;
  left: 50%;
  transform: translateX(-50%);
  width: 20px;
  height: 20px;
  background: linear-gradient(145deg, #fbbf24, #f59e0b);
  border-radius: 50%;
  box-shadow: 
    0 0 30px rgba(251, 191, 36, 1),
    0 0 15px rgba(251, 191, 36, 0.8),
    inset 0 -3px 8px rgba(245, 158, 11, 0.8);
  animation: antennaBlink 2s ease-in-out infinite;
}

@keyframes antennaBlink {
  0%, 100% { 
    opacity: 1;
    box-shadow: 0 0 30px rgba(251, 191, 36, 1), 0 0 15px rgba(251, 191, 36, 0.8), inset 0 -3px 8px rgba(245, 158, 11, 0.8);
  }
  50% { 
    opacity: 0.6;
    box-shadow: 0 0 15px rgba(251, 191, 36, 0.5), 0 0 8px rgba(251, 191, 36, 0.4), inset 0 -3px 8px rgba(245, 158, 11, 0.4);
  }
}

/* Eyes */
.eyes {
  display: flex;
  justify-content: center;
  align-items: center;
  padding: 45px 0 0;
  gap: 25px;
}

.eye {
  width: 45px;
  height: 45px;
  background: #0a0a15;
  border-radius: 50%;
  position: relative;
  box-shadow: 
    0 4px 15px rgba(0, 0, 0, 0.6),
    inset 0 2px 10px rgba(0, 0, 0, 0.8);
  overflow: visible;
  border: 3px solid #1e293b;
}

.eye-shine {
  position: absolute;
  top: 50%;
  left: 50%;
  transform: translate(-50%, -50%);
  width: 32px;
  height: 32px;
  background: radial-gradient(circle at 35% 35%, #60a5fa, #3b82f6, #1e40af);
  border-radius: 50%;
  pointer-events: none;
  box-shadow: 
    0 0 25px rgba(59, 130, 246, 1),
    0 0 40px rgba(59, 130, 246, 0.6),
    inset 0 -4px 8px rgba(30, 64, 175, 0.8);
  animation: eyeGlow 2s ease-in-out infinite;
}

@keyframes eyeGlow {
  0%, 100% { 
    box-shadow: 0 0 25px rgba(59, 130, 246, 1), 0 0 40px rgba(59, 130, 246, 0.6), inset 0 -4px 8px rgba(30, 64, 175, 0.8);
    opacity: 1;
  }
  50% { 
    box-shadow: 0 0 35px rgba(59, 130, 246, 1), 0 0 50px rgba(59, 130, 246, 0.8), inset 0 -4px 8px rgba(30, 64, 175, 1);
    opacity: 0.95;
  }
}

.pupil {
  width: 10px;
  height: 10px;
  background: radial-gradient(circle at 30% 30%, #ffffff, #bfdbfe);
  border-radius: 50%;
  position: absolute;
  top: 35%;
  left: 35%;
  transition: all 0.1s ease;
  box-shadow: 0 0 10px rgba(255, 255, 255, 1);
  z-index: 2;
}

.pupil::after {
  content: "";
  width: 5px;
  height: 5px;
  background: white;
  border-radius: 50%;
  position: absolute;
  top: 1px;
  left: 1px;
  opacity: 1;
  box-shadow: 0 0 6px rgba(255, 255, 255, 1);
}

/* Mouth */
.mouth {
  position: absolute;
  bottom: 25px;
  left: 50%;
  transform: translateX(-50%);
  width: 70px;
  height: 25px;
}

.mouth-line {
  width: 50px;
  height: 25px;
  border: 3px solid #3b82f6;
  border-top: none;
  border-radius: 0 0 25px 25px;
  position: absolute;
  top: 0;
  left: 50%;
  transform: translateX(-50%);
  transition: all 0.2s ease;
  background: linear-gradient(to bottom, transparent, rgba(59, 130, 246, 0.15));
  box-shadow: 0 0 15px rgba(59, 130, 246, 0.4);
}

.mouth.talking .mouth-line {
  animation: mouthTalk 0.3s ease-in-out infinite;
  border-color: #60a5fa;
  box-shadow: 0 0 20px rgba(96, 165, 250, 0.8);
}

@keyframes mouthTalk {
  0%, 100% {
    transform: translateX(-50%) scaleY(1);
    border-width: 3px;
  }
  50% {
    transform: translateX(-50%) scaleY(0.6);
    border-width: 4px;
  }
}

/* Cheek Lights */
.cheek-lights {
  position: absolute;
  bottom: 35px;
  width: 100%;
  display: flex;
  justify-content: space-between;
  padding: 0 15px;
}

.cheek-light {
  width: 8px;
  height: 8px;
  background: radial-gradient(circle at 35% 35%, #60a5fa, #3b82f6);
  border-radius: 50%;
  box-shadow: 
    0 0 15px rgba(59, 130, 246, 0.8),
    inset 0 1px 3px rgba(255, 255, 255, 0.3);
  animation: cheekGlow 3s ease-in-out infinite;
}

.cheek-light.right {
  animation-delay: 1.5s;
}

@keyframes cheekGlow {
  0%, 100% { 
    opacity: 0.7;
    box-shadow: 0 0 15px rgba(59, 130, 246, 0.8), inset 0 1px 3px rgba(255, 255, 255, 0.3);
  }
  50% { 
    opacity: 1;
    box-shadow: 0 0 25px rgba(59, 130, 246, 1), inset 0 1px 3px rgba(255, 255, 255, 0.5);
  }
}

/* Robot Body */
.robot-body {
  width: 140px;
  height: 180px;
  background: linear-gradient(145deg, #ffffff, #e0f2fe);
  border-radius: 50% 50% 50% 50% / 40% 40% 60% 60%;
  margin: -25px auto 0;
  box-shadow: 
    0 20px 50px rgba(0, 0, 0, 0.3),
    inset 0 -15px 30px rgba(59, 130, 246, 0.15),
    inset 0 15px 30px rgba(255, 255, 255, 0.8);
  position: relative;
  z-index: 5;
  border: 5px solid #3b82f6;
}

.robot-body::before {
  content: "";
  position: absolute;
  top: 30%;
  left: 50%;
  transform: translateX(-50%);
  width: 50px;
  height: 50px;
  background: radial-gradient(circle, #3b82f6, #2563eb);
  border-radius: 50%;
  box-shadow: 
    0 0 20px rgba(59, 130, 246, 0.6),
    inset 0 -5px 15px rgba(37, 99, 235, 0.8);
  border: 3px solid #ffffff;
}

.body-panel {
  display: flex;
  gap: 10px;
  justify-content: center;
  padding-top: 110px;
  position: relative;
  z-index: 2;
}

.panel-light {
  width: 10px;
  height: 10px;
  background: rgba(30, 41, 59, 0.4);
  border-radius: 50%;
  transition: all 0.3s ease;
  border: 2px solid rgba(59, 130, 246, 0.3);
  box-shadow: inset 0 2px 4px rgba(0, 0, 0, 0.2);
}

.panel-light.active {
  background: radial-gradient(circle at 35% 35%, #fbbf24, #f59e0b);
  box-shadow: 
    0 0 20px rgba(251, 191, 36, 1),
    inset 0 2px 4px rgba(255, 255, 255, 0.4);
  border-color: rgba(251, 191, 36, 0.8);
  animation: panelBlink 0.5s ease-in-out infinite;
}

@keyframes panelBlink {
  0%, 100% { 
    opacity: 1;
    transform: scale(1);
  }
  50% { 
    opacity: 0.7;
    transform: scale(0.9);
  }
}

/* Robot Arms */
.robot-arms {
  position: absolute;
  top: 160px;
  width: 100%;
  display: flex;
  justify-content: space-between;
  z-index: 3;
  padding: 0 10px;
}

.arm {
  width: 28px;
  height: 70px;
  background: linear-gradient(145deg, #ffffff, #dbeafe);
  border-radius: 14px;
  box-shadow: 
    0 8px 20px rgba(0, 0, 0, 0.25),
    inset 0 -4px 10px rgba(59, 130, 246, 0.15);
  transform-origin: top center;
  border: 4px solid #3b82f6;
  position: relative;
}

.arm::after {
  content: "";
  position: absolute;
  bottom: -12px;
  left: 50%;
  transform: translateX(-50%);
  width: 22px;
  height: 22px;
  background: linear-gradient(145deg, #3b82f6, #2563eb);
  border-radius: 50%;
  border: 3px solid #ffffff;
  box-shadow: 
    0 4px 12px rgba(0, 0, 0, 0.3),
    inset 0 -2px 6px rgba(37, 99, 235, 0.8);
}

.arm.left-arm {
  margin-left: 0;
  transform: rotate(-25deg);
}

.arm.right-arm {
  margin-right: 0;
  transform: rotate(25deg);
}

.arm.waving {
  animation: wave 0.6s ease-in-out infinite;
}

@keyframes wave {
  0%, 100% { transform: rotate(-25deg); }
  50% { transform: rotate(-40deg); }
}

.arm.right-arm.waving {
  animation: waveRight 0.6s ease-in-out infinite;
}

@keyframes waveRight {
  0%, 100% { transform: rotate(25deg); }
  50% { transform: rotate(40deg); }
}

.control-panel {
  background: rgba(255, 255, 255, 0.1);
  backdrop-filter: blur(10px);
}

.conversation-box {
  background: rgba(0, 0, 0, 0.2);
  border-radius: 8px;
  padding: 12px;
}
</style>
