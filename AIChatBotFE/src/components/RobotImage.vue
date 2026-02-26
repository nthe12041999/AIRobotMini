<template>
  <div class="robot-image-container">
    <div class="robot-wrapper" :class="{ talking: isTalking, listening: isListening }">
      <img 
        :src="currentImage" 
        alt="Robot"
        class="robot-img"
        @load="onImageLoad"
      />
    </div>
  </div>
</template>

<script setup>
import { ref, computed, watch } from 'vue'

const props = defineProps({
  isTalking: Boolean,
  isListening: Boolean,
  emotion: {
    type: String,
    default: 'neutral'
  }
})

const imageLoaded = ref(false)

// Map emotions to image files
const emotionMap = {
  neutral: '/images/robot/Frame 1.png',
  happy: '/images/robot/Frame 1-1.png',
  sad: '/images/robot/Frame 1-2.png',
  surprised: '/images/robot/Frame 1-3.png',
  thinking: '/images/robot/Frame 1-4.png',
  love: '/images/robot/Frame 1-5.png',
  sleepy: '/images/robot/Frame 1-6.png',
  talking: '/images/robot/Frame 1-7.png',
  listening: '/images/robot/Frame 1-8.png',
  waving: '/images/robot/Frame 1-9.png',
  laptop: '/images/robot/Frame 1-10.png',
  phone: '/images/robot/Frame 1-11.png'
}

const currentImage = computed(() => {
  if (props.isTalking) return emotionMap.talking
  if (props.isListening) return emotionMap.listening
  return emotionMap[props.emotion] || emotionMap.neutral
})

function onImageLoad() {
  imageLoaded.value = true
}
</script>

<style scoped>
.robot-image-container {
  width: 350px;
  height: 350px;
  margin: 0 auto 30px;
  display: flex;
  justify-content: center;
  align-items: center;
  perspective: 1000px;
}

.robot-wrapper {
  position: relative;
  width: 100%;
  height: 100%;
  transition: transform 0.3s ease;
  filter: drop-shadow(0 20px 40px rgba(0, 0, 0, 0.3));
}

.robot-wrapper.talking {
  animation: robotBounce 0.5s ease-in-out infinite;
}

.robot-wrapper.listening {
  animation: robotPulse 2s ease-in-out infinite;
}

@keyframes robotBounce {
  0%, 100% {
    transform: translateY(0) scale(1);
  }
  50% {
    transform: translateY(-15px) scale(1.03);
  }
}

@keyframes robotPulse {
  0%, 100% {
    transform: scale(1);
    filter: drop-shadow(0 20px 40px rgba(0, 0, 0, 0.3)) brightness(1);
  }
  50% {
    transform: scale(1.05);
    filter: drop-shadow(0 25px 50px rgba(59, 130, 246, 0.4)) brightness(1.1);
  }
}

.robot-img {
  width: 100%;
  height: 100%;
  object-fit: contain;
  transition: opacity 0.3s ease;
  image-rendering: -webkit-optimize-contrast;
  image-rendering: crisp-edges;
}

.robot-img:not([src]) {
  opacity: 0;
}
</style>
