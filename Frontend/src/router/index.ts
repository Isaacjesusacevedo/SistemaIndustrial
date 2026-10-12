import { createRouter, createWebHistory } from 'vue-router'
// import { useAuthStore } from '@/stores/auth'  // MODO DEMO: comentado para acceso libre

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    { path: '/', redirect: '/semanas' },

    // ── Auth ────────────────────────────────────────────────────────
    {
      path: '/login',
      name: 'login',
      component: () => import('@/views/auth/LoginView.vue'),
      meta: { title: 'Iniciar Sesión' }
    },

    // ── Semanas ─────────────────────────────────────────────────────
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

    // ── Sub-módulos de Semana ───────────────────────────────────────
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
    // ── Tirado / Armado ─────────────────────────────────────────────
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

    // ── Catálogo de Productos (global, sin semana) ────────────────────
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

    // ── Dashboard ───────────────────────────────────────────────────
    {
      path: '/dashboard',
      name: 'dashboard',
      component: () => import('@/views/dashboard/DashboardView.vue'),
      meta: { title: 'Dashboard', requiereAuth: true }
    },

    // ── 404 ─────────────────────────────────────────────────────────
    {
      path: '/:pathMatch(.*)*',
      name: 'not-found',
      component: () => import('@/views/not-found/NotFoundView.vue'),
      meta: { title: 'No encontrado' }
    }
  ]
})

// ── Guard de autenticación ──────────────────────────────────────────
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

// ── Título dinámico ─────────────────────────────────────────────────
router.afterEach((to) => {
  const title = (to.meta.title as string) ?? 'Sistema'
  document.title = `${title} · Industrial`
})

export default router