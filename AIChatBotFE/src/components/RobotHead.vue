<template>
  <div class="robot-head-container">
    <div class="robot-head" :class="{ talking: isTalking, listening: isListening }">
      <!-- Antenna -->
      <div class="antenna">
        <div class="antenna-ball"></div>
      </div>
      
      <!-- Head sphere -->
      <div class="head-sphere">
        <!-- Eyes -->
        <div class="eyes">
          <div class="eye left-eye" :class="eyeClass">
            <div class="eye-glow" v-if="emotion !== 'sleepy' && emotion !== 'love'"></div>
            <div class="pupil" :style="pupilStyle" v-if="emotion !== 'sleepy' && emotion !== 'love'"></div>
            <div class="heart" v-if="emotion === 'love'">💗</div>
          </div>
          <div class="eye right-eye" :class="eyeClass">
            <div class="eye-glow" v-if="emotion !== 'sleepy' && emotion !== 'love'"></div>
            <div class="pupil" :style="pupilStyle" v-if="emotion !== 'sleepy' && emotion !== 'love'"></div>
            <div class="heart" v-if="emotion === 'love'">💗</div>
          </div>
        </div>
        
        <!-- Mouth -->
        <div class="mouth" :class="[mouthClass, { active: isTalking }]">
          <div class="mouth-line"></div>
        </div>
        
        <!-- Speech Bubble (when talking) -->
        <div class="speech-bubble" v-if="isTalking">
          <div class="dots">
            <span></span>
            <span></span>
            <span></span>
          </div>
        </div>
        
        <!-- Thinking dots -->
        <div class="thinking-dots" v-if="emotion === 'thinking'">
          <span>.</span>
          <span>.</span>
          <span>.</span>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted, onUnmounted } from 'vue'

const props = defineProps({
  isTalking: Boolean,
  isListening: Boolean,
  emotion: {
    type: String,
    default: 'neutral' // neutral, happy, sad, surprised, sleepy, love, thinking
  }
})

const pupilX = ref(0)
const pupilY = ref(0)

const pupilStyle = computed(() => ({
  transform: `translate(calc(-50% + ${pupilX.value}px), calc(-50% + ${pupilY.value}px))`
}))

const eyeClass = computed(() => ({
  'eye-happy': props.emotion === 'happy',
  'eye-sad': props.emotion === 'sad',
  'eye-surprised': props.emotion === 'surprised',
  'eye-sleepy': props.emotion === 'sleepy',
  'eye-love': props.emotion === 'love'
}))

const mouthClass = computed(() => ({
  'mouth-happy': props.emotion === 'happy',
  'mouth-sad': props.emotion === 'sad',
  'mouth-surprised': props.emotion === 'surprised',
  'mouth-sleepy': props.emotion === 'sleepy'
}))

let eyeInterval = null

function moveEyes() {
  const maxMove = 8
  pupilX.value = (Math.random() - 0.5) * maxMove
  pupilY.value = (Math.random() - 0.5) * maxMove
}

onMounted(() => {
  eyeInterval = setInterval(moveEyes, 2000)
})

onUnmounted(() => {
  if (eyeInterval) clearInterval(eyeInterval)
})
</script>

<style scoped>
.robot-head-container {
  width: 300px;
  height: 300px;
  margin: 0 auto 30px;
  display: flex;
  justify-content: center;
  align-items: center;
  perspective: 1000px;
}

.robot-head {
  position: relative;
  width: 100%;
  height: 100%;
  transform-style: preserve-3d;
  transition: transform 0.3s ease;
}

.robot-head.talking {
  animation: headBounce 0.5s ease-in-out infinite;
}

.robot-head.listening {
  animation: headPulse 2s ease-in-out infinite;
}

@keyframes headBounce {
  0%, 100% { transform: translateY(0) scale(1); }
  50% { transform: translateY(-10px) scale(1.02); }
}

@keyframes headPulse {
  0%, 100% { transform: scale(1); }
  50% { transform: scale(1.05); }
}

/* Antenna */
.antenna {
  position: absolute;
  top: -50px;
  left: 50%;
  transform: translateX(-50%);
  width: 8px;
  height: 50px;
  background: linear-gradient(to bottom, #60a5fa, #3b82f6);
  border-radius: 4px;
  z-index: 10;
}

.antenna-ball {
  position: absolute;
  top: -20px;
  left: 50%;
  transform: translateX(-50%);
  width: 24px;
  height: 24px;
  background: radial-gradient(circle at 35% 35%, #fbbf24, #f59e0b);
  border-radius: 50%;
  box-shadow: 
    0 0 30px rgba(251, 191, 36, 0.8),
    0 0 15px rgba(251, 191, 36, 0.6);
  animation: antennaBlink 2s ease-in-out infinite;
}

@keyframes antennaBlink {
  0%, 100% { 
    opacity: 1;
    box-shadow: 0 0 30px rgba(251, 191, 36, 0.8), 0 0 15px rgba(251, 191, 36, 0.6);
  }
  50% { 
    opacity: 0.6;
    box-shadow: 0 0 15px rgba(251, 191, 36, 0.4), 0 0 8px rgba(251, 191, 36, 0.3);
  }
}

/* Head Sphere */
.head-sphere {
  width: 220px;
  height: 220px;
  margin: 40px auto 0;
  background: 
    radial-gradient(circle at 35% 35%, #3d4a5c, #1a202c),
    linear-gradient(145deg, #2d3748, #1a1f2e);
  border-radius: 50%;
  position: relative;
  box-shadow: 
    0 35px 70px rgba(0, 0, 0, 0.6),
    inset 0 -25px 50px rgba(0, 0, 0, 0.7),
    inset 0 25px 50px rgba(96, 165, 250, 0.2),
    0 0 0 8px #2563eb,
    0 0 0 12px rgba(37, 99, 235, 0.3);
  border: 2px solid #3b82f6;
}

.head-sphere::before {
  content: "";
  position: absolute;
  top: 15px;
  left: 25px;
  width: 80px;
  height: 80px;
  background: radial-gradient(circle at 40% 40%, rgba(255, 255, 255, 0.25), transparent 60%);
  border-radius: 50%;
  pointer-events: none;
  filter: blur(8px);
}

.head-sphere::after {
  content: "";
  position: absolute;
  bottom: 20px;
  right: 20px;
  width: 40px;
  height: 40px;
  background: radial-gradient(circle, rgba(59, 130, 246, 0.3), transparent);
  border-radius: 50%;
  filter: blur(10px);
}

/* Eyes */
.eyes {
  display: flex;
  justify-content: center;
  align-items: center;
  gap: 40px;
  padding-top: 60px;
}

.eye {
  width: 55px;
  height: 55px;
  background: 
    radial-gradient(circle at 50% 50%, #0a0a15, #000000);
  border-radius: 50%;
  position: relative;
  box-shadow: 
    0 6px 20px rgba(0, 0, 0, 0.8),
    inset 0 3px 15px rgba(0, 0, 0, 0.9),
    0 0 0 4px #1e293b,
    0 0 20px rgba(59, 130, 246, 0.3);
  border: 2px solid #0f172a;
  overflow: hidden;
  transition: all 0.3s ease;
}

.eye::before {
  content: "";
  position: absolute;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: radial-gradient(circle at 30% 30%, rgba(59, 130, 246, 0.1), transparent);
  border-radius: 50%;
}

.eye-glow {
  position: absolute;
  top: 50%;
  left: 50%;
  transform: translate(-50%, -50%);
  width: 42px;
  height: 42px;
  background: 
    radial-gradient(circle at 35% 35%, #7dd3fc, #60a5fa, #3b82f6, #1e40af);
  border-radius: 50%;
  box-shadow: 
    0 0 35px rgba(59, 130, 246, 1),
    0 0 60px rgba(59, 130, 246, 0.7),
    inset 0 -5px 15px rgba(30, 64, 175, 0.8);
  animation: eyeGlow 2s ease-in-out infinite;
  filter: brightness(1.2);
}

@keyframes eyeGlow {
  0%, 100% { 
    opacity: 1;
    box-shadow: 0 0 35px rgba(59, 130, 246, 1), 0 0 60px rgba(59, 130, 246, 0.7), inset 0 -5px 15px rgba(30, 64, 175, 0.8);
    filter: brightness(1.2);
  }
  50% { 
    opacity: 0.95;
    box-shadow: 0 0 45px rgba(59, 130, 246, 1), 0 0 70px rgba(59, 130, 246, 0.9), inset 0 -5px 15px rgba(30, 64, 175, 1);
    filter: brightness(1.3);
  }
}

.pupil {
  width: 14px;
  height: 14px;
  background: 
    radial-gradient(circle at 35% 35%, #ffffff, #e0f2fe, #bfdbfe);
  border-radius: 50%;
  position: absolute;
  top: 50%;
  left: 50%;
  transition: transform 0.3s ease;
  box-shadow: 
    0 0 12px rgba(255, 255, 255, 1),
    0 0 20px rgba(255, 255, 255, 0.6);
  z-index: 2;
}

.pupil::after {
  content: "";
  width: 7px;
  height: 7px;
  background: 
    radial-gradient(circle at 40% 40%, #ffffff, rgba(255, 255, 255, 0.8));
  border-radius: 50%;
  position: absolute;
  top: 2px;
  left: 2px;
  box-shadow: 0 0 8px rgba(255, 255, 255, 1);
}

/* Eye variations */
.eye.eye-happy {
  transform: scaleY(0.6) translateY(-5px);
  border-radius: 50% 50% 80% 80%;
}

.eye.eye-sad {
  transform: scaleY(0.8) translateY(5px);
}

.eye.eye-surprised {
  transform: scale(1.3);
}

.eye.eye-sleepy {
  transform: scaleY(0.1);
  background: #1e293b;
}

.eye.eye-love {
  background: transparent;
  border: none;
  box-shadow: none;
}

.heart {
  font-size: 32px;
  position: absolute;
  top: 50%;
  left: 50%;
  transform: translate(-50%, -50%);
  animation: heartBeat 1s ease-in-out infinite;
}

@keyframes heartBeat {
  0%, 100% { transform: translate(-50%, -50%) scale(1); }
  50% { transform: translate(-50%, -50%) scale(1.2); }
}

/* Mouth variations */
.mouth-happy .mouth-line {
  border-color: #3b82f6;
  border-radius: 0 0 40px 40px;
  height: 35px;
  width: 70px;
}

.mouth-sad .mouth-line {
  border-color: #3b82f6;
  border-radius: 40px 40px 0 0;
  border-top: 4px solid #3b82f6;
  border-bottom: none;
  transform: translateX(-50%) translateY(-10px);
}

.mouth-surprised .mouth-line {
  border-radius: 50%;
  border: 4px solid #3b82f6;
  width: 30px;
  height: 30px;
  background: rgba(59, 130, 246, 0.2);
}

.mouth-sleepy .mouth-line {
  width: 40px;
  height: 8px;
  border: none;
  background: #3b82f6;
  border-radius: 4px;
}

/* Speech Bubble */
.speech-bubble {
  position: absolute;
  top: -60px;
  right: -80px;
  background: white;
  padding: 12px 18px;
  border-radius: 20px;
  box-shadow: 0 4px 15px rgba(0, 0, 0, 0.2);
  z-index: 20;
}

.speech-bubble::after {
  content: "";
  position: absolute;
  bottom: -10px;
  left: 20px;
  width: 0;
  height: 0;
  border-left: 10px solid transparent;
  border-right: 10px solid transparent;
  border-top: 10px solid white;
}

.dots {
  display: flex;
  gap: 6px;
}

.dots span {
  width: 8px;
  height: 8px;
  background: #3b82f6;
  border-radius: 50%;
  animation: dotBounce 1s ease-in-out infinite;
}

.dots span:nth-child(2) {
  animation-delay: 0.2s;
}

.dots span:nth-child(3) {
  animation-delay: 0.4s;
}

@keyframes dotBounce {
  0%, 100% { transform: translateY(0); }
  50% { transform: translateY(-8px); }
}

/* Thinking dots */
.thinking-dots {
  position: absolute;
  top: -40px;
  right: -60px;
  font-size: 32px;
  color: #3b82f6;
  font-weight: bold;
  animation: thinkingFloat 2s ease-in-out infinite;
}

.thinking-dots span {
  animation: thinkingBlink 1.5s ease-in-out infinite;
}

.thinking-dots span:nth-child(2) {
  animation-delay: 0.3s;
}

.thinking-dots span:nth-child(3) {
  animation-delay: 0.6s;
}

@keyframes thinkingFloat {
  0%, 100% { transform: translateY(0); }
  50% { transform: translateY(-10px); }
}

@keyframes thinkingBlink {
  0%, 100% { opacity: 0.3; }
  50% { opacity: 1; }
}
.mouth {
  position: absolute;
  bottom: 40px;
  left: 50%;
  transform: translateX(-50%);
  width: 80px;
  height: 30px;
}

.mouth-line {
  width: 60px;
  height: 28px;
  border: 5px solid #3b82f6;
  border-top: none;
  border-radius: 0 0 35px 35px;
  position: absolute;
  top: 0;
  left: 50%;
  transform: translateX(-50%);
  transition: all 0.3s ease;
  background: 
    linear-gradient(to bottom, transparent, rgba(59, 130, 246, 0.15));
  box-shadow: 
    0 0 20px rgba(59, 130, 246, 0.5),
    inset 0 -3px 10px rgba(59, 130, 246, 0.3);
}

.mouth.active .mouth-line {
  animation: mouthTalk 0.3s ease-in-out infinite;
  border-color: #60a5fa;
  box-shadow: 
    0 0 30px rgba(96, 165, 250, 0.9),
    inset 0 -3px 10px rgba(96, 165, 250, 0.5);
  filter: brightness(1.2);
}

@keyframes mouthTalk {
  0%, 100% {
    transform: translateX(-50%) scaleY(1);
    border-width: 4px;
  }
  50% {
    transform: translateX(-50%) scaleY(0.6);
    border-width: 5px;
  }
}
</style>
