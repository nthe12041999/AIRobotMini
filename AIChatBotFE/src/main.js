import { createApp } from 'vue'
import App from './App.vue'
import './style.css'

// Naive UI - Import on demand để giảm bundle size
const app = createApp(App)
app.mount('#app')
