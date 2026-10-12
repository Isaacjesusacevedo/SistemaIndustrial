<!-- views/tirado/TiradoView.vue -->
<script setup lang="ts">
import { ref, computed, watch, onMounted } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { useTiradoProductosStore } from '@/stores/tiradoProductos'
import { useCategoriasStore } from '@/stores/categorias'
import { useProductosCatalogoStore } from '@/stores/productosCatalogo'
import { useToast } from '@/composables/useToast'
import { useResponsive } from '@/composables/useResponsive'
import { ElMessageBox } from 'element-plus'
import FormField from '@/components/FormField.vue'
import { Plus, Edit, Delete, Check, ArrowLeft } from '@element-plus/icons-vue'
import type { FiltroTirado, TiradoProducto } from '@/types/tiradoProducto'
import type { Categoria } from '@/types/categoria'
import { UNIDADES_PRODUCTO } from '@/constants/unidades'

const router = useRouter()
const route = useRoute()
const tiradoStore = useTiradoProductosStore()
const categoriasStore = useCategoriasStore()
const productosCatalogoStore = useProductosCatalogoStore()
const toast = useToast()
const { isMobile } = useResponsive()

const semanaId = computed(() => Number(route.params.semanaId))

// ── Modal crear/editar ──────────────────────────────────────────────
const showModal = ref(false)
const submitting = ref(false)
const editingId = ref<number | null>(null)

const form = ref({
  nombre: '',
  unidadMedida: '',
  pedidoSemanal: 0,
  stockInicial: 0,
  promedioPorTacho: 0,
  categoriaId: null as number | null,
  observaciones: '',
  cantidadTirada: 0,
})

const errors = ref<Record<string, string>>({})
const unidades = [...UNIDADES_PRODUCTO]

// ── Cantidades temporales para cada fila (input en la tabla) ─────────
const tiradasTemp = ref<Record<number, number | string>>({})

// ── Filtros ─────────────────────────────────────────────────────────
const filtros: { key: FiltroTirado; label: string }[] = [
  { key: 'totales',   label: 'Todos' },
  { key: 'faltantes', label: 'Faltantes' },
  { key: 'sobrantes', label: 'Sobrantes' },
]

// ── Carga ───────────────────────────────────────────────────────────
const cargarProductos = async () => {
  if (!semanaId.value) return
  try {
    await Promise.all([
      tiradoStore.cargarPorSemana(semanaId.value),
      categoriasStore.cargarTodas(),
      productosCatalogoStore.fetchActivos(),
    ])
  } catch (e: any) {
    toast.error(e.message || 'Error al cargar productos')
  }
}

// ── Producto del catálogo seleccionado (modo crear) ─────────────────
const productoSelCatalogoId = ref<number | null>(null)

// El select emite string: normalizar a number
const productoSelId = computed(() => {
  const n = Number(productoSelCatalogoId.value)
  return Number.isFinite(n) && n > 0 ? n : null
})

const productoSelCatalogo = computed(() =>
  productosCatalogoStore.activos.find(p => p.id === productoSelId.value) ?? null
)

const productoYaEnSemana = computed(() => {
  if (!productoSelCatalogo.value) return false
  return tiradoStore.items.some(
    item => item.nombre === productoSelCatalogo.value!.nombre
  )
})

watch(productoSelId, (newId) => {
  if (!newId) return
  const p = productosCatalogoStore.activos.find(x => x.id === newId)
  if (p) {
    form.value.nombre = p.nombre
    form.value.unidadMedida = p.unidadMedida
    form.value.pedidoSemanal = p.pedidoSemanalAproximado
    form.value.promedioPorTacho = p.promedioPorTacho
  }
})

// ── Validación ─────────────────────────────────────────────────────
const validate = () => {
  errors.value = {}

  // Observaciones siempre
  if (form.value.observaciones && form.value.observaciones.length > 500) {
    errors.value.observaciones = 'Máximo 500 caracteres'
  }

  // Stock inicial siempre
  const stock = Number(form.value.stockInicial)
  if (isNaN(stock) || stock < 0) {
    errors.value.stockInicial = 'El stock inicial no puede ser negativo'
  }

  if (editingId.value) {
    // MODO EDITAR: solo cantidad tirada
    const tirada = Number(form.value.cantidadTirada)
    if (isNaN(tirada) || tirada < 0) {
      errors.value.cantidadTirada = 'La cantidad tirada no puede ser negativa'
    }
  } else {
    // MODO CREAR: producto seleccionado y no duplicado
    if (!productoSelId.value) {
      errors.value.nombre = 'Debés seleccionar un producto'
    } else if (productoYaEnSemana.value) {
      errors.value.nombre = 'Este producto ya está en la semana'
    }
  }

  return Object.keys(errors.value).length === 0
}

// ── Abrir modales ───────────────────────────────────────────────────
const abrirModalCrear = () => {
  form.value = {
    nombre: '', unidadMedida: '', pedidoSemanal: 0, stockInicial: 0,
    promedioPorTacho: 0, categoriaId: null, observaciones: '', cantidadTirada: 0,
  }
  productoSelCatalogoId.value = null
  errors.value = {}
  editingId.value = null
  showModal.value = true
}

const abrirModalEditar = (item: TiradoProducto) => {
  form.value = {
    nombre: item.nombre,
    unidadMedida: item.unidadMedida,
    pedidoSemanal: item.pedidoSemanal,
    stockInicial: item.stockInicial,
    promedioPorTacho: item.promedioPorTacho,
    categoriaId: item.categoriaId,
    observaciones: item.observaciones ?? '',
    cantidadTirada: item.cantidadTirada,
  }
  errors.value = {}
  editingId.value = item.id
  showModal.value = true
}

const cerrarModal = () => {
  showModal.value = false
  editingId.value = null
  productoSelCatalogoId.value = null
  errors.value = {}
}

// ── Guardar producto ────────────────────────────────────────────────
const handleSubmit = async () => {
  if (!validate()) return
  submitting.value = true
  try {
    if (editingId.value) {
      await tiradoStore.actualizar(editingId.value, {
        ...form.value,
        stockInicial: Number(form.value.stockInicial),
        cantidadTirada: Number(form.value.cantidadTirada),
      })
      toast.success('Producto actualizado correctamente')
    } else {
      await tiradoStore.crear({
        semanaId: semanaId.value,
        nombre: form.value.nombre,
        unidadMedida: form.value.unidadMedida,
        pedidoSemanal: Number(form.value.pedidoSemanal),
        stockInicial: Number(form.value.stockInicial),
        promedioPorTacho: Number(form.value.promedioPorTacho),
        categoriaId: form.value.categoriaId,
        observaciones: form.value.observaciones || null,
      })
      toast.success('Producto agregado a la semana')
    }
    cerrarModal()
  } catch (e: any) {
    toast.error(e.message || 'Error al guardar')
  } finally {
    submitting.value = false
  }
}

// ── Guardar tirada desde la tabla ─────────────────────────────────────
const guardarTirada = async (row: TiradoProducto) => {
  const cantidad = Number(tiradasTemp.value[row.id])

  if (!cantidad || cantidad <= 0) {
    toast.error('Ingresá una cantidad válida')
    return
  }

  try {
    await tiradoStore.registrarTirada(row.id, cantidad, undefined)
    toast.success(`Tirada de ${cantidad} registrada`)
    tiradasTemp.value[row.id] = ''
  } catch (e: any) {
    toast.error(e.message || 'Error al registrar tirada')
  }
}

// ── Eliminar ────────────────────────────────────────────────────────
const confirmarEliminar = async (id: number) => {
  const confirmed = await ElMessageBox.confirm(
    '¿Eliminar este producto? Se perderán sus tiradas registradas.',
    'Confirmar',
    { confirmButtonText: 'Eliminar', cancelButtonText: 'Cancelar', type: 'warning' }
  ).catch(() => false)

  if (!confirmed) return

  try {
    await tiradoStore.eliminar(id)
    toast.success('Producto eliminado')
  } catch (e: any) {
    toast.error(e.message || 'Error al eliminar')
  }
}

// ── Cambio de filtro ────────────────────────────────────────────────
const cambiarFiltro = (nuevo: FiltroTirado) => {
  tiradoStore.cambiarFiltro(nuevo)
}

// ── Estado (solo según Stock Total) ───────────────────────────────────
const getEstado = (item: TiradoProducto): 'Falta' | 'Sobra' => {
  return item.stockTotal < 0 ? 'Falta' : 'Sobra'
}

const getEstadoType = (estado: string): 'danger' | 'success' => {
  return estado === 'Falta' ? 'danger' : 'success'
}

const formatNum = (n: number) => n.toLocaleString('es-AR')

const categoriaNombreEdicion = computed(() =>
  categoriasStore.activas.find((c: Categoria) => c.id === form.value.categoriaId)?.nombre ?? 'Sin categoría'
)

onMounted(cargarProductos)
</script>

<template>
  <div class="section">
    <div class="section__header">
      <div>
        <h1>Sector Tirado</h1>
        <p class="section__subtitle">Semana {{ semanaId }}</p>
      </div>
      <div class="section__actions" style="flex-shrink: 0; gap: 8px;">
        <router-link
          :to="{ name: 'armado', params: { semanaId } }"
          class="btn btn-secondary"
          style="white-space: nowrap;">
          <ArrowLeft /> Volver
        </router-link>
        <router-link
          :to="{ name: 'tachos-semana', params: { semanaId } }"
          class="btn btn-secondary"
          style="white-space: nowrap;">
          Ver Tachos Semana
        </router-link>
        <el-button
          type="primary"
          @click="abrirModalCrear"
          :disabled="!semanaId"
          :icon="Plus"
          style="white-space: nowrap;">
          Nuevo Producto
        </el-button>
      </div>
    </div>

    <!-- Filtros -->
    <div style="margin-bottom: var(--spacing-md);">
      <el-radio-group :model-value="tiradoStore.filtro" @update:model-value="cambiarFiltro">
        <el-radio-button v-for="f in filtros" :key="f.key" :value="f.key">
          {{ f.label }}
        </el-radio-button>
      </el-radio-group>
    </div>

    <div v-if="tiradoStore.loading" class="loading-center"><Spinner /></div>

    <div v-else class="card">
      <el-table :data="tiradoStore.filtrados" border stripe style="width: 100%">
        <el-table-column prop="nombre" label="Producto" min-width="140" />
        <el-table-column prop="unidadMedida" label="Unidad" width="80" />

        <el-table-column label="Pedido" width="70" align="right">
          <template #default="{ row }">{{ formatNum(row.pedidoSemanal) }}</template>
        </el-table-column>

        <el-table-column label="Stock Ini." width="90" align="right">
          <template #default="{ row }">{{ formatNum(row.stockInicial) }}</template>
        </el-table-column>

        <el-table-column label="Tirado" width="80" align="right">
          <template #default="{ row }">
            <span style="font-family: var(--font-mono); font-weight:600;">
              {{ formatNum(row.cantidadTirada) }}
            </span>
          </template>
        </el-table-column>

        <el-table-column label="Tachos" width="70" align="center">
          <template #default="{ row }">
            <span class="badge">{{ row.cantidadTachos }}</span>
          </template>
        </el-table-column>

        <el-table-column label="Stock Total" width="100" align="right">
          <template #default="{ row }">
            <span style="font-family: var(--font-mono); font-weight:600;"
              :class="row.stockTotal < 0 ? 'text-danger' : row.stockTotal === 0 ? 'text-success' : 'text-warning'">
              {{ formatNum(row.stockTotal) }}
            </span>
          </template>
        </el-table-column>

        <el-table-column v-if="!isMobile" label="Tachos Est." width="95" align="right">
          <template #default="{ row }">
            <span style="font-family: var(--font-mono);"
              :class="row.tachosEstimados < 0 ? 'text-danger' : row.tachosEstimados === 0 ? 'text-success' : 'text-warning'">
              {{ row.tachosEstimados.toFixed(2) }}
            </span>
          </template>
        </el-table-column>

        <el-table-column label="Estado" width="110">
          <template #default="{ row }">
            <el-tag :type="getEstadoType(getEstado(row))" size="small">
              {{ getEstado(row) }}
            </el-tag>
          </template>
        </el-table-column>

        <el-table-column label="Acciones" width="200" fixed="right">
          <template #default="{ row }">
            <div style="display:flex; gap:4px; align-items:center; justify-content:flex-end;">
              <!-- Input para cargar la cantidad -->
              <el-input
                v-model="tiradasTemp[row.id]"
                size="small"
                type="number"
                placeholder="0"
                style="width: 65px;"
                :min="0"
              />

              <!-- Botón Guardar (verde) -->
              <el-button
                size="small"
                type="success"
                :icon="Check"
                circle
                @click="guardarTirada(row)"
                :disabled="!tiradasTemp[row.id] || Number(tiradasTemp[row.id]) <= 0"
                title="Guardar tirada" />

              <!-- Botón Editar (azul) -->
              <el-button
                size="small"
                type="primary"
                :icon="Edit"
                circle
                @click="abrirModalEditar(row)"
                title="Editar producto" />

              <!-- Botón Eliminar (rojo) -->
              <el-button
                size="small"
                type="danger"
                :icon="Delete"
                circle
                @click="confirmarEliminar(row.id)"
                title="Eliminar producto" />
            </div>
          </template>
        </el-table-column>
      </el-table>

      <div
        v-if="tiradoStore.filtrados.length === 0"
        style="text-align:center; padding:32px; color:var(--text-3)">
        No hay productos
        {{ tiradoStore.filtro !== 'totales' ? `con ${tiradoStore.filtro}` : 'registrados' }}
        para esta semana
      </div>
    </div>

    <!-- Modal crear/editar producto -->
    <el-dialog
      v-model="showModal"
      :title="editingId ? 'Editar Producto' : 'Nuevo Producto'"
      width="650px"
      :fullscreen="isMobile"
      :before-close="cerrarModal">
      <form @submit.prevent="handleSubmit" class="form">
        <!-- Ficha de solo lectura en edición -->
        <div
          v-if="editingId"
          style="border-left: 3px solid var(--accent); padding: 12px 16px; background: var(--bg-1); border-radius: 0 var(--radius-md) var(--radius-md) 0; margin-bottom: var(--spacing-md);">
          <div style="font-family: var(--font-mono); font-size: 11px; letter-spacing: 0.1em; text-transform: uppercase; color: var(--text-2); margin-bottom: 4px;">
            Producto
          </div>
          <div style="font-size: 1.25rem; font-weight: 700; color: var(--text-0); margin-bottom: 12px;">
            {{ form.nombre }}
          </div>
          <div style="display: grid; grid-template-columns: 1fr 1fr; gap: 12px 16px;">
            <div>
              <div style="font-family: var(--font-mono); font-size: 11px; letter-spacing: 0.08em; text-transform: uppercase; color: var(--text-2); margin-bottom: 2px;">
                Unidad
              </div>
              <div style="color: var(--text-0);">{{ form.unidadMedida }}</div>
            </div>
            <div>
              <div style="font-family: var(--font-mono); font-size: 11px; letter-spacing: 0.08em; text-transform: uppercase; color: var(--text-2); margin-bottom: 2px;">
                Pedido semanal
              </div>
              <div style="color: var(--text-0);">{{ formatNum(Number(form.pedidoSemanal)) }}</div>
            </div>
            <div>
              <div style="font-family: var(--font-mono); font-size: 11px; letter-spacing: 0.08em; text-transform: uppercase; color: var(--text-2); margin-bottom: 2px;">
                Promedio/tacho
              </div>
              <div style="color: var(--text-0);">{{ formatNum(Number(form.promedioPorTacho)) }}</div>
            </div>
            <div>
              <div style="font-family: var(--font-mono); font-size: 11px; letter-spacing: 0.08em; text-transform: uppercase; color: var(--text-2); margin-bottom: 2px;">
                Categoría
              </div>
              <div style="color: var(--text-0);">{{ categoriaNombreEdicion }}</div>
            </div>
          </div>
        </div>

        <template v-if="!editingId">
          <!-- Selector de producto del catálogo -->
          <FormField
            label="Producto"
            v-model="productoSelCatalogoId"
            type="select"
            :options="productosCatalogoStore.activos.map(p => ({
              value: p.id,
              label: `${p.nombre} · ${p.unidadMedida}`
            }))"
            :error="errors.nombre"
            placeholder="Seleccionar producto del catálogo"
            hint="Los productos se administran en el Catálogo de Productos"
            required />

          <!-- Aviso si el catálogo está vacío -->
          <div
            v-if="productosCatalogoStore.activos.length === 0"
            style="padding: 12px; background: var(--amber-dim); border-radius: var(--radius-md);
                   border-left: 3px solid var(--amber); margin-bottom: var(--spacing-md);">
            <p style="margin: 0; color: var(--amber); font-size: 0.875rem;">
              No hay productos en el catálogo.
              <router-link to="/productos" style="color: var(--amber); text-decoration: underline;">
                Crear productos primero
              </router-link>
            </p>
          </div>

          <!-- Aviso si el producto ya está en la semana -->
          <div
            v-if="productoYaEnSemana"
            style="padding: 12px; background: var(--red-dim); border-radius: var(--radius-md);
                   border-left: 3px solid var(--red); margin-bottom: var(--spacing-md);">
            <p style="margin: 0; color: var(--red); font-size: 0.875rem;">
              Este producto ya está cargado en esta semana. No se puede duplicar.
            </p>
          </div>

          <!-- Campos auto-completados (readonly) -->
          <div v-if="productoSelCatalogo" style="margin-top: var(--spacing-md); padding: var(--spacing-md);
                    background: var(--bg-3); border-radius: var(--radius-md);
                    border-left: 3px solid var(--accent);">
            <div style="font-family: var(--font-mono); font-size: 10px; color: var(--text-3);
                        letter-spacing: 0.1em; text-transform: uppercase; margin-bottom: 8px;">
              Datos del producto (solo lectura)
            </div>
            <div style="display: grid; grid-template-columns: 1fr 1fr; gap: 12px 16px;">
              <div>
                <div style="font-size: 10px; color: var(--text-3); text-transform: uppercase;
                            letter-spacing: 0.05em; margin-bottom: 2px;">
                  Unidad
                </div>
                <div style="color: var(--text-0); font-size: 0.875rem;">
                  {{ productoSelCatalogo.unidadMedida }}
                </div>
              </div>
              <div>
                <div style="font-size: 10px; color: var(--text-3); text-transform: uppercase;
                            letter-spacing: 0.05em; margin-bottom: 2px;">
                  Pedido semanal
                </div>
                <div style="color: var(--text-0); font-size: 0.875rem;
                            font-family: var(--font-mono);">
                  {{ formatNum(productoSelCatalogo.pedidoSemanalAproximado) }}
                </div>
              </div>
              <div>
                <div style="font-size: 10px; color: var(--text-3); text-transform: uppercase;
                            letter-spacing: 0.05em; margin-bottom: 2px;">
                  Promedio por tacho
                </div>
                <div style="color: var(--text-0); font-size: 0.875rem;
                            font-family: var(--font-mono);">
                  {{ formatNum(productoSelCatalogo.promedioPorTacho) }}
                </div>
              </div>
            </div>
          </div>

          <!-- Solo Stock Inicial es editable -->
          <div class="form-grid" style="margin-top: var(--spacing-md);">
            <FormField
              label="Stock Inicial"
              v-model="form.stockInicial"
              type="number"
              :min="0"
              :step="1"
              :error="errors.stockInicial"
              hint="Stock al inicio de esta semana"
              required />
          </div>
        </template>

        <template v-else>
          <div class="form-grid">
            <FormField
              label="Stock Inicial"
              v-model="form.stockInicial"
              type="number"
              :min="0"
              :step="0.01"
              :error="errors.stockInicial"
              required />

            <FormField
              label="Cantidad Tirada"
              v-model="form.cantidadTirada"
              type="number"
              :min="0"
              :step="0.01"
              :error="errors.cantidadTirada" />
          </div>

          <div style="display:flex; gap:8px; margin-bottom: var(--spacing-md);">
            <el-button size="small" @click="form.stockInicial = 0">
              Resetear stock
            </el-button>
            <el-button size="small" @click="form.cantidadTirada = 0">
              Resetear tirada a 0
            </el-button>
          </div>
        </template>

        <FormField
          label="Observaciones"
          v-model="form.observaciones"
          type="textarea"
          :error="errors.observaciones"
          placeholder="Observaciones opcionales"
          :rows="3" />

        <div class="form-actions">
          <button type="button" class="btn btn-secondary" @click="cerrarModal">Cancelar</button>
          <button type="submit" class="btn btn-primary" :disabled="submitting || (!editingId && productoYaEnSemana)">
            <span v-if="submitting">Guardando...</span>
            <span v-else>{{ editingId ? 'Actualizar' : 'Crear' }}</span>
          </button>
        </div>
      </form>
    </el-dialog>
  </div>
</template>

<style scoped src="@/assets/styles/views/tirado.css"></style>

<style scoped>
/* Forzar tema oscuro en el-table */
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
  padding: 8px 8px;
}

:deep(.el-table td.el-table__cell) {
  background: transparent;
  color: var(--text-1);
  border-bottom: 1px solid var(--border);
  padding: 6px 8px;
  font-size: 13.5px;
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

:deep(.el-table .el-input) {
  --el-input-bg-color: var(--bg-1);
  --el-input-border-color: var(--border);
  --el-input-text-color: var(--text-0);
}

:deep(.el-table .el-input__wrapper) {
  padding: 2px 8px;
}

:deep(.el-table .el-button.is-circle) {
  padding: 6px;
  width: 28px;
  height: 28px;
}

:deep(.el-table__inner-wrapper::before),
:deep(.el-table__inner-wrapper::after) {
  display: none;
}

:deep(.el-table--border),
:deep(.el-table--border .el-table__cell) {
  border-color: var(--border);
}
</style>