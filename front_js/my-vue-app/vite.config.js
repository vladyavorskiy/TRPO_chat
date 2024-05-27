import vue from '@vitejs/plugin-vue'
import { defineConfig } from 'vite'

const backend = "https://localhost:7008";

const new_backend = "http://backend";
// const backend = "https://localhost:2080";

export default defineConfig(({ command, mode, isSsrBuild, isPreview }) =>{
  return{
 plugins: [vue()],
 server: {
   host: "0.0.0.0",
   proxy: {
     "^/api": {
       target: mode == "development" ? backend : new_backend,
       ws: false,
       secure: false,
     },
     "^/hub": {
       target: mode == "development" ? backend : new_backend,
       ws: true,
       secure: false,
     },
   },
 },
}
});
