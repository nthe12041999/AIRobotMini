<template>
  <div ref="containerRef" class="robot-3d-container"></div>
</template>

<script setup>
import { ref, onMounted, onUnmounted, watch } from 'vue'
import * as THREE from 'three'
import { GLTFLoader } from 'three/examples/jsm/loaders/GLTFLoader.js'
import { OrbitControls } from 'three/examples/jsm/controls/OrbitControls.js'

const props = defineProps({
  isTalking: Boolean,
  isListening: Boolean
})

const containerRef = ref(null)
let scene, camera, renderer, controls, robot, mixer, clock
let animationId

onMounted(() => {
  initThree()
  loadRobot()
  animate()
})

onUnmounted(() => {
  if (animationId) cancelAnimationFrame(animationId)
  if (renderer) {
    renderer.dispose()
    containerRef.value?.removeChild(renderer.domElement)
  }
})

watch(() => props.isTalking, (talking) => {
  if (robot) {
    // Animate robot when talking
    if (talking) {
      robot.rotation.y += 0.01
    }
  }
})

function initThree() {
  // Scene
  scene = new THREE.Scene()
  scene.background = null

  // Camera
  camera = new THREE.PerspectiveCamera(
    45,
    300 / 400,
    0.1,
    1000
  )
  camera.position.set(0, 1, 3)

  // Renderer
  renderer = new THREE.WebGLRenderer({ 
    antialias: true, 
    alpha: true 
  })
  renderer.setSize(300, 400)
  renderer.setPixelRatio(window.devicePixelRatio)
  renderer.shadowMap.enabled = true
  containerRef.value.appendChild(renderer.domElement)

  // Lights
  const ambientLight = new THREE.AmbientLight(0xffffff, 0.6)
  scene.add(ambientLight)

  const directionalLight = new THREE.DirectionalLight(0xffffff, 0.8)
  directionalLight.position.set(5, 5, 5)
  directionalLight.castShadow = true
  scene.add(directionalLight)

  const blueLight = new THREE.PointLight(0x3b82f6, 1, 10)
  blueLight.position.set(0, 2, 2)
  scene.add(blueLight)

  // Controls
  controls = new OrbitControls(camera, renderer.domElement)
  controls.enableDamping = true
  controls.dampingFactor = 0.05
  controls.enableZoom = false
  controls.autoRotate = true
  controls.autoRotateSpeed = 1

  // Clock for animations
  clock = new THREE.Clock()
}

function loadRobot() {
  const loader = new GLTFLoader()
  
  loader.load(
    '/models/robot.glb',
    (gltf) => {
      robot = gltf.scene
      
      // Scale and position
      robot.scale.set(1, 1, 1)
      robot.position.set(0, -1, 0)
      
      // Enable shadows
      robot.traverse((child) => {
        if (child.isMesh) {
          child.castShadow = true
          child.receiveShadow = true
        }
      })
      
      scene.add(robot)
      
      // Setup animations if available
      if (gltf.animations && gltf.animations.length) {
        mixer = new THREE.AnimationMixer(robot)
        gltf.animations.forEach((clip) => {
          mixer.clipAction(clip).play()
        })
      }
    },
    (progress) => {
      console.log('Loading:', (progress.loaded / progress.total * 100) + '%')
    },
    (error) => {
      console.error('Error loading model:', error)
    }
  )
}

function animate() {
  animationId = requestAnimationFrame(animate)
  
  const delta = clock.getDelta()
  
  if (mixer) mixer.update(delta)
  if (controls) controls.update()
  
  // Talking animation
  if (robot && props.isTalking) {
    robot.rotation.y = Math.sin(Date.now() * 0.003) * 0.1
  }
  
  renderer.render(scene, camera)
}
</script>

<style scoped>
.robot-3d-container {
  width: 300px;
  height: 400px;
  margin: 0 auto;
  position: relative;
}

.robot-3d-container canvas {
  display: block;
  border-radius: 20px;
}
</style>
