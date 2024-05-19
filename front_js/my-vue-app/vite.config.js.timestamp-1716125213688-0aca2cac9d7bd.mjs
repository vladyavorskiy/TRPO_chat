// vite.config.js
import vue from "file:///C:/Users/Vladislav%20Yavorskiy/Desktop/%D0%A2%D0%A0%D0%9F%D0%9E/front_js/my-vue-app/node_modules/@vitejs/plugin-vue/dist/index.mjs";
import { defineConfig } from "file:///C:/Users/Vladislav%20Yavorskiy/Desktop/%D0%A2%D0%A0%D0%9F%D0%9E/front_js/my-vue-app/node_modules/vite/dist/node/index.js";
var backend = "https://localhost:7008";
var vite_config_default = defineConfig({
  plugins: [vue()],
  server: {
    proxy: {
      "^/api": {
        target: backend,
        ws: false,
        secure: false
      },
      "^/hub": {
        target: backend,
        ws: true,
        secure: false
      }
    }
  }
});
export {
  vite_config_default as default
};
//# sourceMappingURL=data:application/json;base64,ewogICJ2ZXJzaW9uIjogMywKICAic291cmNlcyI6IFsidml0ZS5jb25maWcuanMiXSwKICAic291cmNlc0NvbnRlbnQiOiBbImNvbnN0IF9fdml0ZV9pbmplY3RlZF9vcmlnaW5hbF9kaXJuYW1lID0gXCJDOlxcXFxVc2Vyc1xcXFxWbGFkaXNsYXYgWWF2b3Jza2l5XFxcXERlc2t0b3BcXFxcXHUwNDIyXHUwNDIwXHUwNDFGXHUwNDFFXFxcXGZyb250X2pzXFxcXG15LXZ1ZS1hcHBcIjtjb25zdCBfX3ZpdGVfaW5qZWN0ZWRfb3JpZ2luYWxfZmlsZW5hbWUgPSBcIkM6XFxcXFVzZXJzXFxcXFZsYWRpc2xhdiBZYXZvcnNraXlcXFxcRGVza3RvcFxcXFxcdTA0MjJcdTA0MjBcdTA0MUZcdTA0MUVcXFxcZnJvbnRfanNcXFxcbXktdnVlLWFwcFxcXFx2aXRlLmNvbmZpZy5qc1wiO2NvbnN0IF9fdml0ZV9pbmplY3RlZF9vcmlnaW5hbF9pbXBvcnRfbWV0YV91cmwgPSBcImZpbGU6Ly8vQzovVXNlcnMvVmxhZGlzbGF2JTIwWWF2b3Jza2l5L0Rlc2t0b3AvJUQwJUEyJUQwJUEwJUQwJTlGJUQwJTlFL2Zyb250X2pzL215LXZ1ZS1hcHAvdml0ZS5jb25maWcuanNcIjtpbXBvcnQgdnVlIGZyb20gJ0B2aXRlanMvcGx1Z2luLXZ1ZSdcbmltcG9ydCB7IGRlZmluZUNvbmZpZyB9IGZyb20gJ3ZpdGUnXG5cbmNvbnN0IGJhY2tlbmQgPSBcImh0dHBzOi8vbG9jYWxob3N0OjcwMDhcIjtcblxuZXhwb3J0IGRlZmF1bHQgZGVmaW5lQ29uZmlnKHtcbiAgcGx1Z2luczogW3Z1ZSgpXSxcbiAgc2VydmVyOiB7XG4gICAgcHJveHk6IHtcbiAgICAgIFwiXi9hcGlcIjoge1xuICAgICAgICB0YXJnZXQ6IGJhY2tlbmQsXG4gICAgICAgIHdzOiBmYWxzZSxcbiAgICAgICAgc2VjdXJlOiBmYWxzZSxcbiAgICAgIH0sXG4gICAgICBcIl4vaHViXCI6IHtcbiAgICAgICAgdGFyZ2V0OiBiYWNrZW5kLFxuICAgICAgICB3czogdHJ1ZSxcbiAgICAgICAgc2VjdXJlOiBmYWxzZSxcbiAgICAgIH0sXG4gICAgfSxcbiAgfSxcbn0pOyJdLAogICJtYXBwaW5ncyI6ICI7QUFBNlksT0FBTyxTQUFTO0FBQzdaLFNBQVMsb0JBQW9CO0FBRTdCLElBQU0sVUFBVTtBQUVoQixJQUFPLHNCQUFRLGFBQWE7QUFBQSxFQUMxQixTQUFTLENBQUMsSUFBSSxDQUFDO0FBQUEsRUFDZixRQUFRO0FBQUEsSUFDTixPQUFPO0FBQUEsTUFDTCxTQUFTO0FBQUEsUUFDUCxRQUFRO0FBQUEsUUFDUixJQUFJO0FBQUEsUUFDSixRQUFRO0FBQUEsTUFDVjtBQUFBLE1BQ0EsU0FBUztBQUFBLFFBQ1AsUUFBUTtBQUFBLFFBQ1IsSUFBSTtBQUFBLFFBQ0osUUFBUTtBQUFBLE1BQ1Y7QUFBQSxJQUNGO0FBQUEsRUFDRjtBQUNGLENBQUM7IiwKICAibmFtZXMiOiBbXQp9Cg==
