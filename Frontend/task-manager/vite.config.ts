import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

// Port 4200 matches the CORS policy configured in the backend.
// strictPort fails loudly if 4200 is taken instead of silently moving to
// another port (which would then be blocked by the backend's CORS policy).
export default defineConfig({
  plugins: [react()],
  server: { port: 4200, strictPort: true },
})
