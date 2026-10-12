<!-- views/productos/EditarProductoView.vue -->
<script setup lang="ts">
import { ref, reactive, computed, onMounted } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { useProductosCatalogoStore } from '@/stores/productosCatalogo'
import { useToast } from '@/composables/useToast'
import FormField from '@/components/FormField.vue'
import { UNIDADES_PRODUCTO } from '@/constants/unidades'

const router = useRouter()
const route = useRoute()
const store = useProductosCatalogoStore()
const toast = useToast()

const productoId = computed(() => Number(route.params.id))

const unidades = [...UNIDADES_PRODUCTO]

const loading = ref(false)
const submitting = ref(false)

const form = reactive({
  nombre: '',
  unidadMedida: '',
  pedidoSemanalAproximado: 0,
  promedioPorTacho: 0,
  activo: true,
  observaciones: ''
})

// ✅ ref en lugar de reactive
const errors = ref<Record<string, string>>({})

const validate = () => {
  errors.value = {}
  if (!form.nombre.trim()) {
    errors.value.nombre = 'El nombre es requerido'
  } else if (form.nombre.length > 200) {
    errors.value.nombre = 'Máximo 200 caracteres'
  }
  if (!form.unidadMedida) {
    errors.value.unidadMedida = 'La unidad de medida es requerida'
  }
  if (form.pedidoSemanalAproximado < 0) {
    errors.value.pedidoSemanalAproximado = 'No puede ser negativo'
  }
  if (form.promedioPorTacho < 0) {
    errors.value.promedioPorTacho = 'No puede ser negativo'
  }
  if (form.observaciones && form.observaciones.length > 500) {
    errors.value.observaciones = 'Máximo 500 caracteres'
  }
  return Object.keys(errors.value).length === 0
}

const cargarDatos = async () => {  loading.value = true
  try {
    const producto = await store.fetchById(productoId.value)

    if (!producto) {
      toast.error('No se encontró el producto')
      router.push({ name: 'productos' })
      return
    }

    Object.assign(form, {
      nombre: producto.nombre,
      unidadMedida: producto.unidadMedida,
      pedidoSemanalAproximado: producto.pedidoSemanalAproximado,
      promedioPorTacho: producto.promedioPorTacho,
      activo: producto.activo,
      observaciones: producto.observaciones ?? ''
    })
  } catch (e: any) {
    toast.error(e.message || 'Error al cargar producto')
    router.push({ name: 'productos' })
  } finally {
    loading.value = false
  }
}

const toBoolean = (v: unknown) => v === true || v === 'true'

const handleSubmit = async () => {
  if (!validate()) return
  submitting.value = true
  try {
    await store.update(productoId.value, {
      nombre: form.nombre.trim(),
      unidadMedida: form.unidadMedida,
      pedidoSemanalAproximado: Number(form.pedidoSemanalAproximado),
      promedioPorTacho: Number(form.promedioPorTacho),
      activo: toBoolean(form.activo),
      observaciones: form.observaciones.trim() || null
    })
    toast.success('Producto actualizado correctamente')
    router.push({ name: 'productos' })
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
      <h1>Editar Producto</h1>
      <router-link :to="{ name: 'productos' }" class="btn btn-secondary">
        Volver
      </router-link>
    </div>

    <div class="card">
      <form v-if="!loading" @submit.prevent="handleSubmit" class="form">
        <div class="form-grid">
          <FormField
            label="Nombre"
            v-model="form.nombre"
            :error="errors.nombre"
            placeholder="Ej: Detergente concentrado"
            required />
          <FormField
            label="Unidad de medida"
            v-model="form.unidadMedida"
            type="select"
            :error="errors.unidadMedida"
            :options="unidades.map(u => ({ value: u, label: u }))"
            placeholder="Seleccionar unidad"
            required />
        </div>

        <div class="form-grid">
          <FormField
            label="Pedido semanal aproximado"
            v-model="form.pedidoSemanalAproximado"
            type="number"
            :error="errors.pedidoSemanalAproximado"
            :min="0"
            :step="0.01"
            hint="Cantidad aproximada que se pide por semana"
            required />
          <FormField
            label="Promedio por tacho"
            v-model="form.promedioPorTacho"
            type="number"
            :error="errors.promedioPorTacho"
            :min="0"
            :step="0.01"
            hint="Rendimiento aproximado por tacho"
            required />
        </div>

        <FormField
          label="Estado"
          v-model="form.activo"
          type="select"
          :options="[
            { value: true, label: 'Activo' },
            { value: false, label: 'Inactivo' }
          ]"
          required />

        <FormField
          label="Observaciones"
          v-model="form.observaciones"
          type="textarea"
          :error="errors.observaciones"
          placeholder="Notas opcionales sobre el producto"
          :rows="3" />

        <div class="form-actions">
          <router-link :to="{ name: 'productos' }" class="btn btn-secondary">
            Cancelar
          </router-link>
          <button type="submit" class="btn btn-primary" :disabled="submitting">
            <span v-if="submitting">Guardando...</span>
            <span v-else>Actualizar</span>
          </button>
        </div>
      </form>

      <div v-else class="loading-center"><Spinner /></div>
    </div>
  </div>
</template>
