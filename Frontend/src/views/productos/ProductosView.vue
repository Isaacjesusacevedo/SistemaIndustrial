<!-- views/productos/ProductosView.vue -->
<script setup lang="ts">
import { onMounted, ref, h } from 'vue'
import { useRouter } from 'vue-router'
import { useProductosCatalogoStore } from '@/stores/productosCatalogo'
import { useToast } from '@/composables/useToast'
import DataTable from '@/components/DataTable.vue'
import { formatNumber } from '@/utils/number'
import { ElMessageBox, ElTag, ElButton } from 'element-plus'
import { Plus } from '@element-plus/icons-vue'
import type { Column } from '@/types/common'
import type { ProductoCatalogo } from '@/types/productoCatalogo'

const router = useRouter()
const store = useProductosCatalogoStore()
const toast = useToast()

const loading = ref(false)

// ── Navegación ─────────────────────────────────────────────────────
const irANuevo = () =>
  router.push({ name: 'nuevo-producto' })

const editProducto = (id: number) =>
  router.push({ name: 'editar-producto', params: { id } })

// ── Eliminar ───────────────────────────────────────────────────────
const confirmDelete = async (id: number) => {
  const confirmed = await ElMessageBox.confirm(
    '¿Eliminar este producto del catálogo?',
    'Confirmar',
    { confirmButtonText: 'Eliminar', cancelButtonText: 'Cancelar', type: 'warning' }
  ).catch(() => false)

  if (!confirmed) return

  try {
    await store.delete(id)       // ✅ el store expone `delete`
    toast.success('Producto eliminado')
  } catch (e: any) {
    toast.error(e.message || 'Error al eliminar')
  }
}

// ── Columnas ────────────────────────────────────────────────────────
const columns: Column[] = [
  { key: 'nombre', label: 'Producto' },
  { key: 'unidadMedida', label: 'Unidad' },
  { key: 'pedidoSemanalAproximado', label: 'Pedido sem. aprox.', render: (p: ProductoCatalogo) => formatNumber(p.pedidoSemanalAproximado) },
  { key: 'promedioPorTacho', label: 'Promedio/tacho', render: (p: ProductoCatalogo) => formatNumber(p.promedioPorTacho) },
  {
    key: 'activo',
    label: 'Estado',
    render: (p: ProductoCatalogo) => h(ElTag, { type: p.activo ? 'success' : 'info', size: 'small' }, () => p.activo ? 'Activo' : 'Inactivo')
  },
  {
    key: 'actions',
    label: 'Acciones',
    width: '200px',
    render: (p: ProductoCatalogo) => h('div', { class: 'actions-cell' }, [
      h(ElButton, {
        size: 'small',
        type: 'primary',
        onClick: () => editProducto(p.id)
      }, () => 'Editar'),
      h(ElButton, {
        size: 'small',
        type: 'danger',
        onClick: () => confirmDelete(p.id)
      }, () => 'Eliminar')
    ])
  }
]

// ── Carga ───────────────────────────────────────────────────────────
const cargarProductos = async () => {
  loading.value = true
  try {
    await store.fetchAll()
  } catch (e: any) {
    toast.error(e.message || 'Error al cargar productos')
  } finally {
    loading.value = false
  }
}

onMounted(cargarProductos)
</script>

<template>
  <div class="section">
    <div class="section__header">
      <div>
        <h1>Catálogo de Productos</h1>
        <p class="section__subtitle">Definí cada producto una vez y reutilizalo semana a semana</p>
      </div>
      <el-button
        type="primary"
        @click="irANuevo"
        :disabled="loading"
        :icon="Plus">
        Nuevo Producto
      </el-button>
    </div>

    <DataTable
      :items="store.items"
      :columns="columns"
      :loading="loading"
      empty-message="No hay productos en el catálogo"
    />
  </div>
</template>
