<template>
  <div class="robot-pixel-container">
    <div class="robot-head" :class="{ talking: isTalking, listening: isListening }">
      <!-- Side ears/handles -->
      <div class="ear left-ear"></div>
      <div class="ear right-ear"></div>
      
      <!-- Screen -->
      <div class="screen">
        <div class="screen-inner">
          <!-- Eyes -->
          <div class="pixel-eyes">
            <div class="pixel-eye left" :class="eyeClass"></div>
            <div class="pixel-eye right" :class="eyeClass"></div>
          </div>
          
          <!-- Mouth -->
          <div class="pixel-mouth" :class="mouthClass"></div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { computed } from 'vue'

const props = defineProps({
  isTalking: Boolean,
  isListening: Boolean,
  emotion: {
    type: String,
    default: 'happy'
  }
})

const eyeClass = computed(() => ({
  'eye-happy': props.emotion === 'happy',
  'eye-sad': props.emotion === 'sad',
  'eye-surprised': props.emotion === 'surprised',
  'eye-sleepy': props.emotion === 'sleepy'
}))

const mouthClass = computed(() => ({
  'mouth-happy': props.emotion === 'happy',
  'mouth-sad': props.emotion === 'sad',
  'mouth-surprised': props.emotion === 'surprised',
  'mouth-talking': props.isTalking
}))
</script>

<style scoped>
.robot-pixel-container {
  width: 350px;
  height: 350px;
  margin: 0 auto 30px;
  display: flex;
  justify-content: center;
  align-items: center;
  perspective: 1000px;
}

.robot-head {
  position: relative;
  width: 280px;
  height: 280px;
  background: 
    radial-gradient(circle at 35% 35%, #f5f5f5, #e0e0e0, #d0d0d0);
  border-radius: 50%;
  box-shadow: 
    0 30px 60px rgba(0, 0, 0, 0.3),
    inset 0 -20px 40px rgba(0, 0, 0, 0.1),
    inset 0 20px 40px rgba(255, 255, 255, 0.8);
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
  50% { transform: translateY(-8px) scale(1.02); }
}

@keyframes headPulse {
  0%, 100% { transform: scale(1); }
  50% { transform: scale(1.03); }
}

/* Ears/Handles */
.ear {
  position: absolute;
  top: 50px;
  width: 60px;
  height: 80px;
  background: 
    linear-gradient(145deg, #f5f5f5, #d0d0d0);
  border-radius: 15px;
  box-shadow: 
    0 10px 25px rgba(0, 0, 0, 0.2),
    inset 0 -5px 15px rgba(0, 0, 0, 0.1),
    inset 0 5px 15px rgba(255, 255, 255, 0.6);
}

.ear.left-ear {
  left: -40px;
  transform: rotate(-10deg);
}

.ear.right-ear {
  right: -40px;
  transform: rotate(10deg);
}

/* Screen */
.screen {
  position: absolute;
  top: 50%;
  left: 50%;
  transform: translate(-50%, -50%);
  width: 220px;
  height: 220px;
  background: 
    linear-gradient(145deg, #2a2a2a, #1a1a1a);
  border-radius: 25% 25% 30% 30% / 25% 25% 30% 30%;
  box-shadow: 
    0 0 0 8px #3a3a3a,
    0 0 0 12px #2a2a2a,
    inset 0 10px 30px rgba(0, 0, 0, 0.8),
    inset 0 -10px 30px rgba(0, 0, 0, 0.5);
  overflow: hidden;
}

.screen-inner {
  width: 100%;
  height: 100%;
  background: 
    radial-gradient(circle at 50% 50%, #2a2a2a, #1a1a1a);
  position: relative;
}

/* Pixel Eyes */
.pixel-eyes {
  display: flex;
  justify-content: center;
  gap: 50px;
  padding-top: 50px;
}

.pixel-eye {
  width: 50px;
  height: 70px;
  background: 
    linear-gradient(to bottom, 
      #00ffff 0%, 
      #00ffff 10%, 
      transparent 10%, 
      transparent 20%,
      #00ffff 20%,
      #00ffff 30%,
      transparent 30%,
      transparent 40%,
      #00ffff 40%,
      #00ffff 50%,
      transparent 50%,
      transparent 60%,
      #00ffff 60%,
      #00ffff 70%,
      transparent 70%,
      transparent 80%,
      #00ffff 80%,
      #00ffff 90%,
      transparent 90%
    );
  background-size: 100% 10px;
  box-shadow: 
    0 0 20px rgba(0, 255, 255, 0.8),
    0 0 40px rgba(0, 255, 255, 0.4),
    inset 0 0 10px rgba(0, 255, 255, 0.3);
  border-radius: 3px;
  animation: pixelGlow 2s ease-in-out infinite;
  position: relative;
}

.pixel-eye::before {
  content: "";
  position: absolute;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: 
    repeating-linear-gradient(
      0deg,
      transparent,
      transparent 2px,
      rgba(0, 0, 0, 0.3) 2px,
      rgba(0, 0, 0, 0.3) 4px
    );
}

@keyframes pixelGlow {
  0%, 100% { 
    opacity: 1;
    box-shadow: 0 0 20px rgba(0, 255, 255, 0.8), 0 0 40px rgba(0, 255, 255, 0.4);
  }
  50% { 
    opacity: 0.95;
    box-shadow: 0 0 30px rgba(0, 255, 255, 1), 0 0 50px rgba(0, 255, 255, 0.6);
  }
}

/* Pixel Mouth */
.pixel-mouth {
  position: absolute;
  bottom: 50px;
  left: 50%;
  transform: translateX(-50%);
  width: 140px;
  height: 40px;
  border: 6px solid #00ffff;
  border-top: none;
  border-radius: 0 0 70px 70px;
  box-shadow: 
    0 0 20px rgba(0, 255, 255, 0.8),
    0 0 40px rgba(0, 255, 255, 0.4),
    inset 0 -5px 15px rgba(0, 255, 255, 0.3);
  transition: all 0.3s ease;
}

.pixel-mouth::before {
  content: "";
  position: absolute;
  bottom: 0;
  left: 0;
  right: 0;
  height: 100%;
  background: 
    repeating-linear-gradient(
      90deg,
      #00ffff,
      #00ffff 3px,
      transparent 3px,
      transparent 6px
    );
  opacity: 0.3;
  border-radius: 0 0 70px 70px;
}

.pixel-mouth.mouth-talking {
  animation: mouthTalk 0.3s ease-in-out infinite;
}

@keyframes mouthTalk {
  0%, 100% {
    transform: translateX(-50%) scaleY(1);
    border-width: 6px;
  }
  50% {
    transform: translateX(-50%) scaleY(0.7);
    border-width: 8px;
  }
}

/* Emotion variations */
.pixel-eye.eye-sad {
  height: 50px;
  transform: translateY(10px);
}

.pixel-eye.eye-surprised {
  height: 90px;
  width: 60px;
}

.pixel-eye.eye-sleepy {
  height: 10px;
  background: #00ffff;
}

.pixel-mouth.mouth-sad {
  border-radius: 70px 70px 0 0;
  border-top: 6px solid #00ffff;
  border-bottom: none;
  transform: translateX(-50%) translateY(10px);
}

.pixel-mouth.mouth-surprised {
  border-radius: 50%;
  border: 6px solid #00ffff;
  width: 50px;
  height: 50px;
}
</style>
