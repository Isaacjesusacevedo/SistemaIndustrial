// stores/insumos.ts
import { defineStore } from 'pinia'
import { ref } from 'vue'
import { insumosApi } from '@/services/api'
import type { Insumo, InsumoCreateDto, InsumoUpdateDto, InsumoResponse } from '@/types/insumo'

export const useInsumosStore = defineStore('insumos', () => {
  const items = ref<InsumoResponse[]>([])
  /** Semana a la que corresponden `items` (null si no se cargó o falló la carga). */
  const semanaIdCargada = ref<number | null>(null)
  const loading = ref(false)
  const error = ref<string | null>(null)
  
  async function fetchBySemana(semanaId: number) {
    loading.value = true
    error.value = null
    try {
      semanaIdCargada.value = null
      const response = await insumosApi.getBySemana(semanaId)
      if (response.success) {
        items.value = response.data || []
        semanaIdCargada.value = semanaId
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
  
  async function create(data: InsumoCreateDto) {
    const response = await insumosApi.create(data)
    if (!response.success) throw new Error(response.message)
    items.value.push(response.data!)
    return response.data!
  }
  
  async function update(id: number, data: InsumoUpdateDto) {
    const response = await insumosApi.update(id, data)
    if (!response.success) throw new Error(response.message)
    
    const index = items.value.findIndex(i => i.id === id)
    if (index !== -1) items.value[index] = response.data!
    
    return response.data!
  }
  
  async function deleteItem(id: number) {
    const response = await insumosApi.delete(id)
    if (!response.success) throw new Error(response.message)
    items.value = items.value.filter(i => i.id !== id)
  }
  
  return { items, semanaIdCargada, loading, error, fetchBySemana, create, update, delete: deleteItem }
})