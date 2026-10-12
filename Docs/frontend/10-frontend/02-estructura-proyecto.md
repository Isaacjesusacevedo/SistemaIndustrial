# Estructura del Proyecto Frontend

## Árbol de Directorios Detallado

```
Frontend/
├── public/                    # Assets estáticos servidos directamente
│   ├── favicon.ico
│   ├── logo.svg
│   └── manifest.json         # PWA manifest (futuro)
├── src/
│   ├── assets/
│   │   ├── styles/
│   │   │   ├── base/
│   │   │   │   ├── fonts.css         # Fuentes
│   │   │   │   ├── variables.css     # Variables CSS (colores, espaciados, radios)
│   │   │   │   ├── reset.css         # Reset
│   │   │   │   ├── typography.css    # Tipografía
│   │   │   │   └── utilities.css     # Utilidades
│   │   │   ├── layouts/
│   │   │   │   ├── app-shell.css     # Shell + sidebar + responsive 640/1024
│   │   │   │   └── section.css       # .section (centrado + padding responsive)
│   │   │   ├── components/
│   │   │   │   ├── buttons.css       # .btn, variantes
│   │   │   │   ├── table.css         # Tablas + badges + media 640px
│   │   │   │   ├── cards.css         # .card, kpi-card
│   │   │   │   ├── forms.css         # Formularios
│   │   │   │   ├── modal.css         # Modal
│   │   │   │   ├── spinner.css       # Spinner
│   │   │   │   └── chart-container.css # Contenedor de gráficos
│   │   │   ├── views/
│   │   │   │   ├── dashboard.css
│   │   │   │   ├── tirado.css
│   │   │   │   ├── tachos-semana.css
│   │   │   │   └── armado.css
│   │   │   ├── element-plus-dark.css # Overrides oscuros para Element Plus
│   │   │   └── main.css              # @import de todo el CSS
│   │   └── main.css                  # (legado/duplicado en assets raíz)
│   ├── components/              # Componentes reutilizables (PascalCase)
│   │   ├── DataTable.vue
│   │   ├── FormField.vue           # Wrapper input + label + error
│   │   ├── SelectField.vue
│   │   ├── DatePickerField.vue
│   │   ├── KpiCard.vue
│   │   ├── ChartContainer.vue      # Contenedor de gráficos (sin lib instalada)
│   │   ├── Modal.vue
│   │   ├── Spinner.vue
│   │   ├── NavBarUserMenu.vue      # Menú usuario + cambio de contraseña
│   │   └── index.ts                # Export barrel (incluye un `NavBar` inexistente: pendiente corregir)
│   ├── composables/             # Lógica reactiva reutilizable (camelCase)
│   │   ├── useAuth.ts
│   │   ├── useApiFetch.ts
│   │   ├── useToast.ts
│   │   ├── useResponsive.ts        # isMobile (≤640px), isTablet (641–1024px), isDesktop
│   │   └── index.ts
│   ├── constants/
│   │   └── unidades.ts             # UNIDADES_PRODUCTO, UNIDADES_INSUMO
│   ├── router/
│   │   └── index.ts              # Rutas + título dinámico (guard comentado en modo demo)
│   ├── services/
│   │   └── api/                  # Servicios API por dominio
│   │       ├── authApi.ts
│   │       ├── semanasApi.ts
│   │       ├── insumosApi.ts
│   │       ├── stockApi.ts
│   │       ├── tiradoProductosApi.ts
│   │       ├── productosCatalogoApi.ts
│   │       ├── categoriasApi.ts
│   │       ├── registroTachoApi.ts
│   │       ├── dashboardApi.ts
│   │       └── index.ts          # Export barrel (no exporta registroTachoApi ni categoriasApi)
│   ├── stores/                  # Pinia stores (camelCase)
│   │   ├── auth.ts               # authStore: user, token, login, logout
│   │   ├── SemanaActiva.ts       # semana activa (selección + persistencia)
│   │   ├── semanas.ts            # semanasStore: list, current, CRUD actions
│   │   ├── insumos.ts
│   │   ├── stock.ts
│   │   ├── tiradoProductos.ts
│   │   ├── productosCatalogo.ts
│   │   ├── categorias.ts
│   │   ├── registroTacho.ts
│   │   ├── dashboard.ts
│   │   └── index.ts              # Exporta auth, semanas, insumos, stock, dashboard, tiradoProductos, productosCatalogo
│   ├── types/                   # Interfaces TypeScript (PascalCase)
│   │   ├── api.ts                # ApiResponse
│   │   ├── auth.ts               # LoginRequest, LoginResponse, UserInfo
│   │   ├── semana.ts
│   │   ├── insumo.ts
│   │   ├── stock.ts
│   │   ├── tirada.ts
│   │   ├── tiradoProducto.ts
│   │   ├── productoCatalogo.ts
│   │   ├── categoria.ts
│   │   ├── registroTacho.ts
│   │   ├── dashboard.ts
│   │   ├── common.ts
│   │   └── index.ts              # No re-exporta categoria, registroTacho, tirada ni common
│   ├── utils/                   # Utilidades puras (sin reactividad)
│   │   ├── date.ts               # formatDate, parseDate, weekNumber
│   │   └── number.ts             # formatNumber, formatCurrency
│   ├── views/                   # Vistas por módulo (kebab-case folders)
│   │   ├── auth/
│   │   │   └── LoginView.vue
│   │   ├── dashboard/
│   │   │   └── DashboardView.vue
│   │   ├── semanas/
│   │   │   ├── SemanasView.vue
│   │   │   ├── NuevaSemanaView.vue      # Valida lunes–sábado
│   │   │   ├── EditarSemanaView.vue     # Valida lunes–sábado
│   │   │   ├── DetalleSemanaView.vue    # Grilla por día laboral (sáb–vie)
│   │   │   └── CargarInicialSemanaView.vue # Wizard de carga inicial
│   │   ├── insumos/
│   │   │   ├── InsumosView.vue
│   │   │   └── NuevoInsumoView.vue
│   │   ├── stock/
│   │   │   ├── StockView.vue
│   │   │   └── NuevoStockView.vue
│   │   ├── tirado/
│   │   │   ├── TiradoView.vue
│   │   │   └── TachosSemanaView.vue
│   │   ├── armado/
│   │   │   └── ArmadoView.vue
│   │   ├── productos/
│   │   │   ├── ProductosView.vue
│   │   │   ├── NuevoProductoView.vue
│   │   │   └── EditarProductoView.vue
│   │   ├── registrotacho/
│   │   │   ├── RegistroTachoView.vue    # Sin ruta registrada
│   │   │   └── NuevoRegistroTachoView.vue # Sin ruta registrada
│   │   └── not-found/
│   │       └── NotFoundView.vue
│   ├── App.vue                  # App-shell con sidebar propio + hamburguesa en mobile
│   └── main.ts                  # Punto de entrada (Element Plus + css-vars dark)
├── index.html                   # HTML template
├── package.json
├── package-lock.json
├── vite.config.ts               # Mínimo: plugin vue + alias @
├── tsconfig.json
├── tsconfig.node.json
├── env.d.ts
├── .gitignore
└── README.md
```

---

## Convenciones de Nombrado

| Tipo | Convención | Ejemplo |
|------|------------|---------|
| **Componentes Vue** | PascalCase | `DataTable.vue`, `KpiCard.vue` |
| **Composables** | camelCase + `use` prefix | `useAuth.ts`, `useApiFetch.ts` |
| **Stores Pinia** | camelCase + `Store` suffix | `authStore`, `semanasStore` |
| **Tipos/Interfaces** | PascalCase | `Semana`, `LoginResponse`, `ApiResponse` |
| **Vistas** | PascalCase + `View` suffix | `SemanasView.vue`, `LoginView.vue` |
| **Servicios API** | camelCase + `Api` suffix | `semanasApi.ts`, `authApi.ts` |
| **Utilidades** | camelCase | `formatDate.ts`, `validation.ts` |
| **Archivos CSS** | kebab-case | `buttons.css`, `tachos-semana.css` |
| **Clases CSS** | BEM-like | `.btn`, `.btn--primary`, `.card__header` |
| **Variables CSS** | kebab-case con prefijo | `--color-primary`, `--spacing-md` |

---

## Módulos Principales

### 1. Autenticación (`auth/`)
- **LoginView.vue**: Formulario login + validación + remember me
- **Stores**: `authStore` (token, user, login, logout, refresh)
- **Composables**: `useAuth` (headers, isAuthenticated, guards)
- **API**: `authApi.login()`, `authApi.refresh()`, `authApi.logout()`

### 2. Dashboard (`dashboard/`)
- **DashboardView.vue**: KPIs cards + gráficos tendencias + alertas
- **Stores**: `dashboardStore` (metrica, tendencias, alertas)
- **Componentes**: `KpiCard`, `ChartContainer`
- **API**: `dashboardApi.getMetricas()`, `getTendencias()`, `getAlertas()`

### 3. Semanas (`semanas/`)
- **SemanasView.vue**: Lista con DataTable + filtros + acciones
- **NuevaSemanaView.vue**: Formulario crear (valida inicio lunes, fin sábado, 6 días; auto-completa lunes/sábado)
- **EditarSemanaView.vue**: Formulario editar (misma validación lunes–sábado + aviso si no cumple)
- **DetalleSemanaView.vue**: Vista detalle + grilla por día laboral (sábado a viernes)
- **CargarInicialSemanaView.vue**: Wizard de carga inicial (stock inicial + pedido semanal por producto del catálogo)
- **Stores**: `semanasStore` (list, current, create, update, delete) + `SemanaActiva` (semana activa)
- **API**: `semanasApi.*`

### 4. Módulos Dependientes de Semana
Cada módulo (Insumos, Stock, Tirado, Armado) sigue un patrón similar:
- **Vista principal**: Lista filtrada por `semanaId` (desde route params)
- **Vista de alta**: `NuevoInsumoView`, `NuevoStockView`
- **Tirado**: `TiradoView` + `TachosSemanaView` (`/semanas/:semanaId/tirado/tachos`, ruta `tachos-semana`)
- **Store**: Datos por semana + CRUD
- **API**: Endpoints con `semanaId` en query o path

### 5. Catálogo de Productos (`productos/`, global sin semana)
- **ProductosView.vue / NuevoProductoView.vue / EditarProductoView.vue**: CRUD en `/productos`
- **Unidades centralizadas**: `src/constants/unidades.ts` (`UNIDADES_PRODUCTO`, `UNIDADES_INSUMO`)
- **Store/API**: `productosCatalogo` + `categorias`

### 6. Registro Tacho (`registrotacho/`)
- **RegistroTachoView.vue / NuevoRegistroTachoView.vue**: existen en disco pero **sin ruta registrada** en el router
- **Store/API/tipos**: `registroTacho.ts`, `registroTachoApi.ts`, `types/registroTacho.ts` existen

> **No existe** módulo de objetivos en el frontend (ni vistas, ni store, ni API, ni tipos).

---

## Patrones de Componentes

### DataTable (Genérico, Reutilizable)
```vue
<!-- DataTable.vue -->
<script setup lang="ts">
interface Column<T> {
  key: string
  label: string
  sortable?: boolean
  render?: (row: T) => VNode | string
  class?: string
}

interface Props<T> {
  items: T[]
  columns: Column<T>[]
  loading?: boolean
  emptyMessage?: string
  onRowClick?: (item: T) => void
}

defineProps<Props<any>>()
defineEmits<{ rowClick: [item: any] }>()
</script>

<template>
  <div class="table-container">
    <table class="table" v-if="items.length">
      <thead class="table-header">
        <tr>
          <th v-for="col in columns" :key="col.key" :class="col.class">
            {{ col.label }}
          </th>
        </tr>
      </thead>
      <tbody>
        <tr v-for="item in items" :key="item.id" class="table-row" @click="$emit('rowClick', item)">
          <td v-for="col in columns" :key="col.key" :class="col.class">
            <component v-if="col.render" :is="col.render" :row="item" />
            <span v-else>{{ item[col.key] }}</span>
          </td>
        </tr>
      </tbody>
    </table>
    <div v-else class="empty-state">{{ emptyMessage || 'No hay datos' }}</div>
  </div>
</template>
```

### FormField (Wrapper Consistente)
```vue
<!-- FormField.vue -->
<script setup lang="ts">
interface Props {
  label: string
  modelValue: any
  type?: 'text' | 'email' | 'password' | 'number' | 'date' | 'select'
  options?: { value: any; label: string }[]
  placeholder?: string
  error?: string
  required?: boolean
  disabled?: boolean
}

const props = defineProps<Props>()
const emit = defineEmits<{ 'update:modelValue': [value: any] }>()
</script>

<template>
  <div class="field" :class="{ 'field--error': error, 'field--required': required }">
    <label class="field__label">{{ label }}{{ required ? ' *' : '' }}</label>
    <input
      v-if="type !== 'select'"
      :type="type || 'text'"
      :value="modelValue"
      @input="emit('update:modelValue', $event.target.value)"
      :placeholder="placeholder"
      :disabled="disabled"
      class="field__input"
    >
    <select
      v-else
      :value="modelValue"
      @change="emit('update:modelValue', $event.target.value)"
      :disabled="disabled"
      class="field__input"
    >
      <option value="" disabled>{{ placeholder }}</option>
      <option v-for="opt in options" :key="opt.value" :value="opt.value">{{ opt.label }}</option>
    </select>
    <span v-if="error" class="field__error">{{ error }}</span>
  </div>
</template>
```

---

## Ruteo y Navegación

### Definición de Rutas (`router/index.ts`, archivo único)

> El router real es un único `router/index.ts` (no existen `routes.ts`, `guards.ts` ni `types.ts`). Incluye redirect de `/` → `/semanas`, título dinámico `${title} · Industrial` en `afterEach` y guard de autenticación **comentado en modo demo**. El `meta` real solo usa `title` y `requiereAuth` (sin `roles`, `layout` ni `soloInvitado`).

```typescript
import { createRouter, createWebHistory } from 'vue-router'

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    { path: '/', redirect: '/semanas' },
    { path: '/login', name: 'login', component: () => import('@/views/auth/LoginView.vue'), meta: { title: 'Iniciar Sesión' } },
    { path: '/semanas', name: 'semanas', component: () => import('@/views/semanas/SemanasView.vue'), meta: { title: 'Semanas', requiereAuth: true } },
    { path: '/semanas/nueva', name: 'nueva-semana', component: () => import('@/views/semanas/NuevaSemanaView.vue'), meta: { title: 'Nueva Semana', requiereAuth: true } },
    { path: '/semanas/:id/cargar-inicial', name: 'cargar-inicial', component: () => import('@/views/semanas/CargarInicialSemanaView.vue'), meta: { title: 'Cargar Datos Iniciales', requiereAuth: true } },
    { path: '/semanas/:id', name: 'detalle-semana', component: () => import('@/views/semanas/DetalleSemanaView.vue'), meta: { title: 'Detalle', requiereAuth: true } },
    { path: '/semanas/:id/editar', name: 'editar-semana', component: () => import('@/views/semanas/EditarSemanaView.vue'), meta: { title: 'Editar Semana', requiereAuth: true } },
    { path: '/semanas/:semanaId/insumos', name: 'insumos', component: () => import('@/views/insumos/InsumosView.vue'), meta: { title: 'Insumos', requiereAuth: true } },
    { path: '/semanas/:semanaId/insumos/nuevo', name: 'nuevo-insumo', component: () => import('@/views/insumos/NuevoInsumoView.vue'), meta: { title: 'Nuevo Insumo', requiereAuth: true } },
    { path: '/semanas/:semanaId/stock', name: 'stock', component: () => import('@/views/stock/StockView.vue'), meta: { title: 'Stock', requiereAuth: true } },
    { path: '/semanas/:semanaId/stock/nuevo', name: 'nuevo-stock', component: () => import('@/views/stock/NuevoStockView.vue'), meta: { title: 'Nuevo Stock', requiereAuth: true } },
    { path: '/semanas/:semanaId/tirado', name: 'tirado', component: () => import('@/views/tirado/TiradoView.vue'), meta: { title: 'Sector Tirado', requiereAuth: true } },
    { path: '/semanas/:semanaId/tirado/tachos', name: 'tachos-semana', component: () => import('@/views/tirado/TachosSemanaView.vue'), meta: { title: 'Tachos de la Semana', requiereAuth: true } },
    { path: '/semanas/:semanaId/armado', name: 'armado', component: () => import('@/views/armado/ArmadoView.vue'), meta: { title: 'Sector Armado', requiereAuth: true } },
    { path: '/productos', name: 'productos', component: () => import('@/views/productos/ProductosView.vue'), meta: { title: 'Productos', requiereAuth: true } },
    { path: '/productos/nuevo', name: 'nuevo-producto', component: () => import('@/views/productos/NuevoProductoView.vue'), meta: { title: 'Nuevo Producto', requiereAuth: true } },
    { path: '/productos/:id/editar', name: 'editar-producto', component: () => import('@/views/productos/EditarProductoView.vue'), meta: { title: 'Editar Producto', requiereAuth: true } },
    { path: '/dashboard', name: 'dashboard', component: () => import('@/views/dashboard/DashboardView.vue'), meta: { title: 'Dashboard', requiereAuth: true } },
    { path: '/:pathMatch(.*)*', name: 'not-found', component: () => import('@/views/not-found/NotFoundView.vue'), meta: { title: 'No encontrado' } }
  ]
})

// Guard comentado en modo demo (ver router/index.ts)
router.afterEach((to) => {
  document.title = `${(to.meta.title as string) ?? 'Sistema'} · Industrial`
})

export default router
```

---

## Estado Global (Pinia)

### authStore (`stores/auth.ts`)
```typescript
export const useAuthStore = defineStore('auth', () => {
  const token = ref<string | null>(localStorage.getItem('auth_token'))
  const refreshToken = ref<string | null>(localStorage.getItem('auth_refresh_token'))
  const user = ref<UserInfo | null>(() => {
    const raw = localStorage.getItem('auth_user')
    return raw ? JSON.parse(raw) : null
  })
  
  const isAuthenticated = computed(() => !!token.value)
  
  async function login(credentials: LoginRequest) {
    const response = await authApi.login(credentials)
    token.value = response.token
    refreshToken.value = response.refreshToken
    user.value = response.user
    persistSession()
    return response
  }
  
  async function refresh() {
    if (!refreshToken.value) throw new Error('No refresh token')
    const response = await authApi.refresh(refreshToken.value)
    token.value = response.token
    refreshToken.value = response.refreshToken
    user.value = response.user
    persistSession()
  }
  
  function logout() {
    token.value = null
    refreshToken.value = null
    user.value = null
    localStorage.removeItem('auth_token')
    localStorage.removeItem('auth_refresh_token')
    localStorage.removeItem('auth_user')
  }
  
  function persistSession() {
    localStorage.setItem('auth_token', token.value!)
    localStorage.setItem('auth_refresh_token', refreshToken.value!)
    localStorage.setItem('auth_user', JSON.stringify(user.value))
  }
  
  async function initAuth() {
    // Nota: el store real no tiene initAuth; la sesión se rehidrata desde localStorage.
    // Validar token actual o refresh (propuesta)
  }
  
  return { token, refreshToken, user, isAuthenticated, login, refresh, logout, initAuth }
})
```

---

## Build y Configuración

### vite.config.ts (real, mínimo)
```typescript
import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'
import { fileURLToPath, URL } from 'node:url'

export default defineConfig({
  plugins: [vue()],
  resolve: {
    alias: {
      '@': fileURLToPath(new URL('./src', import.meta.url))
    }
  }
})
```

> No hay proxy `/api`, ni `manualChunks`, ni aliases `@components/@stores/...` en el código real. No existen archivos `.env` en el repo.

### tsconfig.json
```json
{
  "compilerOptions": {
    "target": "ES2022",
    "useDefineForClassFields": true,
    "module": "ESNext",
    "lib": ["ES2022", "DOM", "DOM.Iterable"],
    "moduleResolution": "bundler",
    "strict": true,
    "jsx": "preserve",
    "resolveJsonModule": true,
    "isolatedModules": true,
    "noEmit": true,
    "baseUrl": ".",
    "paths": {
      "@/*": ["src/*"]
    },
    "typeRoots": ["./node_modules/@types", "./src/types"]
  },
  "include": ["src/**/*.ts", "src/**/*.d.ts", "src/**/*.tsx", "src/**/*.vue"],
  "references": [{ "path": "./tsconfig.node.json" }]
}
```

### .env (no existen en el repo)
```bash
# No hay archivos .env en el código real.
# La URL del backend se resuelve en el cliente HTTP (ver 06-integracion-api.md).
```