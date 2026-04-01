import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'

// https://vite.dev/config/
export default defineConfig({
  plugins: [vue()],
  server: {
    proxy: {
      // Proxy API calls to the app service
      '/api': {
        target: process.env.API_HTTPS || process.env.API_HTTP,
        changeOrigin: true
      }
    }
  }
})
