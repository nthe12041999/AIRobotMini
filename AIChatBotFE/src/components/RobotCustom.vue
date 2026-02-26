<template>
  <div class="robot-custom-container">
    <div class="robot" :class="{ talking: isTalking, listening: isListening }">
      
      <!-- Antenna -->
      <div class="antenna">
        <div class="antenna-tip"></div>
      </div>
      
      <!-- Head -->
      <div class="head">
        <!-- Screen bezel -->
        <div class="screen-bezel">
          <div class="screen">
            <!-- Eyes -->
            <div class="eyes" :class="[eyeClass, { blinking: isBlinking }]">
              <div class="eye left-eye">
                <div class="eye-inner"></div>
              </div>
              <div class="eye right-eye">
                <div class="eye-inner"></div>
              </div>
            </div>
            
            <!-- Mouth -->
            <div class="mouth" :class="[mouthClass, { typing: isBlinking }]">
              <div class="mouth-shape"></div>
            </div>
          </div>
        </div>
      </div>
      
      <!-- Body -->
      <div class="body">
        <div class="body-light" :class="{ active: isTalking }"></div>
        <div class="body-panel">
          <div class="panel-line"></div>
          <div class="panel-line"></div>
          <div class="panel-line"></div>
        </div>
      </div>
      
      <!-- Arms -->
      <div class="arms">
        <div class="arm left-arm">
          <div class="hand"></div>
        </div>
        <div class="arm right-arm">
          <div class="hand"></div>
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
  isBlinking: Boolean,
  emotion: {
    type: String,
    default: 'happy'
  }
})

const eyeClass = computed(() => ({
  'eyes-happy': props.emotion === 'happy',
  'eyes-sad': props.emotion === 'sad',
  'eyes-surprised': props.emotion === 'surprised',
  'eyes-sleepy': props.emotion === 'sleepy',
  'eyes-love': props.emotion === 'love'
}))

const mouthClass = computed(() => ({
  'mouth-happy': props.emotion === 'happy',
  'mouth-sad': props.emotion === 'sad',
  'mouth-surprised': props.emotion === 'surprised',
  'mouth-talking': props.isTalking
}))
</script>

<style scoped>
.robot-custom-container {
  width: 400px;
  height: 450px;
  margin: 0 auto 30px;
  display: flex;
  justify-content: center;
  align-items: center;
  perspective: 1200px;
}

.robot {
  position: relative;
  width: 280px;
  height: 400px;
  transition: transform 0.3s ease;
}

.robot.talking {
  animation: robotBounce 0.6s ease-in-out infinite;
}

.robot.listening {
  animation: robotPulse 2s ease-in-out infinite;
}

@keyframes robotBounce {
  0%, 100% { transform: translateY(0); }
  50% { transform: translateY(-12px); }
}

@keyframes robotPulse {
  0%, 100% { transform: scale(1); }
  50% { transform: scale(1.03); }
}

/* Antenna */
.antenna {
  position: absolute;
  top: -45px;
  left: 50%;
  transform: translateX(-50%);
  width: 6px;
  height: 40px;
  background: linear-gradient(to bottom, #60a5fa, #3b82f6);
  border-radius: 3px;
  box-shadow: 0 0 15px rgba(59, 130, 246, 0.5);
  z-index: 10;
}

.antenna-tip {
  position: absolute;
  top: -18px;
  left: 50%;
  transform: translateX(-50%);
  width: 22px;
  height: 22px;
  background: radial-gradient(circle at 35% 35%, #fbbf24, #f59e0b);
  border-radius: 50%;
  box-shadow: 
    0 0 25px rgba(251, 191, 36, 0.9),
    0 0 40px rgba(251, 191, 36, 0.5);
  animation: antennaBlink 2s ease-in-out infinite;
}

@keyframes antennaBlink {
  0%, 100% { opacity: 1; }
  50% { opacity: 0.5; }
}

/* Head */
.head {
  width: 240px;
  height: 240px;
  margin: 0 auto;
  background: 
    radial-gradient(circle at 35% 35%, #f8f9fa, #e9ecef, #dee2e6);
  border-radius: 50%;
  box-shadow: 
    0 35px 70px rgba(0, 0, 0, 0.4),
    inset 0 -25px 50px rgba(0, 0, 0, 0.08),
    inset 0 25px 50px rgba(255, 255, 255, 0.9);
  position: relative;
  display: flex;
  align-items: center;
  justify-content: center;
}

.head::before {
  content: "";
  position: absolute;
  top: 20px;
  left: 30px;
  width: 70px;
  height: 70px;
  background: radial-gradient(circle at 40% 40%, rgba(255, 255, 255, 0.4), transparent);
  border-radius: 50%;
  filter: blur(15px);
}

/* Screen Bezel */
.screen-bezel {
  width: 190px;
  height: 190px;
  background: 
    linear-gradient(145deg, #4a5568, #2d3748);
  border-radius: 28% 28% 32% 32% / 28% 28% 32% 32%;
  padding: 12px;
  box-shadow: 
    0 0 0 6px #3a4556,
    inset 0 8px 20px rgba(0, 0, 0, 0.6),
    inset 0 -8px 20px rgba(0, 0, 0, 0.4);
}

.screen {
  width: 100%;
  height: 100%;
  background: 
    radial-gradient(circle at 50% 50%, #1e293b, #0f172a);
  border-radius: 20% 20% 24% 24% / 20% 20% 24% 24%;
  position: relative;
  overflow: hidden;
}

.screen::before {
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
      rgba(0, 0, 0, 0.15) 2px,
      rgba(0, 0, 0, 0.15) 4px
    );
  pointer-events: none;
}

/* Eyes */
.eyes {
  display: flex;
  justify-content: center;
  gap: 35px;
  padding-top: 40px;
  transition: all 0.3s ease;
}

.eye {
  width: 35px;
  height: 55px;
  background: #00ffff;
  border-radius: 4px;
  box-shadow: 
    0 0 25px rgba(0, 255, 255, 0.9),
    0 0 45px rgba(0, 255, 255, 0.5),
    inset 0 0 15px rgba(0, 255, 255, 0.4);
  position: relative;
  animation: eyeGlow 2s ease-in-out infinite;
}

.eye-inner {
  position: absolute;
  top: 8px;
  left: 8px;
  width: 19px;
  height: 39px;
  background: 
    linear-gradient(to bottom,
      rgba(255, 255, 255, 0.3) 0%,
      transparent 50%
    );
  border-radius: 2px;
}

@keyframes eyeGlow {
  0%, 100% { 
    opacity: 1;
    box-shadow: 0 0 25px rgba(0, 255, 255, 0.9), 0 0 45px rgba(0, 255, 255, 0.5);
  }
  50% { 
    opacity: 0.95;
    box-shadow: 0 0 35px rgba(0, 255, 255, 1), 0 0 55px rgba(0, 255, 255, 0.7);
  }
}

/* Eye emotions */
.eyes.blinking .eye {
  transform: scaleY(0.1);
  transition: transform 0.2s ease;
}

.eyes-happy .eye {
  height: 45px;
  border-radius: 4px 4px 50% 50%;
}

.eyes-sad .eye {
  height: 40px;
  transform: translateY(8px);
}

.eyes-surprised .eye {
  height: 70px;
  width: 40px;
}

.eyes-sleepy .eye {
  height: 8px;
  background: #00ffff;
}

.eyes-love .eye {
  border-radius: 50% 50% 0 0;
  transform: rotate(45deg);
}

/* Mouth */
.mouth {
  position: absolute;
  bottom: 30px;
  left: 50%;
  transform: translateX(-50%);
  width: 100px;
  height: 35px;
}

.mouth-shape {
  width: 100%;
  height: 100%;
  border: 5px solid #00ffff;
  border-top: none;
  border-radius: 0 0 50px 50px;
  box-shadow: 
    0 0 20px rgba(0, 255, 255, 0.8),
    inset 0 -5px 15px rgba(0, 255, 255, 0.3);
  transition: all 0.15s ease;
}

.mouth.typing .mouth-shape {
  height: 25px;
  border-radius: 0 0 40px 40px;
  border-width: 4px;
}

.mouth-happy .mouth-shape {
  border-radius: 0 0 60px 60px;
  height: 40px;
}

.mouth-sad .mouth-shape {
  border-radius: 60px 60px 0 0;
  border-top: 5px solid #00ffff;
  border-bottom: none;
  transform: translateY(-10px);
}

.mouth-surprised .mouth-shape {
  border-radius: 50%;
  border: 5px solid #00ffff;
  width: 40px;
  height: 40px;
  margin: 0 auto;
}

.mouth-talking .mouth-shape {
  animation: mouthTalk 0.3s ease-in-out infinite;
}

@keyframes mouthTalk {
  0%, 100% {
    transform: scaleY(1);
    border-width: 5px;
  }
  50% {
    transform: scaleY(0.6);
    border-width: 6px;
  }
}

/* Body */
.body {
  width: 160px;
  height: 140px;
  margin: -15px auto 0;
  background: 
    linear-gradient(145deg, #f8f9fa, #e9ecef);
  border-radius: 30% 30% 45% 45% / 25% 25% 50% 50%;
  box-shadow: 
    0 25px 50px rgba(0, 0, 0, 0.3),
    inset 0 -15px 30px rgba(0, 0, 0, 0.08),
    inset 0 15px 30px rgba(255, 255, 255, 0.6);
  position: relative;
  z-index: 5;
}

.body-light {
  position: absolute;
  top: 30px;
  left: 50%;
  transform: translateX(-50%);
  width: 45px;
  height: 45px;
  background: radial-gradient(circle, #cbd5e1, #94a3b8);
  border-radius: 50%;
  box-shadow: 
    inset 0 3px 10px rgba(0, 0, 0, 0.2);
  transition: all 0.3s ease;
}

.body-light.active {
  background: radial-gradient(circle, #3b82f6, #2563eb);
  box-shadow: 
    0 0 25px rgba(59, 130, 246, 0.8),
    inset 0 3px 10px rgba(0, 0, 0, 0.3);
  animation: lightPulse 0.5s ease-in-out infinite;
}

@keyframes lightPulse {
  0%, 100% { opacity: 1; }
  50% { opacity: 0.7; }
}

.body-panel {
  position: absolute;
  bottom: 25px;
  left: 50%;
  transform: translateX(-50%);
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.panel-line {
  width: 80px;
  height: 4px;
  background: linear-gradient(to right, transparent, #94a3b8, transparent);
  border-radius: 2px;
}

/* Arms */
.arms {
  position: absolute;
  top: 240px;
  width: 100%;
  display: flex;
  justify-content: space-between;
  z-index: 3;
}

.arm {
  width: 35px;
  height: 90px;
  background: 
    linear-gradient(145deg, #f8f9fa, #e9ecef);
  border-radius: 18px;
  box-shadow: 
    0 10px 25px rgba(0, 0, 0, 0.2),
    inset 0 -5px 15px rgba(0, 0, 0, 0.08);
  position: relative;
}

.left-arm {
  margin-left: 15px;
  transform: rotate(-15deg);
  transform-origin: top center;
}

.right-arm {
  margin-right: 15px;
  transform: rotate(15deg);
  transform-origin: top center;
}

.hand {
  position: absolute;
  bottom: -12px;
  left: 50%;
  transform: translateX(-50%);
  width: 28px;
  height: 28px;
  background: 
    radial-gradient(circle at 35% 35%, #3b82f6, #2563eb);
  border-radius: 50%;
  box-shadow: 
    0 5px 15px rgba(0, 0, 0, 0.3),
    inset 0 -3px 8px rgba(37, 99, 235, 0.8);
  border: 3px solid #f8f9fa;
}

.robot.talking .left-arm {
  animation: armWaveLeft 0.6s ease-in-out infinite;
}

.robot.talking .right-arm {
  animation: armWaveRight 0.6s ease-in-out infinite;
}

@keyframes armWaveLeft {
  0%, 100% { transform: rotate(-15deg); }
  50% { transform: rotate(-25deg); }
}

@keyframes armWaveRight {
  0%, 100% { transform: rotate(15deg); }
  50% { transform: rotate(25deg); }
}
</style>
