﻿import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

export default defineConfig({
  plugins: [
    react(
      { jsxRuntime: 'classic' }
    ),
  ],
  base: "/",
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
