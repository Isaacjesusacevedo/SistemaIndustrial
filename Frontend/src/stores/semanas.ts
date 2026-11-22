// stores/semanas.ts
import { defineStore } from 'pinia'
import { ref } from 'vue'
import { semanasApi } from '@/services/api'
import type { Semana, SemanaCreateDto, SemanaUpdateDto, SemanaResponse } from '@/types/semana'

export const useSemanasStore = defineStore('semanas', () => {
  // State
  const items = ref<SemanaResponse[]>([])
  const current = ref<SemanaResponse | null>(null)
  const loading = ref(false)
  const error = ref<string | null>(null)
  
  // Petición de fetchAll en curso, para reutilizarla si llega otra igual
  // (ej: App.vue vía inicializar() y SemanasView al montar).
  let fetchAllEnCurso: { key: string; promise: Promise<void> } | null = null

  // Actions
  function fetchAll(params?: { anio?: number; estado?: string; force?: boolean }) {
    const { force = false, ...filtros } = params ?? {}
    const key = JSON.stringify({ anio: filtros.anio, estado: filtros.estado })

    if (!force && fetchAllEnCurso?.key === key) return fetchAllEnCurso.promise

    const promise = cargarTodas(Object.keys(filtros).length ? filtros : undefined).finally(() => {
      if (fetchAllEnCurso?.promise === promise) fetchAllEnCurso = null
    })
    fetchAllEnCurso = { key, promise }
    return promise
  }

  async function cargarTodas(params?: { anio?: number; estado?: string }) {
    loading.value = true
    error.value = null
    try {
      const response = await semanasApi.getAll(params)
      if (response.success) {
        items.value = response.data || []
      } else {
        throw new Error(response.message)
      }
    } catch (e: any) {
      error.value = e.message
      throw e
    } finally {
      loading.value = false
    }
  }
  
  async function fetchById(id: number) {
    loading.value = true
    try {
      const response = await semanasApi.getById(id)
      if (response.success) {
        current.value = response.data
        return response.data
      }
      throw new Error(response.message)
    } catch (e: any) {
      error.value = e.message
      throw e
    } finally {
      loading.value = false
    }
  }
  
  async function create(data: SemanaCreateDto) {
    const response = await semanasApi.create(data)
    if (!response.success) throw new Error(response.message)
    items.value.unshift(response.data!)
    return response.data!
  }
  
  async function update(id: number, data: SemanaUpdateDto) {
    const response = await semanasApi.update(id, data)
    if (!response.success) throw new Error(response.message)
    
    const index = items.value.findIndex(i => i.id === id)
    if (index !== -1) items.value[index] = response.data!
    if (current.value?.id === id) current.value = response.data!
    
    return response.data!
  }
  
  async function deleteItem(id: number) {
    const response = await semanasApi.delete(id)
    if (!response.success) throw new Error(response.message)
    items.value = items.value.filter(i => i.id !== id)
    if (current.value?.id === id) current.value = null
  }
  
  return {
    items,
    current,
    loading,
    error,
    fetchAll,
    fetchById,
    create,
    update,
    delete: deleteItem
  }
})