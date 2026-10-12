// stores/categorias.ts
import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { categoriasApi } from '@/services/api/categoriasApi'
import { useToast } from '@/composables/useToast'
import type { Categoria, CategoriaCreateDto, CategoriaUpdateDto } from '@/types/categoria'

export const useCategoriasStore = defineStore('categorias', () => {
  // ── State ──────────────────────────────────────────────────────────
  const items = ref<Categoria[]>([])
  const loading = ref(false)
  const error = ref<string | null>(null)

  const toast = useToast()

  // ── Getters ────────────────────────────────────────────────────────
  const activas = computed(() => items.value.filter(c => c.activa).sort((a, b) => a.orden - b.orden))

  // ── Actions ────────────────────────────────────────────────────────
  const cargarTodas = async () => {
    loading.value = true
    error.value = null

    try {
      const response = await categoriasApi.getAll()
      if (response.success) {
        items.value = response.data || []
      } else {
        throw new Error(response.message || 'Error al cargar categorías')
      }
    } catch (e: any) {
      error.value = e.message
      toast.error('Error al cargar categorías: ' + e.message)
      items.value = []
    } finally {
      loading.value = false
    }
  }

  const crear = async (data: CategoriaCreateDto) => {
    try {
      const response = await categoriasApi.create(data)
      if (response.success && response.data) {
        items.value.push(response.data)
        return response.data
      }
      throw new Error(response.message || 'Error al crear categoría')
    } catch (e: any) {
      throw new Error(e.message || 'Error al crear categoría')
    }
  }

  const actualizar = async (id: number, data: CategoriaUpdateDto) => {
    try {
      const response = await categoriasApi.update(id, data)
      if (response.success && response.data) {
        const idx = items.value.findIndex(i => i.id === id)
        if (idx !== -1) {
          items.value[idx] = response.data
        }
        return true
      }
      throw new Error(response.message)
    } catch (e: any) {
      throw new Error(e.message || 'Error al actualizar')
    }
  }

  const eliminar = async (id: number) => {
    try {
      const response = await categoriasApi.delete(id)
      if (response.success) {
        items.value = items.value.filter(i => i.id !== id)
        return true
      }
      throw new Error(response.message)
    } catch (e: any) {
      throw new Error(e.message || 'Error al eliminar')
    }
  }

  return {
    // state
    items,
    loading,
    error,
    // getters
    activas,
    // actions
    cargarTodas,
    crear,
    actualizar,
    eliminar,
  }
})