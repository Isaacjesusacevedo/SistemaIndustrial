<!-- components/DataTable.vue -->
<script setup lang="ts">
import { isVNode } from 'vue'

interface Column<T = any> {
  key: string
  label: string
  sortable?: boolean
  render?: (row: T) => any
  class?: string
  width?: string
}

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

// Si el render devuelve un VNode (ej: h('el-tag', ...)), hay que montarlo
// como componente; la interpolación {{ }} lo mostraría como "[object Object]".
const renderVNode = (item: any, column: Column) => () => renderCell(item, column)

function formatDate(date: Date | string): string {
  const d = new Date(date)
  return d.toLocaleDateString('es-ES', { day: '2-digit', month: '2-digit', year: 'numeric' })
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
              <component v-if="isVNode(renderCell(item, col))" :is="renderVNode(item, col)" />
              <template v-else>{{ renderCell(item, col) }}</template>
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

<style scoped src="@/assets/styles/components/table.css"></style>