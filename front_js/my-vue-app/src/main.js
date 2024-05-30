import { createApp } from 'vue'
import './style.css'
import App from './App.vue'
import * as ElementPlusIconsVue from '@element-plus/icons-vue'

// createApp(App).mount('#app')

// const icons = createApp(App)
// for (const [key, component] of Object.entries(ElementPlusIconsVue)) {
//   icons.component(key, component)
// }

// app = createApp(App)
// app.directive('focus', {
//     mounted(el) {
//       el.focus();
//     }
//   });
// app.mount("#app");

const app = createApp(App);

// Register all Element Plus icons globally
for (const [key, component] of Object.entries(ElementPlusIconsVue)) {
  app.component(key, component);
}

// Register custom directives
app.directive('focus', {
  mounted(el) {
    el.focus();
  }
});

// Mount the app to the DOM
app.mount('#app');