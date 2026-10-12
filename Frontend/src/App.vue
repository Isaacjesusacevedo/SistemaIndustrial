<script setup lang="ts">
import { RouterView, useRoute } from 'vue-router'
import { computed, onMounted, ref, watch } from 'vue'
import { useSemanaActivaStore } from '@/stores/SemanaActiva'
import { useResponsive } from '@/composables/useResponsive'
import { Plus, Box, Goods, TrendCharts, DataAnalysis, Menu } from '@element-plus/icons-vue'

const route = useRoute()
const semanaActiva = useSemanaActivaStore()
const { isMobile } = useResponsive()

// Sidebar en mobile: oculto por defecto, se abre con hamburguesa
const sidebarOpen = ref(false)

// Cerrar sidebar al navegar en mobile
watch(() => route.name, () => {
  sidebarOpen.value = false
})

// Rutas que NO deben mostrar el sidebar
const rutasPublicas = ['login', 'not-found']
const mostrarSidebar = computed(() => !rutasPublicas.includes(route.name as string))

// Módulos de la semana activa
const modulosSemana = computed(() => {
  if (!semanaActiva.tieneSemanaActiva) return []
  const semanaId = semanaActiva.semanaId
  return [
    { name: 'insumos',       label: 'Insumos',        icon: Box,         params: { semanaId } },
    { name: 'stock',         label: 'Stock',          icon: Goods,     params: { semanaId } },
    { name: 'tirado',        label: 'Tirado',         icon: TrendCharts, params: { semanaId } },
    { name: 'armado',        label: 'Armado',         icon: DataAnalysis,params: { semanaId } },
  ]
})

// ¿Está activo el link?
const isActive = (name: string) => route.name === name

onMounted(() => {
  semanaActiva.inicializar()
})
</script>

<template>
  <div class="app-shell">
    <!-- Botón hamburguesa (solo mobile) -->
    <button
      v-if="mostrarSidebar && isMobile"
      @click="sidebarOpen = !sidebarOpen"
      class="hamburger-btn"
      aria-label="Abrir menú">
      <Menu style="width:20px; height:20px;" />
    </button>

    <!-- Overlay para cerrar el sidebar en mobile -->
    <div
      v-if="mostrarSidebar && isMobile && sidebarOpen"
      class="app-nav__overlay"
      @click="sidebarOpen = false">
    </div>

    <!-- Sidebar (oculto en login y 404) -->
    <nav v-if="mostrarSidebar" class="app-nav" :class="{ open: sidebarOpen }">
      <div class="nav-brand">
        <div class="nav-brand-logo"><span>SI</span></div>
        <div class="nav-brand-name">Industrial</div>
        <div class="nav-brand-sub">SISTEMA DE GESTIÓN</div>
      </div>

      <!-- Sección principal -->
      <div class="nav-section">
        <div class="nav-section-label">Producción</div>

        <RouterLink to="/dashboard" class="nav-link" :class="{ active: isActive('dashboard') }">
          <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
            <rect x="3" y="3" width="7" height="7"/><rect x="14" y="3" width="7" height="7"/>
            <rect x="14" y="14" width="7" height="7"/><rect x="3" y="14" width="7" height="7"/>
          </svg>
          Dashboard
        </RouterLink>

        <RouterLink to="/semanas" class="nav-link" :class="{ active: isActive('semanas') }">
          <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
            <path d="M8 2v4M16 2v4M3 10h18M5 4h14a2 2 0 0 1 2 2v14a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2V6a2 2 0 0 1 2-2z"/>
          </svg>
          Semanas
        </RouterLink>

        <RouterLink to="/semanas/nueva" class="nav-link" :class="{ active: isActive('nueva-semana') }">
          <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
            <circle cx="12" cy="12" r="10"/><path d="M12 8v8M8 12h8"/>
          </svg>
          Nueva Semana
        </RouterLink>

        <RouterLink to="/productos" class="nav-link" :class="{ active: ['productos', 'nuevo-producto', 'editar-producto'].includes(route.name as string) }">
          <Goods class="nav-link-icon" />
          Productos
        </RouterLink>
      </div>

      <!-- Sección: Semana activa -->
      <div v-if="semanaActiva.tieneSemanaActiva" class="nav-section">
        <div class="nav-section-label">
          Semana activa
        </div>
        <div class="nav-semana-card">
          <div class="nav-semana-title">
            Semana {{ semanaActiva.semana?.numeroSemana }}/{{ semanaActiva.semana?.anio }}
          </div>
          <div class="nav-semana-estado" :class="`estado--${(semanaActiva.semana?.estado || '').toLowerCase()}`">
            {{ semanaActiva.semana?.estado }}
          </div>
        </div>

        <RouterLink
          v-for="mod in modulosSemana"
          :key="mod.name"
          :to="{ name: mod.name, params: mod.params }"
          class="nav-link"
          :class="{ active: isActive(mod.name) }">
          <component :is="mod.icon" class="nav-link-icon" />
          {{ mod.label }}
        </RouterLink>
      </div>

      <!-- Sin semana activa -->
      <div v-else class="nav-section">
        <div class="nav-section-label">Semana activa</div>
        <div class="nav-empty">
          <div class="nav-empty-text">No hay semana activa</div>
          <RouterLink to="/semanas" class="nav-empty-link">Seleccionar →</RouterLink>
        </div>
      </div>

      <div class="nav-footer">
        <div class="nav-status">
          <div class="nav-status-dot"></div>
          <span class="nav-status-text">API conectada</span>
        </div>
      </div>
    </nav>

    <!-- Main content -->
    <main class="app-main" :class="{ 'app-main--full': !mostrarSidebar }">
      <RouterView />
    </main>
  </div>
</template>

<style scoped src="@/assets/styles/layouts/app-shell.css"></style>