import { createApp } from 'vue'
import { createPinia } from 'pinia'

// ── Element Plus a demanda ───────────────────────────────────────────
// Los componentes de los templates los resuelve unplugin-vue-components
// (vite.config.ts). El resolver NO carga estilos de lo que se usa desde
// <script>, así que van a mano:
//   useToast.ts (ElMessage), ElMessageBox.confirm, NavBarUserMenu.vue y
//   los h(ElButton)/h(ElTag) de ProductosView.vue.
import 'element-plus/es/components/message/style/css'
import 'element-plus/es/components/message-box/style/css'
import 'element-plus/es/components/button/style/css'
import 'element-plus/es/components/tag/style/css'
import 'element-plus/es/components/dropdown/style/css'
import 'element-plus/es/components/dropdown-menu/style/css'
import 'element-plus/es/components/dropdown-item/style/css'
// Estilos de los componentes usados en templates: se importan acá para que
// queden ANTES que los estilos de la app (igual que con el CSS completo) y
// los overrides de element-plus-dark.css sin !important sigan ganando.
import 'element-plus/es/components/dialog/style/css'
import 'element-plus/es/components/form-item/style/css'
import 'element-plus/es/components/icon/style/css'
import 'element-plus/es/components/input/style/css'
import 'element-plus/es/components/select/style/css'
import 'element-plus/es/components/option/style/css'
import 'element-plus/es/components/progress/style/css'
import 'element-plus/es/components/radio-group/style/css'
import 'element-plus/es/components/radio-button/style/css'
import 'element-plus/es/components/tabs/style/css'
import 'element-plus/es/components/tab-pane/style/css'
import 'element-plus/es/components/table/style/css'
import 'element-plus/es/components/table-column/style/css'
import 'element-plus/theme-chalk/dark/css-vars.css'

import App from './App.vue'
import router from './router'

import '@/assets/styles/main.css'

const app = createApp(App)
app.use(createPinia())
app.use(router)
app.mount('#app')
