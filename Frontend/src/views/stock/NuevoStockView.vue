<!-- views/stock/NuevoStockView.vue -->
<script setup lang="ts">
import { ref, reactive, computed, watch, onMounted } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { useStockStore } from '@/stores/stock'
import { useProductosCatalogoStore } from '@/stores/productosCatalogo'
import { useToast } from '@/composables/useToast'
import FormField from '@/components/FormField.vue'

const router = useRouter()
const route = useRoute()
const store = useStockStore()
const productosStore = useProductosCatalogoStore()
const toast = useToast()

const semanaId  = computed(() => Number(route.params.semanaId))
const stockId   = computed(() => Number(route.params.id))
const isEditing = computed(() => !!route.params.id)

const loading = ref(false)
const submitting = ref(false)

const form = reactive({
  productoId: null as number | null,
  unidadMedida: '',
  stockInicial: 0,
  entradas: 0,
  salidas: 0
})

// ✅ ref en lugar de reactive
const errors = ref<Record<string, string>>({})

// El select emite string: normalizar a number para comparar con p.id
const selectedId = computed(() => {
  const n = Number(form.productoId)
  return Number.isFinite(n) && n > 0 ? n : null
})

const productoSeleccionado = computed(() =>
  productosStore.activos.find(p => p.id === selectedId.value) ?? null
)

watch(selectedId, (newId) => {
  if (!newId) {
    form.unidadMedida = ''
    return
  }
  const p = productosStore.activos.find(x => x.id === newId)
  if (p) {
    form.unidadMedida = p.unidadMedida
  }
})

const validate = () => {
  errors.value = {}
  if (!selectedId.value) {
    errors.value.productoId = 'El producto es requerido'
  }
  if (form.stockInicial < 0) errors.value.stockInicial = 'No puede ser negativo'
  if (isEditing.value && form.entradas < 0) errors.value.entradas = 'No puede ser negativo'
  if (isEditing.value && form.salidas  < 0) errors.value.salidas  = 'No puede ser negativo'
  return Object.keys(errors.value).length === 0
}

const cargarDatos = async () => {
  loading.value = true
  try {
    await productosStore.fetchActivos()
    if (!isEditing.value) return
    await store.fetchBySemana(semanaId.value)
    const item = store.items.find(s => s.id === stockId.value)

    if (!item) {
      toast.error('No se encontró el registro de stock')
      router.push({ name: 'stock', params: { semanaId: semanaId.value } })
      return
    }

    // Buscar el producto en el catálogo por nombre
    const producto = productosStore.activos.find(p => p.nombre === item.producto)
    form.productoId = producto?.id ?? null
    form.unidadMedida = item.unidadMedida
    form.stockInicial = item.stockInicial
    form.entradas = item.entradas
    form.salidas = item.salidas
  } catch (e: any) {
    toast.error(e.message || 'Error al cargar stock')
    router.push({ name: 'stock', params: { semanaId: semanaId.value } })
  } finally {
    loading.value = false
  }
}

const handleSubmit = async () => {
  if (!validate()) return

  const productoNombre = productoSeleccionado.value?.nombre
  if (!productoNombre) {
    toast.error('Producto no válido')
    return
  }

  submitting.value = true
  try {
    if (isEditing.value) {
      // ✅ StockUpdateDto: producto, unidadMedida, stockInicial, entradas, salidas
      await store.update(stockId.value, {
        producto: productoNombre,
        unidadMedida: form.unidadMedida,
        stockInicial: Number(form.stockInicial),
        entradas: Number(form.entradas),
        salidas: Number(form.salidas)
      })
      toast.success('Stock actualizado correctamente')
    } else {
      // ✅ StockCreateDto: solo semanaId, producto, unidadMedida, stockInicial
      await store.create({
        semanaId: semanaId.value,
        producto: productoNombre,
        unidadMedida: form.unidadMedida,
        stockInicial: Number(form.stockInicial)
      })
      toast.success('Stock creado correctamente')
    }
    router.push({ name: 'stock', params: { semanaId: semanaId.value } })
  } catch (error: any) {
    toast.error(error.message || 'Error al guardar')
  } finally {
    submitting.value = false
  }
}

onMounted(cargarDatos)
</script>

<template>
  <div class="section">
    <div class="section__header">
      <h1>{{ isEditing ? 'Editar' : 'Nuevo' }} Stock</h1>
      <router-link :to="{ name: 'stock', params: { semanaId } }" class="btn btn-secondary">
        Volver
      </router-link>
    </div>

    <div class="card">
      <form v-if="!loading" @submit.prevent="handleSubmit" class="form">
        <div v-if="productosStore.activos.length === 0" class="alert alert-error"
             style="margin-bottom: var(--spacing-md);">
          No hay productos en el catálogo.
          <router-link to="/productos" style="color:var(--accent); text-decoration:underline;">
            Crear productos
          </router-link>
        </div>

        <FormField
          label="Producto"
          v-model="form.productoId"
          type="select"
          :options="productosStore.activos.map(p => ({
            value: p.id,
            label: p.nombre
          }))"
          :error="errors.productoId"
          placeholder="Seleccionar producto"
          hint="Los productos se administran en el Catálogo de Productos"
          required />

        <div class="form-grid">
          <FormField
            label="Unidad de medida"
            v-model="form.unidadMedida"
            :error="errors.unidadMedida"
            disabled
            hint="Se completa automáticamente según el producto" />
        </div>

        <div class="form-grid">
          <FormField
            label="Stock inicial"
            v-model="form.stockInicial"
            type="number"
            :error="errors.stockInicial"
            :min="0"
            :step="0.01"
            required />

          <!-- ✅ Solo se muestran al editar (StockUpdateDto) -->
          <template v-if="isEditing">
            <FormField
              label="Entradas"
              v-model="form.entradas"
              type="number"
              :error="errors.entradas"
              :min="0"
              :step="0.01" />
            <FormField
              label="Salidas"
              v-model="form.salidas"
              type="number"
              :error="errors.salidas"
              :min="0"
              :step="0.01" />
          </template>
        </div>

        <div class="form-actions">
          <router-link :to="{ name: 'stock', params: { semanaId } }" class="btn btn-secondary">
            Cancelar
          </router-link>
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
