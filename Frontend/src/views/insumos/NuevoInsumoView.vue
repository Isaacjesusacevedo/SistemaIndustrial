<!-- views/insumos/NuevoInsumoView.vue -->
<script setup lang="ts">
import { ref, reactive, computed, onMounted } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { useInsumosStore } from '@/stores/insumos'
import { useToast } from '@/composables/useToast'
import FormField from '@/components/FormField.vue'

const router = useRouter()
const route = useRoute()
const store = useInsumosStore()
const toast = useToast()

const semanaId   = computed(() => Number(route.params.semanaId))
const insumoId   = computed(() => Number(route.params.id))
const isEditing  = computed(() => !!route.params.id)

const loading = ref(false)
const submitting = ref(false)

const form = reactive({
  nombre: '',
  unidadMedida: '',
  cantidadRequerida: 0,
  cantidadConsumida: 0,
  stockDisponible: 0
})

// ✅ ref en lugar de reactive
const errors = ref<Record<string, string>>({})

const unidades = ['kg', 'lt', 'un', 'm', 'm2', 'm3', 't', 'pza', 'bolsa', 'caja', 'pallet']

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
  if (form.cantidadRequerida < 0) {
    errors.value.cantidadRequerida = 'No puede ser negativo'
  }
  // ✅ Solo validar los campos que se envían al editar
  if (isEditing.value) {
    if (form.cantidadConsumida < 0) {
      errors.value.cantidadConsumida = 'No puede ser negativo'
    }
    if (form.stockDisponible < 0) {
      errors.value.stockDisponible = 'No puede ser negativo'
    }
  }
  return Object.keys(errors.value).length === 0
}

// ✅ fetchBySemana + find (no hay fetchById)
const cargarDatos = async () => {
  if (!isEditing.value) return
  loading.value = true
  try {
    await store.fetchBySemana(semanaId.value)
    const item = store.items.find(i => i.id === insumoId.value)

    if (!item) {
      toast.error('No se encontró el insumo')
      router.push({ name: 'insumos', params: { semanaId: semanaId.value } })
      return
    }

    Object.assign(form, {
      nombre: item.nombre,
      unidadMedida: item.unidadMedida,
      cantidadRequerida: item.cantidadRequerida,
      cantidadConsumida: item.cantidadConsumida,
      stockDisponible: item.stockDisponible
    })
  } catch (e: any) {
    toast.error(e.message || 'Error al cargar insumo')
    router.push({ name: 'insumos', params: { semanaId: semanaId.value } })
  } finally {
    loading.value = false
  }
}

const handleSubmit = async () => {
  if (!validate()) return
  submitting.value = true
  try {
    if (isEditing.value) {
      // ✅ InsumoUpdateDto: nombre, unidadMedida, cantidadRequerida, cantidadConsumida, stockDisponible
      await store.update(insumoId.value, {
        nombre: form.nombre,
        unidadMedida: form.unidadMedida,
        cantidadRequerida: form.cantidadRequerida,
        cantidadConsumida: form.cantidadConsumida,
        stockDisponible: form.stockDisponible
      })
      toast.success('Insumo actualizado correctamente')
    } else {
      // ✅ InsumoCreateDto: semanaId, nombre, unidadMedida, cantidadRequerida
      await store.create({
        semanaId: semanaId.value,
        nombre: form.nombre,
        unidadMedida: form.unidadMedida,
        cantidadRequerida: form.cantidadRequerida
      })
      toast.success('Insumo creado correctamente')
    }
    router.push({ name: 'insumos', params: { semanaId: semanaId.value } })
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
      <h1>{{ isEditing ? 'Editar' : 'Nuevo' }} Insumo</h1>
      <router-link :to="{ name: 'insumos', params: { semanaId } }" class="btn btn-secondary">
        Volver
      </router-link>
    </div>

    <div class="card">
      <form v-if="!loading" @submit.prevent="handleSubmit" class="form">
        <FormField
          label="Nombre"
          v-model="form.nombre"
          :error="errors.nombre"
          placeholder="Nombre del insumo"
          required />

        <div class="form-grid">
          <FormField
            label="Unidad de medida"
            v-model="form.unidadMedida"
            type="select"
            :options="unidades.map(u => ({ value: u, label: u }))"
            :error="errors.unidadMedida"
            required />
        </div>

        <div class="form-grid">
          <FormField
            label="Cantidad requerida"
            v-model="form.cantidadRequerida"
            type="number"
            :error="errors.cantidadRequerida"
            :min="0"
            :step="0.01"
            required />

          <!-- ✅ Solo se muestran al editar (InsumoUpdateDto) -->
          <template v-if="isEditing">
            <FormField
              label="Cantidad consumida"
              v-model="form.cantidadConsumida"
              type="number"
              :error="errors.cantidadConsumida"
              :min="0"
              :step="0.01" />
            <FormField
              label="Stock disponible"
              v-model="form.stockDisponible"
              type="number"
              :error="errors.stockDisponible"
              :min="0"
              :step="0.01" />
          </template>
        </div>

        <div class="form-actions">
          <router-link :to="{ name: 'insumos', params: { semanaId } }" class="btn btn-secondary">
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

