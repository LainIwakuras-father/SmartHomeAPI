import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'

// https://vite.dev/config/
export default defineConfig({
  plugins: [vue()],
  resolve: {
    alias: {
      '@': '/src'
    }
  },
  // server: {
  //   port: 3001,
  //   proxy: {
  //     '/api': {
  //       target: 'http://localhost:5033/api',
  //       changeOrigin: true,
  //       secure: false,
  //       // rewrite: (path) => path // Не переписываем путь
  //     }
  //   }
  // }
})
