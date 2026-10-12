# Enrutamiento

## Visión General

El enrutamiento usa **Vue Router 4** con:
- **Lazy loading** para todas las rutas (code splitting)
- **Archivo único** `router/index.ts` (no hay `routes.ts`, `guards.ts` ni `types.ts`)
- **Guard de autenticación comentado** (modo demo, acceso libre)
- **Redirect** de `/` → `/semanas`
- **Meta mínimo**: solo `title` y `requiereAuth` (sin `roles`, `layout` ni `soloInvitado`)
- **Título dinámico** `${title} · Industrial` en `afterEach`

---

## Configuración Principal (`router/index.ts`)

```typescript
import { createRouter, createWebHistory } from 'vue-router'
// import { useAuthStore } from '@/stores/auth'  // MODO DEMO: comentado para acceso libre

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    { path: '/', redirect: '/semanas' },
    // ... (ver listado completo abajo)
  ]
})

// ── Guard de autenticación ──────────────────────────────────────────
// MODO DEMO: guard comentado para permitir acceso libre
// (al reactivarlo: si requiereAuth y no hay sesión → login con ?redirect=,
//  si ya hay sesión y va a login → dashboard)

// ── Título dinámico ─────────────────────────────────────────────────
router.afterEach((to) => {
  const title = (to.meta.title as string) ?? 'Sistema'
  document.title = `${title} · Industrial`
})

export default router
```

---

## Definición de Rutas (`router/index.ts`)

```typescript
import type { RouteRecordRaw } from 'vue-router'

export const routes: RouteRecordRaw[] = [
  // === REDIRECT RAÍZ ===
  { path: '/', redirect: '/semanas' },

  // === AUTENTICACIÓN ===
  {
    path: '/login',
    name: 'login',
    component: () => import('@/views/auth/LoginView.vue'),
    meta: { title: 'Iniciar Sesión' }
  },

  // === SEMANAS ===
  {
    path: '/semanas',
    name: 'semanas',
    component: () => import('@/views/semanas/SemanasView.vue'),
    meta: { title: 'Semanas', requiereAuth: true }
  },
  {
    path: '/semanas/nueva',
    name: 'nueva-semana',
    component: () => import('@/views/semanas/NuevaSemanaView.vue'),
    meta: { title: 'Nueva Semana', requiereAuth: true }
  },
  {
    path: '/semanas/:id/cargar-inicial',
    name: 'cargar-inicial',
    component: () => import('@/views/semanas/CargarInicialSemanaView.vue'),
    meta: { title: 'Cargar Datos Iniciales', requiereAuth: true }
  },
  {
    path: '/semanas/:id',
    name: 'detalle-semana',
    component: () => import('@/views/semanas/DetalleSemanaView.vue'),
    meta: { title: 'Detalle', requiereAuth: true }
  },
  {
    path: '/semanas/:id/editar',
    name: 'editar-semana',
    component: () => import('@/views/semanas/EditarSemanaView.vue'),
    meta: { title: 'Editar Semana', requiereAuth: true }
  },

  // === MÓDULOS DEPENDIENTES DE SEMANA ===

  {
    path: '/semanas/:semanaId/insumos',
    name: 'insumos',
    component: () => import('@/views/insumos/InsumosView.vue'),
    meta: { title: 'Insumos', requiereAuth: true }
  },
  {
    path: '/semanas/:semanaId/insumos/nuevo',
    name: 'nuevo-insumo',
    component: () => import('@/views/insumos/NuevoInsumoView.vue'),
    meta: { title: 'Nuevo Insumo', requiereAuth: true }
  },
  {
    path: '/semanas/:semanaId/stock',
    name: 'stock',
    component: () => import('@/views/stock/StockView.vue'),
    meta: { title: 'Stock', requiereAuth: true }
  },
  {
    path: '/semanas/:semanaId/stock/nuevo',
    name: 'nuevo-stock',
    component: () => import('@/views/stock/NuevoStockView.vue'),
    meta: { title: 'Nuevo Stock', requiereAuth: true }
  },
  {
    path: '/semanas/:semanaId/tirado',
    name: 'tirado',
    component: () => import('@/views/tirado/TiradoView.vue'),
    meta: { title: 'Sector Tirado', requiereAuth: true }
  },
  {
    path: '/semanas/:semanaId/tirado/tachos',
    name: 'tachos-semana',
    component: () => import('@/views/tirado/TachosSemanaView.vue'),
    meta: { title: 'Tachos de la Semana', requiereAuth: true }
  },
  {
    path: '/semanas/:semanaId/armado',
    name: 'armado',
    component: () => import('@/views/armado/ArmadoView.vue'),
    meta: { title: 'Sector Armado', requiereAuth: true }
  },

  // === CATÁLOGO DE PRODUCTOS (global, sin semana) ===
  {
    path: '/productos',
    name: 'productos',
    component: () => import('@/views/productos/ProductosView.vue'),
    meta: { title: 'Productos', requiereAuth: true }
  },
  {
    path: '/productos/nuevo',
    name: 'nuevo-producto',
    component: () => import('@/views/productos/NuevoProductoView.vue'),
    meta: { title: 'Nuevo Producto', requiereAuth: true }
  },
  {
    path: '/productos/:id/editar',
    name: 'editar-producto',
    component: () => import('@/views/productos/EditarProductoView.vue'),
    meta: { title: 'Editar Producto', requiereAuth: true }
  },

  // === DASHBOARD (en /dashboard, no en /) ===
  {
    path: '/dashboard',
    name: 'dashboard',
    component: () => import('@/views/dashboard/DashboardView.vue'),
    meta: { title: 'Dashboard', requiereAuth: true }
  },

  // === 404 ===
  {
    path: '/:pathMatch(.*)*',
    name: 'not-found',
    component: () => import('@/views/not-found/NotFoundView.vue'),
    meta: { title: 'No encontrado' }
  }
]
```

> **Rutas que NO existen**: no hay `/semanas/:semanaId/objetivos` (sin módulo de objetivos) ni `/semanas/:semanaId/registrotacho` (las vistas de `registrotacho/` no están registradas). El nombre de la ruta home no es `home` sino `dashboard`, y vive en `/dashboard`.

---

## Tipos de Meta (reales)

No existe `router/types.ts`. El `meta` real solo usa:

```typescript
interface RouteMeta {
  /** Título de la página para document.title (`${title} · Industrial`) */
  title?: string
  /** Requiere usuario autenticado (guard actualmente comentado en modo demo) */
  requiereAuth?: boolean
}
```

---

## Guards Globales (modo demo: comentados)

No existe `router/guards.ts` ni `setupGuards`. El guard está comentado en `router/index.ts` para permitir acceso libre:

```typescript
// MODO DEMO: guard de auth comentado para permitir acceso libre
// router.beforeEach((to) => {
//   const authStore = useAuthStore()
//
//   if (to.meta.requiereAuth && !authStore.isAuthenticated) {
//     return { name: 'login', query: { redirect: to.fullPath } }
//   }
//
//   if (to.name === 'login' && authStore.isAuthenticated) {
//     return { name: 'dashboard' }
//   }
// })
```

> Al salir del modo demo, descomentar este guard. No hay validación de roles ni `useToast` en el router real.

---

## Navegación Programática

### En Vistas (Composition API)
```typescript
// En NuevaSemanaView.vue - tras crear exitosamente
const router = useRouter()
const store = useSemanasStore()

const crearSemana = async () => {
  try {
    const nueva = await store.create(formData)
    toast.success('Semana creada correctamente')
    router.push({ name: 'detalle-semana', params: { id: nueva.id } })
  } catch (error) {
    toast.error('Error al crear semana')
  }
}
```

### En Composables
```typescript
// composables/useNavigation.ts
import { useRouter } from 'vue-router'

export function useNavigation() {
  const router = useRouter()
  
  const goToSemanaDetalle = (id: number) => 
    router.push({ name: 'detalle-semana', params: { id } })
  
  const goToSemanaEditar = (id: number) => 
    router.push({ name: 'editar-semana', params: { id } })
  
  const goToInsumos = (semanaId: number) =>
    router.push({ name: 'insumos', params: { semanaId } })

  const goToTachos = (semanaId: number) =>
    router.push({ name: 'tachos-semana', params: { semanaId } })

  const goToCargaInicial = (id: number) =>
    router.push({ name: 'cargar-inicial', params: { id } })

  const goBack = () => router.back()

  const goHome = () => router.push({ name: 'semanas' })

  const goLogin = (redirect?: string) =>
    router.push({ name: 'login', query: { redirect } })

  return { goToSemanaDetalle, goToSemanaEditar, goToInsumos, goToTachos, goToCargaInicial, goBack, goHome, goLogin }
}
```

### En Templates
```vue
<!-- Botones de navegación -->
<router-link :to="{ name: 'nueva-semana' }" class="btn btn-primary">
  <PlusIcon /> Nueva Semana
</router-link>

<router-link 
  v-for="semana in semanas" 
  :key="semana.id" 
  :to="{ name: 'detalle-semana', params: { id: semana.id } }"
  class="table-row-link"
>
  {{ semana.numeroSemana }}/{{ semana.anio }}
</router-link>

<!-- Botón volver con fallback -->
<button @click="goBack" class="btn btn-secondary">
  <ArrowLeftIcon /> Volver
</button>
```

---

## Parámetros y Query Strings

### Props en Rutas (estado real)

Las rutas reales **no** usan `props: true`; las vistas leen los parámetros con `useRoute()`:

```typescript
// DetalleSemanaView.vue / CargarInicialSemanaView.vue (patrón real)
const route = useRoute()
const semanaId = computed(() => Number(route.params.id)) // :id
// En sub-módulos: Number(route.params.semanaId) // :semanaId
```

### Query Strings para Filtros/Paginación
```typescript
// SemanasView.vue - sincronizar URL con filtros
const router = useRouter()
const route = useRoute()

const filters = ref({
  anio: Number(route.query.anio) || new Date().getFullYear(),
  estado: route.query.estado || '',
  page: Number(route.query.page) || 1,
  pageSize: 20
})

// Actualizar URL sin recargar
const updateFilters = (newFilters: Partial<typeof filters.value>) => {
  filters.value = { ...filters.value, ...newFilters }
  router.replace({ 
    query: { ...route.query, ...filters.value } 
  })
}

// Watch para cargar datos al cambiar query
watch(() => route.query, () => {
  filters.value = {
    anio: Number(route.query.anio) || new Date().getFullYear(),
    estado: route.query.estado || '',
    page: Number(route.query.page) || 1
  }
  store.fetchAll(filters.value)
}, { deep: true })
```

---

## Layouts (Estructura de Página)

No existe carpeta `layouts/`. El layout vive en `App.vue` (app-shell con sidebar propio):

- **Sidebar** visible en todas las rutas salvo `login` y `not-found` (`mostrarSidebar`).
- **Sección Producción**: links a `dashboard` (`/dashboard`), `semanas`, `nueva-semana` y `productos`.
- **Sección Semana activa**: links a `insumos`, `stock`, `tirado` y `armado` con `params: { semanaId }` (solo si `semanaActiva.tieneSemanaActiva`).
- **Mobile**: botón hamburguesa + overlay; el sidebar se cierra al navegar (`watch(route.name)`).
- **Estilos**: `src/assets/styles/layouts/app-shell.css` importado con `<style scoped src=...>` en `App.vue`; breakpoints 640px (mobile) y 1024px (tablet con sidebar colapsado a íconos).

---

## Transiciones de Página

```css
/* src/assets/styles/base/utilities.css (o equivalente) */
.page-enter-active,
.page-leave-active {
  transition: opacity 0.2s ease, transform 0.2s ease;
}

.page-enter-from,
.page-leave-to {
  opacity: 0;
  transform: translateY(10px);
}
```

> Nota: el código real no usa transiciones de página en `App.vue` (renderiza `<RouterView />` directo); el snippet es una guía si se quieren agregar.

---

## Debugging Rutas

### Vue DevTools
- Panel "Router" muestra historial, ruta actual, params, query
- Time-travel debugging entre rutas

### Logs de Navegación
```typescript
// En guards.ts - temporal para debug
router.beforeEach((to, from, next) => {
  console.group(`🧭 Navegación: ${from.name} → ${to.name}`)
  console.log('Params:', to.params)
  console.log('Query:', to.query)
  console.log('Meta:', to.meta)
  console.groupEnd()
  next()
})
```

### Rutas Registradas
```typescript
// En main.ts - debug
import router from './router'
console.table(router.getRoutes().map(r => ({
  name: r.name,
  path: r.path,
  meta: r.meta
})))
```