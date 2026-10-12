<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useSemanaActivaStore } from '@/stores/SemanaActiva'
import { useSemanasStore } from '@/stores/semanas'
import { useInsumosStore } from '@/stores/insumos'
import { useStockStore } from '@/stores/stock'
import { useTiradoProductosStore } from '@/stores/tiradoProductos'
import { useAuthStore } from '@/stores/auth'
import { useToast } from '@/composables/useToast'
import { Plus, Box, Goods, TrendCharts, DataAnalysis } from '@element-plus/icons-vue'

const router = useRouter()
const semanaActiva = useSemanaActivaStore()
const semanasStore = useSemanasStore()
const insumosStore = useInsumosStore()
const stockStore = useStockStore()
const tiradoStore = useTiradoProductosStore()
const authStore = useAuthStore()
const toast = useToast()

const loading = ref(false)

const semana = computed(() => semanaActiva.semana)
const semanaId = computed(() => semanaActiva.semanaId)

// ─── Cálculo de progreso ──────────────────────────────────────────
interface Modulo {
  key: string
  label: string
  icon: any
  cargado: boolean
  count: number
  mensaje: string
  ruta: any
  color: string
  boton: string
}

const modulos = computed<Modulo[]>(() => {
  if (!semanaId.value) return []
  const sid = semanaId.value

  // 1. Insumos
  const insCount = insumosStore.items.length
  const insOk = insCount > 0

  // 2. Stock
  const stkCount = stockStore.items.length
  const stkOk = stkCount > 0

  // 3. Tirado
  const tirCount = tiradoStore.items.length
  const tirOk = tirCount > 0 && tiradoStore.items.every(p => p.cantidadTirada > 0)

  return [
    {
      key: 'insumos',
      label: 'Insumos',
      icon: Box,
      cargado: insOk,
      count: insCount,
      mensaje: insOk ? `${insCount} insumo${insCount !== 1 ? 's' : ''} cargado${insCount !== 1 ? 's' : ''}` : 'Sin insumos',
      ruta: { name: 'insumos', params: { semanaId: sid } },
      color: 'info',
      boton: insOk ? 'Ver' : 'Completar'
    },
    {
      key: 'stock',
      label: 'Stock',
      icon: Goods,
      cargado: stkOk,
      count: stkCount,
      mensaje: stkOk ? `${stkCount} producto${stkCount !== 1 ? 's' : ''} en stock` : 'Sin stock cargado',
      ruta: { name: 'stock', params: { semanaId: sid } },
      color: 'success',
      boton: stkOk ? 'Ver' : 'Completar'
    },
    {
      key: 'tirado',
      label: 'Tirado',
      icon: TrendCharts,
      cargado: tirOk,
      count: tirCount,
      mensaje: tirOk ? 'Todos con tirada' : `${tiradoStore.items.filter(p => p.cantidadTirada > 0).length}/${tirCount} con tirada`,
      ruta: { name: 'tirado', params: { semanaId: sid } },
      color: 'danger',
      boton: tirOk ? 'Ver' : 'Completar'
    }
  ]
})

const totalProductos = computed(() => tiradoStore.items.length)

const productosConTirada = computed(() =>
  tiradoStore.items.filter(p => p.cantidadTirada > 0).length
)

const progreso = computed(() => {
  if (totalProductos.value === 0) return 0
  return Math.round((productosConTirada.value / totalProductos.value) * 100)
})

// ─── Pendientes críticos ──────────────────────────────────────────
interface Pendiente {
  tipo: 'danger' | 'warning' | 'info'
  texto: string
  ruta: any
}

const pendientes = computed<Pendiente[]>(() => {
  if (!semanaId.value) return []
  const sid = semanaId.value
  const lista: Pendiente[] = []

  // Tirado con faltantes
  const conFaltante = tiradoStore.items.filter(p => p.stockTotal < 0)
  if (conFaltante.length > 0) {
    lista.push({
      tipo: 'danger',
      texto: `${conFaltante.length} producto${conFaltante.length !== 1 ? 's' : ''} de tirado con faltante`,
      ruta: { name: 'tirado', params: { semanaId: sid } }
    })
  }

  // Tirado sin tirada
  const sinTirada = tiradoStore.items.filter(p => p.cantidadTirada === 0)
  if (sinTirada.length > 0) {
    lista.push({
      tipo: 'warning',
      texto: `${sinTirada.length} producto${sinTirada.length !== 1 ? 's' : ''} sin tirada registrada`,
      ruta: { name: 'tirado', params: { semanaId: sid } }
    })
  }

  // Stock sin cargar
  if (stockStore.items.length === 0) {
    lista.push({
      tipo: 'info',
      texto: 'No hay stock cargado para esta semana',
      ruta: { name: 'stock', params: { semanaId: sid } }
    })
  }

  return lista
})

// ─── Carga ────────────────────────────────────────────────────────
const cargarTodo = async () => {
  loading.value = true
  try {
    await semanaActiva.inicializar()

    if (!semanaId.value) {
      // Sin semana activa: recargar semanas por si el user crea una nueva
      await semanasStore.fetchAll()
      return
    }

    const sid = semanaId.value
    await Promise.all([
      insumosStore.fetchBySemana(sid),
      stockStore.fetchBySemana(sid),
      tiradoStore.cargarPorSemana(sid)
    ])
  } catch (e: any) {
    toast.error(e.message || 'Error al cargar el dashboard')
  } finally {
    loading.value = false
  }
}

// ─── Handlers ─────────────────────────────────────────────────────
const handleCambioSemana = (id: number) => {
  semanaActiva.setSemana(id)
  cargarTodo()
}

const handleNuevaSemana = () => {
  router.push({ name: 'nueva-semana' })
}

// ─── Lifecycle ────────────────────────────────────────────────────
onMounted(cargarTodo)
</script>

<template>
  <div class="section">
    <!-- Header -->
    <div class="section__header">
      <div>
        <h1>Dashboard de Producción</h1>
        <p class="section__subtitle" v-if="semana">
          Semana {{ semana.numeroSemana }}/{{ semana.anio }} · {{ semana.estado }}
        </p>
        <p class="section__subtitle" v-else>
          Sin semana activa — seleccioná una para empezar
        </p>
      </div>
      <div class="section__actions">
        <el-select
          :model-value="semanaId"
          placeholder="Seleccionar semana"
          style="width: 220px"
          clearable
          @change="handleCambioSemana">
          <el-option
            v-for="s in semanasStore.items"
            :key="s.id"
            :label="`Sem ${s.numeroSemana}/${s.anio} - ${s.estado}`"
            :value="s.id" />
        </el-select>
        <el-button
          type="primary"
          @click="handleNuevaSemana"
          v-if="authStore.isSupervisor"
          :icon="Plus">
          Nueva Semana
        </el-button>
      </div>
    </div>

    <!-- Loading -->
    <div v-if="loading" class="loading-center"><Spinner /></div>

    <!-- Sin semana activa -->
    <div v-else-if="!semana" class="card" style="text-align:center; padding: 60px;">
      <div style="font-size: 3rem; margin-bottom: 16px;">📅</div>
      <h2 style="margin-bottom: 8px;">Sin semana activa</h2>
      <p style="color: var(--text-3); margin-bottom: 24px;">
        Seleccioná una semana del listado o creá una nueva para empezar
      </p>
      <div style="display:flex; gap: 12px; justify-content: center;">
        <router-link to="/semanas" class="btn btn-secondary">Ver semanas</router-link>
        <router-link to="/semanas/nueva" class="btn btn-primary">Crear nueva</router-link>
      </div>
    </div>

    <!-- Contenido principal -->
    <template v-else>
      <!-- Barra de progreso general -->
      <div class="card" style="margin-bottom: var(--spacing-lg);">
        <div style="display:flex; align-items: center; justify-content: space-between; margin-bottom: var(--spacing-md);">
          <div>
            <h2 style="margin:0 0 4px 0;">Progreso de la semana</h2>
            <p style="color: var(--text-3); font-size: 0.875rem; margin: 0;">
              {{ productosConTirada }} de {{ totalProductos }} productos con tirada
            </p>
          </div>
          <div style="font-size: 2.5rem; font-weight: 700;" :style="{
            color: progreso >= 80 ? 'var(--color-success)'
              : progreso >= 50 ? 'var(--color-warning)'
              : 'var(--color-danger)'
          }">
            {{ progreso }}%
          </div>
        </div>
        <el-progress
          :percentage="progreso"
          :stroke-width="16"
          :show-text="false"
          :color="progreso >= 80 ? '#22c55e' : progreso >= 50 ? '#f59e0b' : '#ef4444'" />
      </div>

      <!-- Checklist de módulos -->
      <div style="margin-bottom: var(--spacing-lg);">
        <h2 style="margin-bottom: var(--spacing-md);">Módulos de la semana</h2>
        <div class="modulos-grid">
          <div
            v-for="mod in modulos"
            :key="mod.key"
            class="modulo-card"
            :class="{ 'modulo-card--ok': mod.cargado }">
            <div class="modulo-card__icon-wrap" :class="`icon-wrap--${mod.color}`">
              <component :is="mod.icon" class="modulo-card__icon" />
            </div>
            <div class="modulo-card__content">
              <div class="modulo-card__label">{{ mod.label }}</div>
              <div class="modulo-card__status">
                <span v-if="mod.cargado" class="modulo-card__check">✅</span>
                <span v-else class="modulo-card__pending">⏳</span>
                {{ mod.mensaje }}
              </div>
            </div>
            <router-link
              :to="mod.ruta"
              class="modulo-card__btn"
              :class="{ 'modulo-card__btn--primary': !mod.cargado }">
              {{ mod.boton }}
            </router-link>
          </div>
        </div>
      </div>

      <!-- Pendientes críticos -->
      <div class="card">
        <h2 style="margin-bottom: var(--spacing-md);">Pendientes</h2>
        <div v-if="pendientes.length === 0" style="text-align:center; padding: 24px; color: var(--color-success);">
          <div style="font-size: 2rem; margin-bottom: 8px;">🎉</div>
          <div>¡Todo en orden! No hay pendientes críticos</div>
        </div>
        <div v-else style="display:flex; flex-direction: column; gap: var(--spacing-sm);">
          <router-link
            v-for="(p, idx) in pendientes"
            :key="idx"
            :to="p.ruta"
            class="pendiente-item"
            :class="`pendiente--${p.tipo}`">
            <span class="pendiente-dot"></span>
            <span>{{ p.texto }}</span>
            <span style="margin-left:auto; color: var(--text-3);">→</span>
          </router-link>
        </div>
      </div>
    </template>
  </div>
</template>
