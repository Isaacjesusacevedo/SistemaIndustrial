<!-- views/semanas/NuevaSemanaView.vue -->
<script setup lang="ts">
import { ref, reactive, computed, watch } from 'vue'
import { useRouter } from 'vue-router'
import { useSemanasStore } from '@/stores/semanas'
import { useToast } from '@/composables/useToast'
import FormField from '@/components/FormField.vue'

const router = useRouter()
const store = useSemanasStore()
const toast = useToast()

const form = reactive({
  numeroSemana: 1,
  anio: new Date().getFullYear(),
  fechaInicio: '',
  fechaFin: ''
})

// ✅ ref en lugar de reactive → `errors.value = {}` ahora funciona
const errors = ref<Record<string, string>>({})
const loading = ref(false)
const submitting = ref(false)

const toInputDate = (d: Date) => {
  const y = d.getFullYear()
  const m = String(d.getMonth() + 1).padStart(2, '0')
  const day = String(d.getDate()).padStart(2, '0')
  return `${y}-${m}-${day}`
}

const parseInputDate = (s: string) => {
  const [y, m, d] = s.split('-').map(Number)
  return new Date(y, m - 1, d)
}

function getLunesDeEstaSemana(): Date {
  const hoy = new Date()
  const dia = hoy.getDay() // 0=domingo, 1=lunes, ..., 6=sábado
  const diasAlLunes = dia === 0 ? 1 : dia === 1 ? 0 : -(dia - 1)
  const lunes = new Date(hoy)
  lunes.setDate(hoy.getDate() + diasAlLunes)
  lunes.setHours(0, 0, 0, 0)
  return lunes
}

const addDias = (d: Date, n: number) => {
  const r = new Date(d)
  r.setDate(d.getDate() + n)
  return r
}

const fechasValidas = computed(() => {
  if (!form.fechaInicio || !form.fechaFin) return true
  const inicio = parseInputDate(form.fechaInicio)
  const fin = parseInputDate(form.fechaFin)
  return (
    inicio.getDay() === 1 &&
    fin.getDay() === 6 &&
    Math.round((fin.getTime() - inicio.getTime()) / (1000 * 60 * 60 * 24)) === 5
  )
})

watch(
  () => form.fechaInicio,
  (nuevo) => {
    if (!nuevo) return
    form.fechaFin = toInputDate(addDias(parseInputDate(nuevo), 5))
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
  return Object.keys(errors.value).length === 0
}

const handleSubmit = async () => {
  if (!validate()) return

  submitting.value = true
  try {
    const nueva = await store.create({
      numeroSemana: form.numeroSemana,
      anio: form.anio,
      fechaInicio: new Date(form.fechaInicio).toISOString(),
      fechaFin: new Date(form.fechaFin).toISOString()
    })
    toast.success('Semana creada correctamente')
    router.push({ name: 'cargar-inicial', params: { id: nueva.id } })
  } catch (error: any) {
    toast.error(error.message || 'Error al crear semana')
  } finally {
    submitting.value = false
  }
}

// Auto-completar: lunes de la semana actual (próximo lunes si hoy es domingo),
// fin = inicio + 5 días (sábado)
const lunesInicial = getLunesDeEstaSemana()
const sabadoInicial = addDias(lunesInicial, 5)

form.fechaInicio = toInputDate(lunesInicial)
form.fechaFin = toInputDate(sabadoInicial)
</script>

<template>
  <div class="section">
    <div class="section__header">
      <h1>Nueva Semana</h1>
      <router-link :to="{ name: 'semanas' }" class="btn btn-secondary">Volver</router-link>
    </div>

    <div class="card">
      <form @submit.prevent="handleSubmit" class="form" v-if="!loading">
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

        <div class="form-actions">
          <router-link :to="{ name: 'semanas' }" class="btn btn-secondary">Cancelar</router-link>
          <button type="submit" class="btn btn-primary" :disabled="submitting">
            <span v-if="submitting">Creando...</span>
            <span v-else>Crear Semana</span>
          </button>
        </div>
      </form>

      <div v-else class="loading-center"><Spinner /></div>
    </div>
  </div>
</template>
