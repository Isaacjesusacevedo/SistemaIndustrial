<!-- views/productos/NuevoProductoView.vue -->
<script setup lang="ts">
import { ref, reactive } from 'vue'
import { useRouter } from 'vue-router'
import { useProductosCatalogoStore } from '@/stores/productosCatalogo'
import { useToast } from '@/composables/useToast'
import FormField from '@/components/FormField.vue'
import { UNIDADES_PRODUCTO } from '@/constants/unidades'

const router = useRouter()
const store = useProductosCatalogoStore()
const toast = useToast()

const unidades = [...UNIDADES_PRODUCTO]

const form = reactive({
  nombre: '',
  unidadMedida: '',
  pedidoSemanalAproximado: 0,
  promedioPorTacho: 0,
  observaciones: ''
})

// ✅ ref en lugar de reactive → errors.value = {} funciona
const errors = ref<Record<string, string>>({})
const submitting = ref(false)

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

const handleSubmit = async () => {
  if (!validate()) return

  submitting.value = true
  try {
    await store.create({
      nombre: form.nombre.trim(),
      unidadMedida: form.unidadMedida,
      pedidoSemanalAproximado: Number(form.pedidoSemanalAproximado),
      promedioPorTacho: Number(form.promedioPorTacho),
      observaciones: form.observaciones.trim() || null
    })
    toast.success('Producto creado correctamente')
    router.push({ name: 'productos' })
  } catch (error: any) {
    toast.error(error.message || 'Error al crear producto')
  } finally {
    submitting.value = false
  }
}
</script>

<template>
  <div class="section">
    <div class="section__header">
      <h1>Nuevo Producto</h1>
      <router-link :to="{ name: 'productos' }" class="btn btn-secondary">
        Volver
      </router-link>
    </div>

    <div class="card">
      <form @submit.prevent="handleSubmit" class="form">
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
            <span v-if="submitting">Creando...</span>
            <span v-else>Crear Producto</span>
          </button>
        </div>
      </form>
    </div>
  </div>
</template>
