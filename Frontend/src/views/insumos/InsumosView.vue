<!-- views/insumos/InsumosView.vue -->
<script setup lang="ts">
import { computed, onMounted, ref, h } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useInsumosStore } from '@/stores/insumos'
import { useToast } from '@/composables/useToast'
import DataTable from '@/components/DataTable.vue'
import { formatNumber } from '@/utils/number'
import { formatDate } from '@/utils/date'
import { ElMessageBox } from 'element-plus'
import { Plus } from '@element-plus/icons-vue'
import type { Column } from '@/types/common'
import type { InsumoResponse } from '@/types/insumo'

const route = useRoute()
const router = useRouter()
const store = useInsumosStore()
const toast = useToast()

const semanaId = computed(() => Number(route.params.semanaId))
const loading = ref(false)

// ── Navegación ─────────────────────────────────────────────────────
const irANuevo = () =>
  router.push({ name: 'nuevo-insumo', params: { semanaId: semanaId.value } })

const editInsumo = (id: number) =>
  router.push({ name: 'editar-insumo', params: { semanaId: semanaId.value, id } })

// ── Eliminar ───────────────────────────────────────────────────────
const confirmDelete = async (id: number) => {
  const confirmed = await ElMessageBox.confirm(
    '¿Eliminar este insumo?',
    'Confirmar',
    { confirmButtonText: 'Eliminar', cancelButtonText: 'Cancelar', type: 'warning' }
  ).catch(() => false)

  if (!confirmed) return

  try {
    await store.delete(id)          // ✅ el store expone `delete`
    toast.success('Insumo eliminado')
  } catch (e: any) {
    toast.error(e.message || 'Error al eliminar')
  }
}

// ── Columnas ────────────────────────────────────────────────────────
const columns: Column[] = [
  { key: 'nombre',         label: 'Insumo' },
  { key: 'unidadMedida',   label: 'Unidad',    width: '100px' },
  { key: 'cantidadRequerida', label: 'Requerido', width: '120px',
    render: (i: InsumoResponse) => formatNumber(i.cantidadRequerida) },
  { key: 'cantidadConsumida', label: 'Consumido', width: '120px',
    render: (i: InsumoResponse) => formatNumber(i.cantidadConsumida) },
  { key: 'stockDisponible', label: 'Stock',     width: '120px',
    render: (i: InsumoResponse) => formatNumber(i.stockDisponible) },
  { key: 'fechaCreacion',  label: 'Creado',
    render: (i: InsumoResponse) => formatDate(i.fechaCreacion) },
  {
    key: 'actions',
    label: 'Acciones',
    width: '200px',
    render: (i: InsumoResponse) => h('div', { class: 'actions-cell' }, [
      h('el-button', {
        size: 'small', type: 'primary',
        onClick: () => editInsumo(i.id)
      }, () => 'Editar'),
      h('el-button', {
        size: 'small', type: 'danger',
        onClick: () => confirmDelete(i.id)
      }, () => 'Eliminar')
    ])
  }
]

// ── Carga ───────────────────────────────────────────────────────────
const cargarInsumos = async () => {
  loading.value = true
  try {
    await store.fetchBySemana(semanaId.value)
  } catch (e: any) {
    toast.error(e.message || 'Error al cargar insumos')
  } finally {
    loading.value = false
  }
}

onMounted(cargarInsumos)
</script>

<template>
  <div class="section">
    <div class="section__header">
      <h1>Insumos</h1>
      <el-button
        type="primary"
        @click="irANuevo"
        :disabled="loading"
        :icon="Plus">
        Nuevo Insumo
      </el-button>
    </div>

    <DataTable
      :items="store.items"
      :columns="columns"
      :loading="loading"
      empty-message="No hay insumos registrados para esta semana"
    />
  </div>
</template>

