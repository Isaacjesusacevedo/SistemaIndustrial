<!-- views/armado/ArmadoView.vue -->
<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { useSemanasStore } from '@/stores/semanas'
import { useTiradoProductosStore } from '@/stores/tiradoProductos'
import { useToast } from '@/composables/useToast'
import { useResponsive } from '@/composables/useResponsive'
import { ArrowLeft, Box, Check, Warning, DataAnalysis, Edit } from '@element-plus/icons-vue'
import KpiCard from '@/components/KpiCard.vue'
import FormField from '@/components/FormField.vue'
import type { FiltroTirado, TiradoProducto } from '@/types/tiradoProducto'

const router = useRouter()
const route = useRoute()
const semanasStore = useSemanasStore()
const tiradoStore = useTiradoProductosStore()
const toast = useToast()
const { isMobile } = useResponsive()

const semanaId = computed(() => Number(route.params.semanaId))

const semana = ref<any>(null)
const loading = ref(false)

const filtros: { key: FiltroTirado; label: string }[] = [
  { key: 'totales', label: 'Todos' },
  { key: 'faltantes', label: 'Faltantes' },
  { key: 'sobrantes', label: 'Sobrantes' },
]

// ── Carga ───────────────────────────────────────────────────────────
const cargarDatos = async () => {
  loading.value = true
  try {
    const [s] = await Promise.all([
      semanasStore.fetchById(semanaId.value),
      tiradoStore.cargarPorSemana(semanaId.value),
    ])
    semana.value = s
  } catch (e: any) {
    toast.error(e.message || 'Error al cargar armado')
  } finally {
    loading.value = false
  }
}

// ── Navegación ─────────────────────────────────────────────────────
const irATirado = () =>
  router.push({ name: 'tirado', params: { semanaId: semanaId.value } })

// ── Modal modificar stock (ajuste por conteo físico) ────────────────
const showEditarStockModal = ref(false)
const productoEditando = ref<TiradoProducto | null>(null)
const nuevoStock = ref<number>(0)
const errorStock = ref('')
const guardandoStock = ref(false)

const abrirModalEditarStock = (row: TiradoProducto) => {
  productoEditando.value = row
  nuevoStock.value = row.stockTotal
  errorStock.value = ''
  showEditarStockModal.value = true
}

const cerrarEditarStock = () => {
  showEditarStockModal.value = false
  productoEditando.value = null
  errorStock.value = ''
}

const guardarStock = async () => {
  if (!productoEditando.value) return

  if (nuevoStock.value < 0) {
    errorStock.value = 'El stock no puede ser negativo'
    return
  }

  guardandoStock.value = true
  try {
    // Si hay endpoint, usarlo
    // await tiradoStore.actualizarStock(productoEditando.value.id, nuevoStock.value)

    // Por ahora: toast de "en desarrollo"
    toast.info('Función de ajuste de stock en desarrollo')
    cerrarEditarStock()
  } catch (e: any) {
    toast.error(e.message || 'Error al actualizar stock')
  } finally {
    guardandoStock.value = false
  }
}

// ── Cambio de filtro ────────────────────────────────────────────────
const cambiarFiltro = (nuevo: FiltroTirado) => {
  tiradoStore.cambiarFiltro(nuevo)
}

// ── Helpers visuales ────────────────────────────────────────────────
const getEstadoType = (estado: string) => {
  switch (estado) {
    case 'Listo': return 'success'
    case 'Sobra': return 'warning'
    case 'Falta': return 'danger'
    default: return 'info'
  }
}

const getProgreso = (item: TiradoProducto) => {
  if (!item.pedidoSemanal || item.pedidoSemanal <= 0) return 0
  const disponible = item.stockInicial + item.cantidadTirada
  return Math.min(100, Math.round((disponible / item.pedidoSemanal) * 100))
}

const formatNum = (n: number) => n.toLocaleString('es-AR')

onMounted(cargarDatos)
</script>

<template>
  <div class="section">
    <div class="section__header" v-if="!loading && semana">
      <div>
        <h1>Sector Armado</h1>
        <p class="section__subtitle">
          Semana {{ semana.numeroSemana }}/{{ semana.anio }} · {{ semana.estado }}
        </p>
      </div>
      <div class="section__actions">
        <router-link :to="{ name: 'semanas' }" class="btn btn-secondary">
          <ArrowLeft /> Volver
        </router-link>
        <el-button type="primary" @click="irATirado" :icon="Box">
          Ir a Tirado
        </el-button>
      </div>
    </div>

    <div v-if="loading" class="loading-center"><Spinner /></div>

    <div v-else-if="semana" class="card">
      <!-- KPIs del sector armado -->
      <div class="kpi-grid" style="margin-bottom: var(--spacing-lg);">
        <KpiCard
          title="Total Productos"
          :value="tiradoStore.totales.total"
          icon="📦"
          color="primary"
        />
        <KpiCard
          title="Completados"
          :value="tiradoStore.totales.completados"
          :trend="tiradoStore.totales.porcentaje"
          trend-label="% del total"
          icon="✅"
          :color="tiradoStore.totales.porcentaje >= 80 ? 'success' : 'warning'"
        />
        <KpiCard
          title="Faltantes"
          :value="tiradoStore.totales.faltantes"
          icon="⚠️"
          :color="tiradoStore.totales.faltantes > 0 ? 'danger' : 'success'"
        />
        <KpiCard
          title="Sobrantes"
          :value="tiradoStore.totales.sobrantes"
          icon="📈"
          :color="tiradoStore.totales.sobrantes > 0 ? 'warning' : 'info'"
        />
        <KpiCard
          title="Tachos Faltantes"
          :value="`-${tiradoStore.totales.totalTachosFaltantes}`"
          icon="🔻"
          :color="tiradoStore.totales.totalTachosFaltantes > 0 ? 'danger' : 'success'"
        />
        <KpiCard
          title="Tachos Sobrantes"
          :value="`+${tiradoStore.totales.totalTachosSobrantes}`"
          icon="🔺"
          :color="tiradoStore.totales.totalTachosSobrantes > 0 ? 'success' : 'info'"
        />
      </div>

      <!-- Progreso general -->
      <div class="card" style="margin-bottom: var(--spacing-lg);">
        <div style="display:flex; align-items:center; justify-content:space-between; margin-bottom: var(--spacing-sm);">
          <h3 style="margin:0;">Progreso general</h3>
          <span style="font-weight:700; color:var(--color-primary);">
            {{ tiradoStore.totales.porcentaje }}%
          </span>
        </div>
        <el-progress
          :percentage="tiradoStore.totales.porcentaje"
          :stroke-width="14"
          :color="tiradoStore.totales.porcentaje >= 80 ? '#22c55e' : tiradoStore.totales.porcentaje >= 50 ? '#f59e0b' : '#ef4444'" />
      </div>

      <!-- Filtros -->
      <div style="margin-bottom: var(--spacing-md);">
        <el-radio-group :model-value="tiradoStore.filtro" @update:model-value="cambiarFiltro">
          <el-radio-button v-for="f in filtros" :key="f.key" :value="f.key">
            {{ f.label }}
          </el-radio-button>
        </el-radio-group>
      </div>

      <!-- Tabla (solo visualización + ajuste de stock) -->
      <el-table
        :data="tiradoStore.filtrados"
        border
        stripe
        style="width: 100%"
        empty-text="No hay productos para mostrar en esta vista">
        <el-table-column prop="nombre" label="Producto" min-width="140" />
        <el-table-column prop="unidadMedida" label="Unidad" width="120" />

        <el-table-column label="Pedido" width="80" align="right">
          <template #default="{ row }">{{ formatNum(row.pedidoSemanal) }}</template>
        </el-table-column>

        <el-table-column label="Tirado" width="90" align="right">
          <template #default="{ row }">
            <span style="font-family: var(--font-mono); font-weight:600;">
              {{ formatNum(row.cantidadTirada) }}
            </span>
          </template>
        </el-table-column>

        <el-table-column label="Progreso" width="130">
          <template #default="{ row }">
            <div style="display:flex; align-items:center; gap:8px;">
              <el-progress
                :percentage="getProgreso(row)"
                :stroke-width="6"
                :show-text="false"
                :color="getProgreso(row) >= 100 ? '#22c55e' : getProgreso(row) >= 50 ? '#f59e0b' : '#ef4444'"
                style="flex:1;" />
              <span style="font-size:0.75rem; color: var(--text-3); min-width: 35px;">
                {{ getProgreso(row) }}%
              </span>
            </div>
          </template>
        </el-table-column>

        <el-table-column label="Stock Total" width="110" align="right">
          <template #default="{ row }">
            <span style="font-family: var(--font-mono); font-weight:600;"
              :class="row.stockTotal < 0 ? 'text-danger' : row.stockTotal === 0 ? 'text-success' : 'text-warning'">
              {{ formatNum(row.stockTotal) }}
            </span>
          </template>
        </el-table-column>

        <el-table-column label="Tachos Est." width="95" align="right">
          <template #default="{ row }">
            <span style="font-family: var(--font-mono);"
              :class="row.tachosEstimados < 0 ? 'text-danger' : row.tachosEstimados === 0 ? 'text-success' : 'text-warning'">
              {{ row.tachosEstimados.toFixed(2) }}
            </span>
          </template>
        </el-table-column>

        <el-table-column label="Estado" width="85">
          <template #default="{ row }">
            <el-tag :type="getEstadoType(row.estado)" size="small">
              {{ row.estado }}
            </el-tag>
          </template>
        </el-table-column>

        <el-table-column label="Acciones" width="130" fixed="right">
          <template #default="{ row }">
            <el-button
              size="small"
              type="warning"
              :icon="Edit"
              @click="abrirModalEditarStock(row)">
              Stock
            </el-button>
          </template>
        </el-table-column>
      </el-table>
    </div>

    <!-- Modal modificar stock -->
    <el-dialog
      v-model="showEditarStockModal"
      title="Modificar stock"
      width="450px"
      :fullscreen="isMobile"
      :before-close="cerrarEditarStock">

      <div v-if="productoEditando" style="margin-bottom: var(--spacing-md); padding: var(--spacing-md); background: var(--bg-3); border-radius: var(--radius-md);">
        <p style="margin:0 0 8px 0; color: var(--text-1); font-size:0.875rem;">
          <strong style="color: var(--text-0);">{{ productoEditando.nombre }}</strong>
          · {{ productoEditando.unidadMedida }}
        </p>
        <p style="margin:0 0 8px 0; color: var(--text-2); font-size:0.75rem;">
          Stock calculado actual:
          <strong style="font-family: var(--font-mono); color: var(--text-0);">
            {{ formatNum(productoEditando.stockTotal) }}
          </strong>
        </p>
        <p style="margin:0; color: var(--text-3); font-size:0.7rem;">
          = Stock inicial ({{ formatNum(productoEditando.stockInicial) }})
          + Tirado ({{ formatNum(productoEditando.cantidadTirada) }})
          − Pedido ({{ formatNum(productoEditando.pedidoSemanal) }})
        </p>
      </div>

      <FormField
        label="Stock real (ajustado)"
        v-model="nuevoStock"
        type="number"
        :step="1"
        :error="errorStock"
        placeholder="Ej: 91"
        hint="Valor real contado en el depósito" />

      <div class="form-actions" style="display:flex; justify-content:flex-end; gap:8px; margin-top: var(--spacing-md);">
        <button type="button" class="btn btn-secondary" @click="cerrarEditarStock">
          Cancelar
        </button>
        <button
          type="button"
          class="btn btn-primary"
          @click="guardarStock"
          :disabled="guardandoStock">
          {{ guardandoStock ? 'Guardando...' : 'Actualizar' }}
        </button>
      </div>
    </el-dialog>
  </div>
</template>

<style scoped src="@/assets/styles/views/armado.css"></style>

<style scoped>
/* Fix tema oscuro en el-table */
:deep(.el-table) {
  --el-table-bg-color: transparent;
  --el-table-tr-bg-color: transparent;
  --el-table-header-bg-color: var(--bg-3);
  --el-table-row-hover-bg-color: var(--surface);
  --el-table-border-color: var(--border);
  --el-table-text-color: var(--text-0);
  --el-table-header-text-color: var(--text-3);
  background: transparent;
}

:deep(.el-table th.el-table__cell) {
  background: var(--bg-3);
  color: var(--text-3);
  font-family: var(--font-mono);
  font-size: 11px;
  text-transform: uppercase;
  letter-spacing: 0.06em;
  border-bottom: 1px solid var(--border);
  padding: 8px 10px;
}

:deep(.el-table td.el-table__cell) {
  background: transparent;
  color: var(--text-1);
  border-bottom: 1px solid var(--border);
  padding: 8px 10px;
}

:deep(.el-table__body tr:hover > td.el-table__cell) {
  background: var(--surface) !important;
}

/* Fix stripe en tema oscuro */
:deep(.el-table__body tr.el-table__row--striped td.el-table__cell) {
  background: rgba(255, 255, 255, 0.02) !important;
}

:deep(.el-table__body tr:not(.el-table__row--striped) td.el-table__cell) {
  background: transparent !important;
}

:deep(.el-table__inner-wrapper::before) {
  display: none;
}

.text-danger { color: var(--red) !important; }
.text-success { color: var(--green) !important; }
.text-warning { color: var(--amber) !important; }

/* KPIs compactos */
:deep(.kpi-card) {
  padding: var(--spacing-md) !important;
}

:deep(.kpi-card__value) {
  font-size: 1.75rem !important;
}

:deep(.kpi-card__title) {
  font-size: 0.75rem !important;
}
</style>