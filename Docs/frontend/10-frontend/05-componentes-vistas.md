# Componentes y Vistas

## Componentes Reutilizables (`components/`)

Componentes reales: `DataTable`, `FormField`, `SelectField`, `DatePickerField`, `KpiCard`, `ChartContainer`, `Modal`, `Spinner`, `NavBarUserMenu` (+ `index.ts` como barrel).

> No existen `NavBar.vue` (la navegación vive en el sidebar de `App.vue`), ni `DataTableColumn`, `ConfirmDialog`, `LoadingOverlay`, `EmptyState`, `Breadcrumb`, `Pagination`, `Alert`, `Badge`, `Avatar`, `Dropdown` o `Tooltip`. El barrel `index.ts` exporta un `NavBar` inexistente (pendiente corregir).

### Sidebar (`App.vue`, no es un componente separado)
```vue
<script setup lang="ts">
// Patrón real (extracto simplificado de App.vue)
import { useSemanaActivaStore } from '@/stores/SemanaActiva'
import { useResponsive } from '@/composables/useResponsive'

const semanaActiva = useSemanaActivaStore()
const { isMobile } = useResponsive()
const sidebarOpen = ref(false) // oculto por defecto en mobile

// Links fijos: dashboard (/dashboard), semanas, nueva-semana, productos
// Links de semana activa (solo si tieneSemanaActiva): insumos, stock, tirado, armado
// Sidebar oculto en rutas login y not-found
</script>

<template>
  <!-- Botón hamburguesa (solo mobile) + overlay para cerrar -->
  <!-- <nav class="app-nav"> con secciones Producción / Semana activa -->
  <!-- <main class="app-main"><RouterView /></main> -->
</template>
```

> El menú se filtra por semana activa (`semanaActiva.tieneSemanaActiva`), no por roles. Los iconos de los módulos de semana son de `@element-plus/icons-vue` (`Box`, `Goods`, `TrendCharts`, `DataAnalysis`, `Menu`, `Plus`).

### KpiCard (`KpiCard.vue`)
```vue
<script setup lang="ts">
interface Props {
  title: string
  value: string | number
  trend?: number // Porcentaje cambio vs periodo anterior
  trendLabel?: string
  icon?: string
  color?: 'primary' | 'success' | 'warning' | 'danger' | 'info'
}

const props = withDefaults(defineProps<Props>(), {
  trend: 0,
  color: 'primary'
})

const trendClass = computed(() => {
  if (props.trend > 0) return 'kpi-card__trend--up'
  if (props.trend < 0) return 'kpi-card__trend--down'
  return 'kpi-card__trend--neutral'
})

const trendIcon = computed(() => {
  if (props.trend > 0) return '↑'
  if (props.trend < 0) return '↓'
  return '→'
})
</script>

<template>
  <div class="kpi-card" :class="`kpi-card--${color}`">
    <div class="kpi-card__header">
      <h3 class="kpi-card__title">{{ title }}</h3>
      <span v-if="icon" class="kpi-card__icon">{{ icon }}</span>
    </div>
    <div class="kpi-card__value">{{ value }}</div>
    <div v-if="trend !== 0" class="kpi-card__trend" :class="trendClass">
      <span class="kpi-card__trend-icon">{{ trendIcon }}</span>
      <span class="kpi-card__trend-value">{{ Math.abs(trend) }}%</span>
      <span v-if="trendLabel" class="kpi-card__trend-label">{{ trendLabel }}</span>
    </div>
  </div>
</template>

<style scoped>
.kpi-card { padding: var(--spacing-lg); background: var(--color-bg-card); border-radius: var(--radius-lg); border: 1px solid var(--color-border); }
.kpi-card--primary { border-left: 4px solid var(--color-primary); }
.kpi-card--success { border-left: 4px solid var(--color-success); }
.kpi-card--warning { border-left: 4px solid var(--color-warning); }
.kpi-card--danger { border-left: 4px solid var(--color-danger); }
.kpi-card--info { border-left: 4px solid var(--color-info); }
.kpi-card__header { display: flex; justify-content: space-between; align-items: flex-start; margin-bottom: var(--spacing-sm); }
.kpi-card__title { font-size: 0.875rem; font-weight: 500; color: var(--color-text-secondary); text-transform: uppercase; letter-spacing: 0.5px; }
.kpi-card__value { font-size: 2rem; font-weight: 700; color: var(--color-text-primary); line-height: 1.2; }
.kpi-card__trend { display: flex; align-items: center; gap: 0.25rem; margin-top: var(--spacing-sm); font-size: 0.75rem; font-weight: 600; }
.kpi-card__trend--up { color: var(--color-success); }
.kpi-card__trend--down { color: var(--color-danger); }
.kpi-card__trend--neutral { color: var(--color-text-tertiary); }
</style>
```

### DataTable (`DataTable.vue`)
```vue
<script setup lang="ts">
import type { Column } from '@/types/common'
import { h } from 'vue'

interface Props<T = any> {
  items: T[]
  columns: Column<T>[]
  loading?: boolean
  emptyMessage?: string
  striped?: boolean
  hoverable?: boolean
  onRowClick?: (item: T) => void
}

const props = withDefaults(defineProps<Props>(), {
  loading: false,
  emptyMessage: 'No hay datos disponibles',
  striped: true,
  hoverable: true
})

const emit = defineEmits<{ rowClick: [item: any] }>()

const renderCell = (item: any, column: Column) => {
  if (column.render) return column.render(item)
  const value = item[column.key]
  if (value === null || value === undefined) return '—'
  if (typeof value === 'boolean') return value ? '✓' : '✗'
  if (value instanceof Date) return formatDate(value)
  return String(value)
}
</script>

<template>
  <div class="data-table-wrapper">
    <div v-if="loading" class="data-table__loading">
      <div class="spinner"></div>
      <span>Cargando...</span>
    </div>
    
    <div v-else class="data-table-container">
      <table v-if="items.length" class="data-table" :class="{ 'data-table--striped': striped, 'data-table--hoverable': hoverable }">
        <thead>
          <tr>
            <th v-for="col in columns" :key="col.key" :class="col.class" :style="{ width: col.width }">
              {{ col.label }}
            </th>
          </tr>
        </thead>
        <tbody>
          <tr 
            v-for="item in items" 
            :key="item.id" 
            :class="{ 'data-table__row--clickable': onRowClick }"
            @click="onRowClick && onRowClick(item)"
          >
            <td v-for="col in columns" :key="col.key" :class="col.class">
              {{ renderCell(item, col) }}
            </td>
          </tr>
        </tbody>
      </table>
      
      <div v-else class="data-table__empty">
        {{ emptyMessage }}
      </div>
    </div>
  </div>
</template>
```

### Modal (`Modal.vue`)
```vue
<script setup lang="ts">
import { ref, watch } from 'vue'
import { Teleport } from 'vue'

interface Props {
  modelValue: boolean
  title?: string
  size?: 'sm' | 'md' | 'lg' | 'xl' | 'full'
  closeOnOverlayClick?: boolean
  showCloseButton?: boolean
}

const props = withDefaults(defineProps<Props>(), {
  size: 'md',
  closeOnOverlayClick: true,
  showCloseButton: true
})

const emit = defineEmits<{ 'update:modelValue': [value: boolean] }>()

const close = () => emit('update:modelValue', false)

watch(() => props.modelValue, (val) => {
  if (val) document.body.style.overflow = 'hidden'
  else document.body.style.overflow = ''
})
</script>

<template>
  <Teleport to="body">
    <Transition name="modal">
      <div v-if="modelValue" class="modal-overlay" @click.self="closeOnOverlayClick && close">
        <div class="modal" :class="`modal--${size}`" @click.stop>
          <div class="modal__header">
            <h3 class="modal__title">{{ title }}</h3>
            <button v-if="showCloseButton" @click="close" class="modal__close" aria-label="Cerrar">×</button>
          </div>
          <div class="modal__body">
            <slot />
          </div>
          <div class="modal__footer">
            <slot name="footer">
              <button @click="close" class="btn btn-secondary">Cancelar</button>
              <button @click="$emit('confirm')" class="btn btn-primary">Confirmar</button>
            </slot>
          </div>
        </div>
      </div>
    </Transition>
  </Teleport>
</template>

<style scoped>
.modal-overlay { position: fixed; inset: 0; background: rgba(0,0,0,0.5); display: flex; align-items: center; justify-content: center; padding: var(--spacing-md); z-index: 1000; }
.modal { background: var(--color-bg-card); border-radius: var(--radius-lg); box-shadow: var(--shadow-xl); max-height: 90vh; display: flex; flex-direction: column; }
.modal--sm { width: 100%; max-width: 400px; }
.modal--md { width: 100%; max-width: 600px; }
.modal--lg { width: 100%; max-width: 900px; }
.modal--xl { width: 100%; max-width: 1200px; }
.modal--full { width: 100%; max-width: 100%; height: 100%; max-height: 100%; }
.modal__header { display: flex; justify-content: space-between; align-items: center; padding: var(--spacing-lg); border-bottom: 1px solid var(--color-border); }
.modal__title { margin: 0; font-size: 1.125rem; font-weight: 600; }
.modal__close { background: none; border: none; font-size: 1.5rem; cursor: pointer; color: var(--color-text-tertiary); padding: 0; line-height: 1; }
.modal__body { padding: var(--spacing-lg); overflow-y: auto; flex: 1; }
.modal__footer { display: flex; justify-content: flex-end; gap: var(--spacing-sm); padding: var(--spacing-lg); border-top: 1px solid var(--color-border); }

.modal-enter-active, .modal-leave-active { transition: all 0.2s ease; }
.modal-enter-from { opacity: 0; transform: scale(0.95); }
.modal-leave-to { opacity: 0; transform: scale(0.95); }
</style>
```

### FormField (`FormField.vue`)
```vue
<script setup lang="ts">
import { computed } from 'vue'

interface Props {
  label: string
  modelValue: any
  type?: 'text' | 'email' | 'password' | 'number' | 'date' | 'textarea' | 'select'
  options?: { value: any; label: string }[]
  placeholder?: string
  error?: string
  hint?: string
  required?: boolean
  disabled?: boolean
  readonly?: boolean
  autocomplete?: string
  rows?: number
}

const props = defineProps<Props>()
const emit = defineEmits<{ 'update:modelValue': [value: any] }>()

const inputId = computed(() => `field-${props.label.toLowerCase().replace(/\s+/g, '-')}`)
</script>

<template>
  <div class="form-field" :class="{ 'form-field--error': error, 'form-field--required': required, 'form-field--disabled': disabled }">
    <label :for="inputId" class="form-field__label">
      {{ label }}
      <span v-if="required" class="form-field__asterisk" aria-hidden="true">*</span>
    </label>
    
    <div class="form-field__input-wrapper">
      <input
        v-if="type !== 'select' && type !== 'textarea'"
        :id="inputId"
        :type="type || 'text'"
        :value="modelValue"
        @input="emit('update:modelValue', $event.target.value)"
        :placeholder="placeholder"
        :disabled="disabled"
        :readonly="readonly"
        :autocomplete="autocomplete"
        :aria-invalid="!!error"
        :aria-describedby="error ? `${inputId}-error` : hint ? `${inputId}-hint` : undefined"
        class="form-field__input"
      />
      
      <select
        v-else-if="type === 'select'"
        :id="inputId"
        :value="modelValue"
        @change="emit('update:modelValue', $event.target.value)"
        :disabled="disabled"
        :aria-invalid="!!error"
        class="form-field__input"
      >
        <option v-if="placeholder" value="" disabled>{{ placeholder }}</option>
        <option v-for="opt in options" :key="opt.value" :value="opt.value">{{ opt.label }}</option>
      </select>
      
      <textarea
        v-else
        :id="inputId"
        :value="modelValue"
        @input="emit('update:modelValue', $event.target.value)"
        :placeholder="placeholder"
        :disabled="disabled"
        :readonly="readonly"
        :rows="rows || 4"
        :aria-invalid="!!error"
        class="form-field__input form-field__input--textarea"
      />
    </div>
    
    <span v-if="error" :id="`${inputId}-error`" class="form-field__error" role="alert">{{ error }}</span>
    <span v-else-if="hint" :id="`${inputId}-hint`" class="form-field__hint">{{ hint }}</span>
  </div>
</template>
```

---

## Responsive y Tema Oscuro (Código Real)

### useResponsive (`composables/useResponsive.ts`)
```typescript
// Breakpoints reales
isMobile:  width <= 640
isTablet:  640 < width <= 1024
isDesktop: width > 1024
```

### Patrones responsive usados
- **Sidebar**: en tablet (641–1024px) colapsa a solo íconos (`app-shell.css`); en mobile (≤640px) se oculta y se abre con botón hamburguesa + overlay, cerrándose al navegar.
- **Diálogos fullscreen en mobile**: `:fullscreen="isMobile"` en `ArmadoView`, `TiradoView`, `TachosSemanaView` y `NavBarUserMenu` (cambio de contraseña).
- **Columnas ocultas en mobile**: `v-if="!isMobile"` (ej. columna `observaciones` en `TachosSemanaView`, `Tachos Est.` en `TiradoView`).
- **Tablas**: `table.css` reduce paddings y fuentes con `@media (max-width: 640px)`.

### Tema oscuro
- `main.ts` importa `element-plus/theme-chalk/dark/css-vars.css`.
- `src/assets/styles/element-plus-dark.css` redefine diálogos, tablas y formularios de Element Plus con las variables propias (`--bg-2`, `--text-0`, `--border`, ...). No hay conmutador claro/oscuro: el tema oscuro es el estilo base.

---

## Vistas Principales

> Vistas reales por módulo: `semanas/` (5: lista, nueva, editar, detalle, wizard `cargar-inicial`), `insumos/` (lista + nuevo), `stock/` (lista + nuevo), `tirado/` (`TiradoView` + `TachosSemanaView`), `armado/` (`ArmadoView`), `productos/` (lista + nuevo + editar), `dashboard/`, `auth/` y `not-found/`. Las vistas de `registrotacho/` existen pero sin ruta. No hay vistas de objetivos.

### DashboardView (`views/dashboard/DashboardView.vue`, ejemplo simplificado)
```vue
<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { useDashboardStore } from '@/stores/dashboard'
import KpiCard from '@/components/KpiCard.vue'
import ChartContainer from '@/components/ChartContainer.vue'
import { useAuthStore } from '@/stores/auth'

const dashboardStore = useDashboardStore()
const authStore = useAuthStore()

const metricas = ref<DashboardMetricasDto | null>(null)
const tendencias = ref<DashboardTendenciaDto[]>([])
const alertas = ref<DashboardAlertaDto[]>([])
const loading = ref(false)
const error = ref<string | null>(null)

const cargarDashboard = async () => {
  loading.value = true
  error.value = null
  try {
    const [m, t, a] = await Promise.all([
      dashboardStore.getMetricas(),
      dashboardStore.getTendencias(),
      dashboardStore.getAlertas()
    ])
    metricas.value = m
    tendencias.value = t
    alertas.value = a
  } catch (e: any) {
    error.value = e.message
  } finally {
    loading.value = false
  }
}

onMounted(cargarDashboard)
</script>

<template>
  <div class="section">
    <div class="section__header">
      <h1>Dashboard de Producción</h1>
      <div class="section__actions">
        <el-select v-model="periodo" placeholder="Período" style="width: 180px">
          <el-option label="Última semana" value="week" />
          <el-option label="Último mes" value="month" />
          <el-option label="Último trimestre" value="quarter" />
        </el-select>
      </div>
    </div>
    
    <!-- KPIs Cards -->
    <div class="kpi-grid" v-if="metricas">
      <KpiCard 
        title="Total Producido" 
        :value="metricas.totalProducido.toLocaleString()" 
        :trend="metricas.tendenciaProduccion"
        trendLabel="vs semana anterior"
        icon="📦"
        color="primary"
      />
      <KpiCard 
        title="Calidad" 
        :value="metricas.porcentajeCalidad.toFixed(1) + '%'" 
        :trend="metricas.tendenciaCalidad"
        trendLabel="vs semana anterior"
        icon="✅"
        :color="metricas.porcentajeCalidad >= 95 ? 'success' : 'warning'"
      />
      <KpiCard 
        title="Insumos Consumidos" 
        :value="metricas.insumosConsumidos.toLocaleString()" 
        icon="📦"
        :color="'info'"
      />
      <KpiCard 
        title="Stock Actual" 
        :value="metricas.stockActual.toLocaleString()" 
        :trend="metricas.tendenciaStock"
        trendLabel="vs semana anterior"
        icon="📦"
        :color="metricas.stockActual > metricas.stockMinimo ? 'success' : 'danger'"
      />
    </div>
    
    <!-- Alertas -->
    <div v-if="alertas.length" class="alertas-section">
      <h2>Alertas</h2>
      <div class="alertas-grid">
        <el-alert 
          v-for="alerta in alertas" 
          :key="alerta.tipo + alerta.mensaje"
          :title="alerta.mensaje"
          :description="alerta.detalle"
          :type="alerta.severidad === 'Critical' ? 'error' : alerta.severidad === 'Warning' ? 'warning' : 'info'"
          :closable="false"
          class="alerta-item"
        />
      </div>
    </div>
    
    <!-- Gráficos -->
    <div class="charts-grid">
      <ChartContainer title="Tendencia de Producción" :height="300">
        <canvas ref="produccionChart"></canvas>
      </ChartContainer>
      <ChartContainer title="Tendencia de Calidad" :height="300">
        <canvas ref="calidadChart"></canvas>
      </ChartContainer>
    </div>
  </div>
</template>
```

### SemanasView (`views/semanas/SemanasView.vue`, ejemplo simplificado)
```vue
<script setup lang="ts">
import { onMounted, ref, computed } from 'vue'
import { useRouter } from 'vue-router'
import { useSemanasStore } from '@/stores/semanas'
import { useToast } from '@/composables/useToast'
import DataTable from '@/components/DataTable.vue'
import { formatDate } from '@/utils/date'
import type { Column } from '@/types/common'

const router = useRouter()
const store = useSemanasStore()
const toast = useToast()

const semanas = ref<Semana[]>([])
const loading = ref(false)
const deletingId = ref<number | null>(null)

const columns: Column<Semana>[] = [
  { key: 'numeroSemana', label: 'Semana', render: (s) => `Sem ${s.numeroSemana}/${s.anio}` },
  { key: 'fechaInicio', label: 'Inicio', render: (s) => formatDate(s.fechaInicio) },
  { key: 'fechaFin', label: 'Fin', render: (s) => formatDate(s.fechaFin) },
  { key: 'estado', label: 'Estado', render: (s) => h('el-tag', { type: estadoType(s.estado) }, () => s.estado) },
  { key: 'insumosCount', label: 'Insumos', width: '100px' },
  { key: 'registrosTachoCount', label: 'Registros', width: '100px' },
  { 
    key: 'actions', 
    label: 'Acciones', 
    width: '180px',
    render: (s) => h('div', { class: 'actions-cell' }, [
      h('el-button', { size: 'small', type: 'primary', onClick: () => router.push({ name: 'detalle-semana', params: { id: s.id }}) }, 'Ver'),
      h('el-button', { size: 'small', onClick: () => router.push({ name: 'editar-semana', params: { id: s.id }}) }, 'Editar'),
      h('el-button', { size: 'small', type: 'danger', onClick: () => confirmDelete(s.id), disabled: deletingId.value === s.id }, deletingId.value === s.id ? '...' : 'Eliminar')
    ])
  }
]

const estadoType = (estado: string) => {
  switch (estado) {
    case 'Completada': return 'success'
    case 'EnProgreso': return 'info'
    case 'Planificada': return 'warning'
    case 'Cancelada': return 'danger'
    default: return 'info'
  }
}

const cargarSemanas = async () => {
  loading.value = true
  try {
    semanas.value = await store.fetchAll()
  } catch (e: any) {
    toast.error('Error al cargar semanas')
  } finally {
    loading.value = false
  }
}

const confirmDelete = async (id: number) => {
  const confirmed = await ElMessageBox.confirm('¿Eliminar esta semana? Se perderán todos sus datos asociados.', 'Confirmar', {
    confirmButtonText: 'Eliminar',
    cancelButtonText: 'Cancelar',
    type: 'warning'
  }).catch(() => false)
  
  if (!confirmed) return
  
  deletingId.value = id
  try {
    await store.delete(id)
    toast.success('Semana eliminada')
    await cargarSemanas()
  } catch (e: any) {
    toast.error(e.message || 'Error al eliminar')
  } finally {
    deletingId.value = null
  }
}

onMounted(cargarSemanas)
</script>

<template>
  <div class="section">
    <div class="section__header">
      <h1>Semanas de Producción</h1>
      <router-link to="/semanas/nueva" class="btn btn-primary">
        <PlusIcon /> Nueva Semana
      </router-link>
    </div>
    
    <DataTable
      :items="semanas"
      :columns="columns"
      :loading="loading"
      emptyMessage="No hay semanas registradas"
    />
  </div>
</template>
```

---

## Patrones de Vista Comunes

### Vista CRUD Estándar
```vue
<!-- Estructura base para Nueva/Editar -->
<script setup lang="ts">
import { ref, reactive, onMounted } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { useToast } from '@/composables/useToast'
import FormField from '@/components/FormField.vue'
import Modal from '@/components/Modal.vue'

const router = useRouter()
const route = useRoute()
const toast = useToast()
const store = useXxxStore() // semanas, insumos, stock, etc.

const isEditing = computed(() => !!route.params.id)
const loading = ref(false)
const submitting = ref(false)

const form = reactive({
  // campos...
})

const errors = reactive<Record<string, string>>({})

const validate = () => { /* ... */ return true }

const cargarDatos = async () => {
  if (!isEditing.value) return
  loading.value = true
  try {
    const data = await store.fetchById(Number(route.params.id))
    Object.assign(form, data)
  } catch (e) { toast.error('Error al cargar') }
  finally { loading.value = false }
}

const handleSubmit = async () => {
  if (!validate()) return
  submitting.value = true
  try {
    if (isEditing.value) {
      await store.update(Number(route.params.id), form)
      toast.success('Actualizado correctamente')
    } else {
      await store.create(form)
      toast.success('Creado correctamente')
    }
    router.push({ name: 'xxx-list' }) // o detalle
  } catch (e: any) {
    toast.error(e.message || 'Error al guardar')
  } finally {
    submitting.value = false
  }
}

onMounted(cargarDatos)
</script>

<template>
  <div class="section">
    <div class="section__header">
      <h1>{{ isEditing ? 'Editar' : 'Nuevo' }} {{ entidad }}</h1>
      <router-link :to="{ name: 'xxx-list' }" class="btn btn-secondary">Volver</router-link>
    </div>
    
    <div class="card">
      <form @submit.prevent="handleSubmit" class="form" v-if="!loading">
        <div class="form-grid">
          <FormField label="Campo 1" v-model="form.campo1" :error="errors.campo1" required />
          <FormField label="Campo 2" v-model="form.campo2" :error="errors.campo2" />
          <!-- más campos -->
        </div>
        
        <div class="form-actions">
          <button type="button" @click="router.back()" class="btn btn-secondary">Cancelar</button>
          <button type="submit" class="btn btn-primary" :disabled="submitting">
            <span v-if="submitting">Guardando...</span>
            <span v-else>{{ isEditing ? 'Actualizar' : 'Crear' }}</span>
          </button>
        </div>
      </form>
      
      <div v-else class="loading-center"><Spinner /></div>
    </div>
  </div>
</template>
```