import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

export default defineConfig({
  plugins: [
    react(
      { jsxRuntime: 'classic' }
    ),
  ],
  base: "/farss/",
  server: {
    watch: {
      usePolling: true,
    },
    proxy: {
      "/api": {
        target: "http://localhost:5000/",
      }
    }
  }
})
