// stores/semanaActiva.ts
import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { useSemanasStore } from './semanas'

const STORAGE_KEY = 'industrial_semana_activa_id'

export const useSemanaActivaStore = defineStore('semanaActiva', () => {
  const semanasStore = useSemanasStore()

  // State
  const semanaId = ref<number | null>(null)
  const inicializado = ref(false)

  // Getters
  const semana = computed(() =>
    semanasStore.items.find(s => s.id === semanaId.value) || null
  )

  const tieneSemanaActiva = computed(() => !!semana.value)

  // Actions
  function setSemana(id: number | null) {
    semanaId.value = id
    if (id) {
      localStorage.setItem(STORAGE_KEY, String(id))
    } else {
      localStorage.removeItem(STORAGE_KEY)
    }
  }

  /**
   * Inicializa la semana activa:
   * 1. Carga las semanas si no están
   * 2. Intenta restaurar desde localStorage
   * 3. Si no, selecciona la más reciente
   *
   * Todas las llamadas concurrentes (App.vue, DashboardView, ...) esperan la
   * MISMA promesa, así nadie sigue de largo antes de que termine la carga.
   */
  let initPromise: Promise<void> | null = null

  async function cargarInicial() {
    try {
      if (semanasStore.items.length === 0) {
        await semanasStore.fetchAll()
      }

      // Restaurar desde localStorage
      const saved = localStorage.getItem(STORAGE_KEY)
      if (saved) {
        const id = Number(saved)
        if (semanasStore.items.some(s => s.id === id)) {
          semanaId.value = id
          return
        }
      }

      // Si no hay guardada o fue eliminada, tomar la más reciente
      if (semanasStore.items.length > 0) {
        setSemana(semanasStore.items[0].id)
      }
    } catch {
      // Silencioso: si falla, simplemente no hay semana activa.
      // Se descarta la promesa para que una próxima llamada pueda reintentar.
      initPromise = null
    } finally {
      inicializado.value = true
    }
  }

  function inicializar() {
    return (initPromise ??= cargarInicial())
  }

  /** Descarta la inicialización previa y vuelve a cargar (ej: tras borrar la semana activa). */
  function reinicializar() {
    initPromise = null
    return inicializar()
  }

  return {
    semanaId,
    semana,
    tieneSemanaActiva,
    inicializado,
    setSemana,
    inicializar,
    reinicializar
  }
})