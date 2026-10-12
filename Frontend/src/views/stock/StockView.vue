<!-- views/stock/StockView.vue -->
<script setup lang="ts">
import { computed, onMounted, ref, h } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useStockStore } from '@/stores/stock'
import { useToast } from '@/composables/useToast'
import DataTable from '@/components/DataTable.vue'
import { formatNumber } from '@/utils/number'
import { formatDate } from '@/utils/date'
import { ElMessageBox } from 'element-plus'
import { Plus } from '@element-plus/icons-vue'
import type { Column } from '@/types/common'
import type { StockResponse } from '@/types/stock'

const route = useRoute()
const router = useRouter()
const store = useStockStore()
const toast = useToast()

const semanaId = computed(() => Number(route.params.semanaId))
const loading = ref(false)

// ── Navegación ─────────────────────────────────────────────────────
const irANuevo = () =>
  router.push({ name: 'nuevo-stock', params: { semanaId: semanaId.value } })

const editStock = (id: number) =>
  router.push({ name: 'editar-stock', params: { semanaId: semanaId.value, id } })

// ── Eliminar ───────────────────────────────────────────────────────
const confirmDelete = async (id: number) => {
  const confirmed = await ElMessageBox.confirm(
    '¿Eliminar este registro de stock?',
    'Confirmar',
    { confirmButtonText: 'Eliminar', cancelButtonText: 'Cancelar', type: 'warning' }
  ).catch(() => false)

  if (!confirmed) return

  try {
    await store.delete(id)          // ✅ el store expone `delete`
    toast.success('Stock eliminado')
  } catch (e: any) {
    toast.error(e.message || 'Error al eliminar')
  }
}

// ── Columnas ────────────────────────────────────────────────────────
const columns: Column[] = [
  { key: 'producto',      label: 'Producto' },
  { key: 'unidadMedida',  label: 'Unidad',   width: '100px' },
  { key: 'stockInicial',  label: 'Inicial',  width: '120px', render: (s: StockResponse) => formatNumber(s.stockInicial) },
  { key: 'entradas',      label: 'Entradas', width: '120px', render: (s: StockResponse) => formatNumber(s.entradas) },
  { key: 'salidas',       label: 'Salidas',  width: '120px', render: (s: StockResponse) => formatNumber(s.salidas) },
  { key: 'stockFinal',    label: 'Final',    width: '120px', render: (s: StockResponse) => formatNumber(s.stockFinal) },
  { key: 'fechaCreacion', label: 'Creado',   render: (s: StockResponse) => formatDate(s.fechaCreacion) },
  {
    key: 'actions',
    label: 'Acciones',
    width: '200px',
    render: (s: StockResponse) => h('div', { class: 'actions-cell' }, [
      h('el-button', {
        size: 'small',
        type: 'primary',
        onClick: () => editStock(s.id)
      }, () => 'Editar'),
      h('el-button', {
        size: 'small',
        type: 'danger',
        onClick: () => confirmDelete(s.id)
      }, () => 'Eliminar')
    ])
  }
]

// ── Carga ───────────────────────────────────────────────────────────
const cargarStock = async () => {
  loading.value = true
  try {
    await store.syncFromTirado(semanaId.value)
  } catch (e: any) {
    try {
      await store.fetchBySemana(semanaId.value)
    } catch {
      toast.error(e.message || 'Error al cargar stock')
    }
  } finally {
    loading.value = false
  }
}

onMounted(cargarStock)
</script>

<template>
  <div class="section">
    <div class="section__header">
      <h1>Stock</h1>
      <el-button
        type="primary"
        @click="irANuevo"
        :disabled="loading"
        :icon="Plus">
        Nuevo Stock
      </el-button>
    </div>

    <DataTable
      :items="store.items"
      :columns="columns"
      :loading="loading"
      empty-message="No hay stock registrado para esta semana"
    />
  </div>
</template>

