import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

// https://vite.dev/config/
export default defineConfig({
  plugins: [react()],
  server: {
    port: 5173,
    proxy: {
      '/identity': {
        target: 'http://localhost:5227',
        changeOrigin: true,
        rewrite: (path) => path.replace(/^\/identity/, ''),
      },
      '/bff': {
        target: 'http://localhost:5265',
        changeOrigin: true,
        rewrite: (path) => path.replace(/^\/bff/, ''),
      },
      '/vendas': {
        target: 'http://localhost:5165',
        changeOrigin: true,
        rewrite: (path) => path.replace(/^\/vendas/, ''),
      },
    },
  },
})
