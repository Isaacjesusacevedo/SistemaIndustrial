// stores/stock.ts
import { defineStore } from 'pinia'
import { ref } from 'vue'
import { stockApi } from '@/services/api'
import type { Stock, StockCreateDto, StockUpdateDto, StockResponse } from '@/types/stock'

export const useStockStore = defineStore('stock', () => {
  const items = ref<StockResponse[]>([])
  const loading = ref(false)
  const error = ref<string | null>(null)
  
  async function fetchBySemana(semanaId: number) {
    loading.value = true
    error.value = null
    try {
      const response = await stockApi.getBySemana(semanaId)
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
  
  async function create(data: StockCreateDto) {
    const response = await stockApi.create(data)
    if (!response.success) throw new Error(response.message)
    items.value.push(response.data!)
    return response.data!
  }
  
  async function update(id: number, data: StockUpdateDto) {
    const response = await stockApi.update(id, data)
    if (!response.success) throw new Error(response.message)
    
    const index = items.value.findIndex(i => i.id === id)
    if (index !== -1) items.value[index] = response.data!
    
    return response.data!
  }
  
  async function deleteItem(id: number) {
    const response = await stockApi.delete(id)
    if (!response.success) throw new Error(response.message)
    items.value = items.value.filter(i => i.id !== id)
  }

  async function syncFromTirado(semanaId: number) {
    loading.value = true
    error.value = null
    try {
      const response = await stockApi.syncFromTirado(semanaId)
      if (response.success && response.data) {
        items.value = response.data
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
  
  return { items, loading, error, fetchBySemana, create, update, delete: deleteItem, syncFromTirado }
})