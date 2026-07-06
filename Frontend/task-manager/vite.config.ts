import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

// Port 4200 matches the CORS policy configured in the backend.
export default defineConfig({
  plugins: [react()],
  server: { port: 4200 },
})
