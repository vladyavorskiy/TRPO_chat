import vue from '@vitejs/plugin-vue'
import { defineConfig } from 'vite'

const backend = "https://localhost:7008";

export default defineConfig({
  plugins: [vue()],
  server: {
    proxy: {
      "^/api": {
        target: backend,
        ws: false,
        secure: false,
      },
      "^/hub": {
        target: backend,
        ws: true,
        secure: false,
      },
    },
  },
});