<template>
  <div class="robot-sprite-container">
    <div 
      class="robot-sprite"
      :class="{ 
        'talking': isTalking,
        'listening': isListening 
      }"
      :style="spriteStyle"
    ></div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted, onUnmounted, watch } from 'vue'

const props = defineProps({
  isTalking: Boolean,
  isListening: Boolean
})

// Sprite sheet configuration
const SPRITE_WIDTH = 340
const SPRITE_HEIGHT = 320
const COLS = 10
const ROWS = 8

// Animation states
const currentFrame = ref(0)
const currentRow = ref(0)
let animationInterval = null

// Sprite positions for different states
const ANIMATIONS = {
  idle: { row: 0, frames: 10, speed: 200 },
  talking: { row: 6, frames: 10, speed: 100 },
  listening: { row: 4, frames: 10, speed: 150 },
  happy: { row: 7, frames: 6, speed: 150 }
}

const spriteStyle = computed(() => ({
  backgroundImage: 'url(/images/robot-sprite.png)',
  backgroundPosition: `-${currentFrame.value * SPRITE_WIDTH}px -${currentRow.value * SPRITE_HEIGHT}px`,
  width: `${SPRITE_WIDTH}px`,
  height: `${SPRITE_HEIGHT}px`,
  backgroundSize: `${SPRITE_WIDTH * COLS}px ${SPRITE_HEIGHT * ROWS}px`
}))

function animate(animationType) {
  const anim = ANIMATIONS[animationType]
  if (!anim) return

  currentRow.value = anim.row
  
  if (animationInterval) {
    clearInterval(animationInterval)
  }

  animationInterval = setInterval(() => {
    currentFrame.value = (currentFrame.value + 1) % anim.frames
  }, anim.speed)
}

function stopAnimation() {
  if (animationInterval) {
    clearInterval(animationInterval)
    animationInterval = null
  }
  currentFrame.value = 0
}

watch(() => props.isTalking, (talking) => {
  if (talking) {
    animate('talking')
  } else if (props.isListening) {
    animate('listening')
  } else {
    animate('idle')
  }
})

watch(() => props.isListening, (listening) => {
  if (listening && !props.isTalking) {
    animate('listening')
  } else if (!props.isTalking) {
    animate('idle')
  }
})

onMounted(() => {
  animate('idle')
})

onUnmounted(() => {
  stopAnimation()
})
</script>

<style scoped>
.robot-sprite-container {
  width: 340px;
  height: 320px;
  margin: 0 auto 30px;
  display: flex;
  justify-content: center;
  align-items: center;
  filter: drop-shadow(0 20px 40px rgba(0, 0, 0, 0.3));
}

.robot-sprite {
  background-repeat: no-repeat;
  image-rendering: -webkit-optimize-contrast;
  image-rendering: crisp-edges;
  transform-origin: center;
  transition: transform 0.3s ease;
}

.robot-sprite.talking {
  animation: bounce 0.5s ease-in-out infinite;
}

.robot-sprite.listening {
  animation: pulse 1.5s ease-in-out infinite;
}

@keyframes bounce {
  0%, 100% {
    transform: translateY(0) scale(1);
  }
  50% {
    transform: translateY(-10px) scale(1.02);
  }
}

@keyframes pulse {
  0%, 100% {
    transform: scale(1);
    filter: brightness(1);
  }
  50% {
    transform: scale(1.05);
    filter: brightness(1.1);
  }
}
</style>
