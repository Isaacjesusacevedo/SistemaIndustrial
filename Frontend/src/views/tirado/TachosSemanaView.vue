<!-- views/tirado/TachosSemanaView.vue -->
<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { useTiradoProductosStore } from '@/stores/tiradoProductos'
import { useCategoriasStore } from '@/stores/categorias'
import { useToast } from '@/composables/useToast'
import { useResponsive } from '@/composables/useResponsive'
import { Plus, ArrowLeft, Refresh } from '@element-plus/icons-vue'
import KpiCard from '@/components/KpiCard.vue'
import FormField from '@/components/FormField.vue'

const router = useRouter()
const route = useRoute()
const tiradoStore = useTiradoProductosStore()
const categoriasStore = useCategoriasStore()
const toast = useToast()
const { isMobile } = useResponsive()

const semanaId = computed(() => Number(route.params.semanaId))

// ── State ───────────────────────────────────────────────────────────
const loading = ref(false)
const filterProducto = ref<number | null>(null)
const filterDia = ref<string>('')
const showModal = ref(false)
const submitting = ref(false)

const form = ref({
  tiradoProductoId: 0,
  cantidad: 0,
  observaciones: '',
})

const errors = ref<Record<string, string>>({})

const diasSemana = ['sabado', 'lunes', 'martes', 'miercoles', 'jueves', 'viernes']
const diasLabels: Record<string, string> = {
  sabado: 'Sábado',
  lunes: 'Lunes',
  martes: 'Martes',
  miercoles: 'Miércoles',
  jueves: 'Jueves',
  viernes: 'Viernes',
}

// ── Carga ───────────────────────────────────────────────────────────
const cargarDatos = async () => {
  loading.value = true
  try {
    await Promise.all([
      tiradoStore.cargarTachosSemana(semanaId.value),
      tiradoStore.cargarPorSemana(semanaId.value),
      categoriasStore.cargarTodas(),
    ])
  } catch (e: any) {
    toast.error(e.message || 'Error al cargar tachos')
  } finally {
    loading.value = false
  }
}

// ── Helper: obtener día de la semana desde una fecha ────────────────
const getDiaKey = (fechaStr: string): string => {
  const fecha = new Date(fechaStr)
  switch (fecha.getDay()) {
    case 0: return 'domingo'
    case 1: return 'lunes'
    case 2: return 'martes'
    case 3: return 'miercoles'
    case 4: return 'jueves'
    case 5: return 'viernes'
    case 6: return 'sabado'
    default: return ''
  }
}

// ── Tiradas filtradas ───────────────────────────────────────────────
// ⚠️ IMPORTANTE: debe ir ANTES de resumenPorDia porque este la usa
const tiradasFiltradas = computed(() => {
  let result = [...tiradoStore.tachosSemana]

  if (filterProducto.value) {
    result = result.filter(t => t.tiradoProductoId === filterProducto.value)
  }

  if (filterDia.value) {
    result = result.filter(t => getDiaKey(t.fecha) === filterDia.value)
  }

  return result.sort((a, b) => new Date(b.fecha).getTime() - new Date(a.fecha).getTime())
})

// ── Resumen por día ─────────────────────────────────────────────────
const resumenPorDia = computed(() => {
  const resumen: Record<string, { cantidad: number; count: number }> = {}
  for (const dia of diasSemana) {
    resumen[dia] = { cantidad: 0, count: 0 }
  }

  for (const t of tiradasFiltradas.value) {
    const key = getDiaKey(t.fecha)
    if (resumen[key]) {
      resumen[key].cantidad += t.cantidad
      resumen[key].count += 1
    }
  }
  return resumen
})

const totalTachos = computed(() => tiradasFiltradas.value.length)

// ── Productos de la semana ──────────────────────────────────────────
const productosSemana = computed(() => tiradoStore.items)

// ── Número de tacho por producto ────────────────────────────────────
const tachoNumber = (productoId: number, index: number) => {
  const productoTachos = tiradasFiltradas.value
    .filter(t => t.tiradoProductoId === productoId)
    .sort((a, b) => new Date(a.fecha).getTime() - new Date(b.fecha).getTime())
  const target = tiradasFiltradas.value[index]
  const pos = productoTachos.findIndex(t => t.id === target.id)
  return pos + 1
}

// ── Formateo de fecha ───────────────────────────────────────────────
const formatDia = (fechaStr: string) => {
  const fecha = new Date(fechaStr)
  const jsDia = fecha.getDay()
  const nombres = ['Dom', 'Lun', 'Mar', 'Mié', 'Jue', 'Vie', 'Sáb']
  const diaStr = fecha.toLocaleDateString('es-AR', { day: '2-digit', month: '2-digit' })
  return `${nombres[jsDia]} ${diaStr}`
}

// ── Validación ──────────────────────────────────────────────────────
const validate = () => {
  errors.value = {}
  if (!form.value.tiradoProductoId) {
    errors.value.tiradoProductoId = 'Debe seleccionar un producto'
  }
  const cant = Number(form.value.cantidad)
  if (!cant || cant <= 0) {
    errors.value.cantidad = 'La cantidad debe ser mayor a 0'
  }
  if (form.value.observaciones && form.value.observaciones.length > 500) {
    errors.value.observaciones = 'Máximo 500 caracteres'
  }
  return Object.keys(errors.value).length === 0
}

// ── Abrir/cerrar modal ──────────────────────────────────────────────
const abrirModal = () => {
  form.value = { tiradoProductoId: 0, cantidad: 0, observaciones: '' }
  errors.value = {}
  showModal.value = true
}

const cerrarModal = () => {
  showModal.value = false
  errors.value = {}
}

// ── Registrar tacho ─────────────────────────────────────────────────
const handleSubmit = async () => {
  if (!validate()) return
  submitting.value = true
  try {
    await tiradoStore.registrarTirada(
      Number(form.value.tiradoProductoId),
      Number(form.value.cantidad),
      form.value.observaciones
    )
    toast.success('Tacho registrado correctamente')
    cerrarModal()
    await tiradoStore.cargarTachosSemana(semanaId.value)
  } catch (e: any) {
    toast.error(e.message || 'Error al registrar tacho')
  } finally {
    submitting.value = false
  }
}

// ── Summary de la tabla ─────────────────────────────────────────────
const getSummaries = (param: any) => {
  const { columns } = param
  const sums: string[] = []
  columns.forEach((column: any, index: number) => {
    if (index === 0) {
      sums[index] = 'TOTAL'
      return
    }
    if (column.property === 'cantidad' || column.label === 'Cantidad') {
      sums[index] = tiradasFiltradas.value
        .reduce((sum, t) => sum + t.cantidad, 0)
        .toLocaleString('es-AR')
      return
    }
    if (column.label === '# Tacho') {
      sums[index] = String(tiradasFiltradas.value.length)
      return
    }
    sums[index] = ''
  })
  return sums
}

onMounted(cargarDatos)
</script>

<template>
  <div class="section">
    <div class="section__header">
      <div>
        <h1>Tachos de la Semana</h1>
        <p class="section__subtitle">Semana {{ semanaId }}</p>
      </div>
      <div class="section__actions" style="flex-shrink: 0; gap: 8px;">
        <router-link
          :to="{ name: 'tirado', params: { semanaId } }"
          class="btn btn-secondary"
          style="white-space: nowrap;">
          <ArrowLeft /> Volver
        </router-link>
        <el-button
          type="primary"
          @click="abrirModal"
          :icon="Plus"
          style="white-space: nowrap;">
          Registrar Tacho
        </el-button>
      </div>
    </div>

    <div v-if="loading" class="loading-center"><Spinner /></div>

    <div v-else class="card">
      <!-- Resumen superior (6 tarjetas) -->
      <div class="kpi-grid" style="margin-bottom: var(--spacing-lg); grid-template-columns: repeat(6, 1fr);">
        <KpiCard
          v-for="dia in diasSemana"
          :key="dia"
          :title="diasLabels[dia]"
          :value="resumenPorDia[dia].cantidad.toLocaleString('es-AR')"
          :trend="resumenPorDia[dia].count"
          trend-label="# tachos"
          :icon="resumenPorDia[dia].count > 0 ? '📦' : '—'"
          :color="resumenPorDia[dia].count > 0 ? 'primary' : 'info'"
        />
      </div>

      <!-- Filtros -->
      <div class="filter-row">
        <el-select
          v-model="filterProducto"
          placeholder="Filtrar por producto"
          style="width: 280px;"
          clearable>
          <el-option label="Todos los productos" :value="null" />
          <el-option
            v-for="p in productosSemana"
            :key="p.id"
            :label="p.nombre"
            :value="p.id" />
        </el-select>

        <el-select
          v-model="filterDia"
          placeholder="Filtrar por día"
          style="width: 180px;"
          clearable>
          <el-option label="Todos los días" value="" />
          <el-option
            v-for="dia in diasSemana"
            :key="dia"
            :label="diasLabels[dia]"
            :value="dia" />
        </el-select>

        <el-button @click="cargarDatos" :icon="Refresh">Actualizar</el-button>
      </div>

      <!-- Tabla -->
      <el-table
        :data="tiradasFiltradas"
        border
        stripe
        style="width: 100%"
        :summary-method="getSummaries"
        :show-summary="tiradasFiltradas.length > 0"
        :empty-text="filterProducto || filterDia ? 'No hay tachos con los filtros seleccionados' : 'No hay tachos registrados esta semana'">
        <el-table-column prop="fecha" label="Día" width="140">
          <template #default="{ row }">
            {{ formatDia(row.fecha) }}
          </template>
        </el-table-column>
        <el-table-column prop="productoNombre" label="Producto" min-width="200" />
        <el-table-column label="Cantidad" width="130" align="right">
          <template #default="{ row }">
            <span style="font-family: var(--font-mono); font-weight:600;">
              {{ row.cantidad.toLocaleString('es-AR') }}
            </span>
          </template>
        </el-table-column>
        <el-table-column label="# Tacho" width="100" align="center">
          <template #default="{ row, $index }">
            <span class="badge">{{ tachoNumber(row.tiradoProductoId, $index) }}</span>
          </template>
        </el-table-column>
        <el-table-column v-if="!isMobile" prop="observaciones" label="Observaciones" min-width="200">
          <template #default="{ row }">
            <span v-if="row.observaciones">{{ row.observaciones }}</span>
            <span v-else style="color:var(--text-3)">—</span>
          </template>
        </el-table-column>
      </el-table>
    </div>

    <!-- Modal registrar tacho -->
    <el-dialog
      v-model="showModal"
      title="Registrar Tacho"
      width="550px"
      :fullscreen="isMobile"
      :before-close="cerrarModal">
      <form @submit.prevent="handleSubmit" class="form">
        <FormField
          label="Producto"
          v-model="form.tiradoProductoId"
          type="select"
          :options="productosSemana.map(p => ({ value: p.id, label: p.nombre }))"
          :error="errors.tiradoProductoId"
          placeholder="Seleccionar producto"
          required />

        <FormField
          label="Cantidad"
          v-model="form.cantidad"
          type="number"
          :min="1"
          :step="1"
          :error="errors.cantidad"
          required />

        <FormField
          label="Observaciones"
          v-model="form.observaciones"
          type="textarea"
          :error="errors.observaciones"
          placeholder="Observaciones opcionales"
          :rows="3" />

        <div class="form-actions">
          <button type="button" class="btn btn-secondary" @click="cerrarModal">Cancelar</button>
          <button type="submit" class="btn btn-primary" :disabled="submitting">
            <span v-if="submitting">Guardando...</span>
            <span v-else>Registrar</span>
          </button>
        </div>
      </form>
    </el-dialog>
  </div>
</template>

<style scoped src="@/assets/styles/views/tachos-semana.css"></style>

<style scoped>
/* Tema oscuro en el-table */
:deep(.el-table) {
  --el-table-bg-color: transparent;
  --el-table-tr-bg-color: transparent;
  --el-table-header-bg-color: var(--bg-3);
  --el-table-row-hover-bg-color: var(--surface);
  --el-table-border-color: var(--border);
  --el-table-text-color: var(--text-0);
  --el-table-header-text-color: var(--text-3);
  background: transparent;
  color: var(--text-0);
}

:deep(.el-table th.el-table__cell) {
  background: var(--bg-3);
  color: var(--text-3);
  font-family: var(--font-mono);
  font-size: 11px;
  font-weight: 400;
  letter-spacing: 0.06em;
  text-transform: uppercase;
  border-bottom: 1px solid var(--border);
  padding: 8px 10px;
}

:deep(.el-table td.el-table__cell) {
  background: transparent;
  color: var(--text-1);
  border-bottom: 1px solid var(--border);
  padding: 8px 10px;
}

:deep(.el-table tr) {
  background: transparent;
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

:deep(.el-table__footer tr > td.el-table__cell) {
  background: var(--bg-3);
  color: var(--text-0);
  font-weight: 700;
}

:deep(.el-table__inner-wrapper::before),
:deep(.el-table__inner-wrapper::after) {
  display: none;
}

:deep(.el-table--border),
:deep(.el-table--border .el-table__cell) {
  border-color: var(--border);
}

:deep(.el-table__empty-block) {
  background: transparent;
}

:deep(.el-table__empty-text) {
  color: var(--text-2);
}
</style>