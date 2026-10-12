// stores/productosCatalogo.ts
import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { productosCatalogoApi } from '@/services/api/productosCatalogoApi'
import type {
  ProductoCatalogo,
  ProductoCatalogoCreateDto,
  ProductoCatalogoUpdateDto
} from '@/types/productoCatalogo'

export const useProductosCatalogoStore = defineStore('productosCatalogo', () => {
  // ── State ──────────────────────────────────────────────────────────
  const items = ref<ProductoCatalogo[]>([])
  const loading = ref(false)
  const error = ref<string | null>(null)

  // ── Getters ────────────────────────────────────────────────────────
  const activos = computed(() => items.value.filter(p => p.activo))

  // ── Actions ────────────────────────────────────────────────────────
  const fetchAll = async () => {
    loading.value = true
    error.value = null
    try {
      const response = await productosCatalogoApi.getAll()
      if (response.success) {
        items.value = response.data || []
      } else {
        throw new Error(response.message || 'Error al cargar productos')
      }
    } catch (e: any) {
      error.value = e.message
      throw e
    } finally {
      loading.value = false
    }
  }

  const fetchActivos = async () => {
    await fetchAll()
    return activos.value
  }

  const fetchById = async (id: number) => {    const found = items.value.find(p => p.id === id)
    if (found) return found
    const response = await productosCatalogoApi.getById(id)
    if (!response.success || !response.data) throw new Error(response.message || 'No se encontró el producto')
    return response.data
  }

  const create = async (data: ProductoCatalogoCreateDto) => {
    const response = await productosCatalogoApi.create(data)
    if (!response.success) throw new Error(response.message)
    items.value.push(response.data!)
    return response.data!
  }

  const update = async (id: number, data: ProductoCatalogoUpdateDto) => {
    const response = await productosCatalogoApi.update(id, data)
    if (!response.success) throw new Error(response.message)

    const index = items.value.findIndex(i => i.id === id)
    if (index !== -1) items.value[index] = response.data!

    return response.data!
  }

  const deleteItem = async (id: number) => {
    const response = await productosCatalogoApi.delete(id)
    if (!response.success) throw new Error(response.message)
    items.value = items.value.filter(i => i.id !== id)
  }

  return {
    // state
    items,
    loading,
    error,
    // getters
    activos,
    // actions
    fetchAll,
    fetchActivos,
    fetchById,
    create,
    update,
    delete: deleteItem,
  }
})
