<!-- views/semanas/EditarSemanaView.vue -->
<script setup lang="ts">
import { ref, reactive, computed, watch, onMounted } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { useSemanasStore } from '@/stores/semanas'
import { useToast } from '@/composables/useToast'
import FormField from '@/components/FormField.vue'

const router = useRouter()
const route = useRoute()
const store = useSemanasStore()
const toast = useToast()

const loading = ref(false)
const submitting = ref(false)
const fechasOriginalesInvalidas = ref(false)

// ✅ Union literal local (Opción A)
const ESTADOS = ['Planificada', 'EnProgreso', 'Completada', 'Cancelada'] as const
type EstadoSemana = typeof ESTADOS[number]

const form = reactive({
  numeroSemana: 1,
  anio: new Date().getFullYear(),
  fechaInicio: '',
  fechaFin: '',
  estado: 'Planificada' as EstadoSemana   // ✅ tipado, no string genérico
})

const errors = ref<Record<string, string>>({})

const parseInputDate = (s: string) => {
  const [y, m, d] = s.split('-').map(Number)
  return new Date(y, m - 1, d)
}

const toInputDate = (d: Date) => {
  const y = d.getFullYear()
  const m = String(d.getMonth() + 1).padStart(2, '0')
  const day = String(d.getDate()).padStart(2, '0')
  return `${y}-${m}-${day}`
}

const sonFechasLunesASabado = (inicioStr: string, finStr: string) => {
  if (!inicioStr || !finStr) return true
  const inicio = parseInputDate(inicioStr)
  const fin = parseInputDate(finStr)
  return (
    inicio.getDay() === 1 &&
    fin.getDay() === 6 &&
    Math.round((fin.getTime() - inicio.getTime()) / (1000 * 60 * 60 * 24)) === 5
  )
}

const fechasValidas = computed(() => sonFechasLunesASabado(form.fechaInicio, form.fechaFin))

watch(
  () => form.fechaInicio,
  (nuevo) => {
    if (!nuevo) return
    const inicio = parseInputDate(nuevo)
    const sabado = new Date(inicio)
    sabado.setDate(inicio.getDate() + 5)
    form.fechaFin = toInputDate(sabado)
  }
)

const validate = () => {
  errors.value = {}
  if (!form.numeroSemana || form.numeroSemana < 1 || form.numeroSemana > 53) {
    errors.value.numeroSemana = 'Número de semana debe ser entre 1 y 53'
  }
  if (!form.anio || form.anio < 2020 || form.anio > 2030) {
    errors.value.anio = 'Año fuera de rango (2020-2030)'
  }
  if (!form.fechaInicio) errors.value.fechaInicio = 'Fecha inicio es requerida'
  if (!form.fechaFin) errors.value.fechaFin = 'Fecha fin es requerida'
  if (form.fechaInicio && form.fechaFin) {
    const inicio = parseInputDate(form.fechaInicio)
    const fin = parseInputDate(form.fechaFin)
    if (inicio.getDay() !== 1) {
      errors.value.fechaInicio = 'La fecha de inicio debe ser lunes'
    }
    if (fin.getDay() !== 6) {
      errors.value.fechaFin = 'La fecha de fin debe ser sábado'
    } else if (fin <= inicio) {
      errors.value.fechaFin = 'Fecha fin debe ser posterior a fecha inicio'
    }
    const diff = Math.round((fin.getTime() - inicio.getTime()) / (1000 * 60 * 60 * 24))
    if (!errors.value.fechaInicio && !errors.value.fechaFin && diff !== 5) {
      errors.value.fechaFin = 'La semana debe durar 6 días (lunes a sábado)'
    }
  }
  // ✅ .includes sobre ESTADOS const
  if (!ESTADOS.includes(form.estado)) {
    errors.value.estado = 'Estado inválido'
  }
  return Object.keys(errors.value).length === 0
}

const cargarDatos = async () => {
  loading.value = true
  try {
    const data = await store.fetchById(Number(route.params.id))

    // ✅ Guard clause: data puede ser null
    if (!data) {
      toast.error('No se encontró la semana')
      router.push({ name: 'semanas' })
      return
    }

    const inicioStr = data.fechaInicio.split('T')[0]
    const finStr = data.fechaFin.split('T')[0]

    if (!sonFechasLunesASabado(inicioStr, finStr)) {
      fechasOriginalesInvalidas.value = true
      toast.warning('Esta semana no va de lunes a sábado. Corregí las fechas antes de guardar.')
    }

    Object.assign(form, {
      numeroSemana: data.numeroSemana,
      anio: data.anio,
      fechaInicio: inicioStr,
      fechaFin: finStr,
      estado: data.estado
    })
  } catch (e: any) {
    toast.error(e.message || 'Error al cargar semana')
    router.push({ name: 'semanas' })
  } finally {
    loading.value = false
  }
}

const handleSubmit = async () => {
  if (!validate()) return
  submitting.value = true
  try {
    await store.update(Number(route.params.id), {
      numeroSemana: form.numeroSemana,
      anio: form.anio,
      // ✅ .toISOString() → string
      fechaInicio: new Date(form.fechaInicio).toISOString(),
      fechaFin: new Date(form.fechaFin).toISOString(),
      estado: form.estado                    // ✅ ya es EstadoSemana
    })
    toast.success('Semana actualizada correctamente')
    router.push({ name: 'semanas' })
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
      <h1>Editar Semana</h1>
      <router-link :to="{ name: 'semanas' }" class="btn btn-secondary">Volver</router-link>
    </div>

    <div class="card">
      <form @submit.prevent="handleSubmit" class="form" v-if="!loading">
        <div v-if="fechasOriginalesInvalidas" class="alert alert-error" style="margin-bottom:16px">
          Esta semana fue creada con el formato anterior (incluía domingo).
          Ajustá las fechas a lunes–sábado antes de guardar.
        </div>

        <div class="form-grid">
          <FormField
            label="Número de semana"
            v-model="form.numeroSemana"
            type="number"
            :error="errors.numeroSemana"
            :min="1"
            :max="53"
            required />
          <FormField
            label="Año"
            v-model="form.anio"
            type="number"
            :error="errors.anio"
            :min="2020"
            :max="2030"
            required />
        </div>

        <div class="form-grid">
          <FormField
            label="Fecha inicio"
            v-model="form.fechaInicio"
            type="date"
            :error="errors.fechaInicio"
            hint="Siempre debe ser lunes"
            required />
          <FormField
            label="Fecha fin"
            v-model="form.fechaFin"
            type="date"
            :error="errors.fechaFin"
            hint="Siempre debe ser sábado (5 días después del lunes)"
            required />
        </div>

        <p v-if="!fechasValidas" style="color:var(--red); font-size:0.75rem;">
          Las semanas van de lunes a sábado
        </p>

        <!-- ✅ Opciones generadas desde ESTADOS -->
        <FormField
          label="Estado"
          v-model="form.estado"
          type="select"
          :error="errors.estado"
          :options="ESTADOS.map(e => ({
            value: e,
            label: e === 'EnProgreso' ? 'En Progreso' : e
          }))"
          required />

        <div class="form-actions">
          <router-link :to="{ name: 'semanas' }" class="btn btn-secondary">Cancelar</router-link>
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
