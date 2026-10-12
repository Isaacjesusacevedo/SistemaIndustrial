// stores/tiradoProductos.ts
import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { tiradoProductosApi } from '@/services/api/tiradoProductosApi'
import { useToast } from '@/composables/useToast'
import type {
  TiradoProducto,
  TiradoProductoCreateDto,
  TiradoProductoUpdateDto,
  RegistrarTiradaDto,
  FiltroTirado,
  TiradoProductoBulkCreateDto
} from '@/types/tiradoProducto'
import type { Tirada } from '@/types/tirada'

export const useTiradoProductosStore = defineStore('tiradoProductos', () => {
  // ── State ──────────────────────────────────────────────────────────
  const items = ref<TiradoProducto[]>([])
  const tachosSemana = ref<Tirada[]>([])
  const loading = ref(false)
  const error = ref<string | null>(null)
  const filtro = ref<FiltroTirado>('totales')
  const semanaIdActual = ref<number | null>(null)

  const toast = useToast()

  // ── Getters ────────────────────────────────────────────────────────
  /** Lista filtrada en cliente */
  const filtrados = computed(() => {
    switch (filtro.value) {
      case 'faltantes':
        return items.value.filter(p => p.stockTotal < 0)
      case 'sobrantes':
        return items.value.filter(p => p.stockTotal >= 0)
      default:
        return items.value
    }
  })

  /** Totales para el panel de resumen (Armado) */
  const totales = computed(() => {
    const total = items.value.length
    const faltantes = items.value.filter(p => p.stockTotal < 0).length
    const sobrantes = items.value.filter(p => p.stockTotal > 0).length
    const completados = items.value.filter(p => p.estado === 'Listo' || p.estado === 'Sobra').length

    const totalTachosFaltantes = items.value
      .filter(p => p.stockTotal < 0)
      .reduce((sum, p) => sum + Math.abs(p.tachosEstimados), 0)

    const totalTachosSobrantes = items.value
      .filter(p => p.stockTotal > 0)
      .reduce((sum, p) => sum + p.tachosEstimados, 0)

    return {
      total,
      faltantes,
      sobrantes,
      completados,
      porcentaje: total > 0 ? Math.round((completados / total) * 100) : 0,
      totalTachosFaltantes: Math.round(totalTachosFaltantes * 100) / 100,
      totalTachosSobrantes: Math.round(totalTachosSobrantes * 100) / 100,
    }
  })

  /** Agrupa items por categoría */
  const porCategoria = computed(() => {
    const grupos: Record<string, TiradoProducto[]> = {}
    for (const item of items.value) {
      const key = item.categoriaNombre || 'Sin categoría'
      if (!grupos[key]) grupos[key] = []
      grupos[key].push(item)
    }
    return Object.entries(grupos).map(([categoria, productos]) => ({
      categoria,
      productos,
      tachosEstimados: Math.round(productos.reduce((sum, p) => sum + p.tachosEstimados, 0) * 100) / 100,
    }))
  })

  // ── Actions ────────────────────────────────────────────────────────
  const cargarPorSemana = async (semanaId: number, filtroLocal?: FiltroTirado) => {
    loading.value = true
    error.value = null
    semanaIdActual.value = semanaId

    if (filtroLocal) filtro.value = filtroLocal

    try {
      const response = await tiradoProductosApi.getBySemana(semanaId, filtro.value)

      if (response.success) {
        items.value = response.data || []
      } else {
        throw new Error(response.message || 'Error al cargar productos')
      }
    } catch (e: any) {
      error.value = e.message
      toast.error('Error al cargar productos: ' + e.message)
      items.value = []
    } finally {
      loading.value = false
    }
  }

  const cambiarFiltro = (nuevoFiltro: FiltroTirado) => {
    filtro.value = nuevoFiltro
  }

  const cambiarFiltroYRecargar = async (nuevoFiltro: FiltroTirado) => {
    if (!semanaIdActual.value) return
    await cargarPorSemana(semanaIdActual.value, nuevoFiltro)
  }

  const crear = async (data: TiradoProductoCreateDto) => {
    try {
      const response = await tiradoProductosApi.create(data)
      if (response.success && response.data) {
        if (semanaIdActual.value) {
          await cargarPorSemana(semanaIdActual.value)
        }
        return response.data
      }
      throw new Error(response.message || 'Error al crear producto')
    } catch (e: any) {
      throw new Error(e.message || 'Error al crear producto')
    }
  }

  const actualizar = async (id: number, data: TiradoProductoUpdateDto) => {
    try {
      const response = await tiradoProductosApi.update(id, data)
      if (response.success) {
        if (semanaIdActual.value) {
          await cargarPorSemana(semanaIdActual.value)
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
      const response = await tiradoProductosApi.delete(id)
      if (response.success) {
        items.value = items.value.filter(i => i.id !== id)
        return true
      }
      throw new Error(response.message)
    } catch (e: any) {
      throw new Error(e.message || 'Error al eliminar')
    }
  }

  const registrarTirada = async (id: number, cantidad: number, observaciones?: string) => {
    try {
      const dto: RegistrarTiradaDto = { cantidad, observaciones: observaciones ?? null }
      const response = await tiradoProductosApi.registrarTirada(id, dto)
      if (response.success && response.data) {
        const idx = items.value.findIndex(i => i.id === id)
        if (idx !== -1) {
          items.value[idx] = response.data
        }
        return response.data
      }
      throw new Error(response.message || 'Error al registrar tirada')
    } catch (e: any) {
      throw new Error(e.message || 'Error al registrar tirada')
    }
  }

  const createBulk = async (semanaId: number, dto: TiradoProductoBulkCreateDto) => {
    const response = await tiradoProductosApi.createBulk(semanaId, dto)
    if (!response.success) throw new Error(response.message)
    // Recargar la semana para tener los items actualizados
    await cargarPorSemana(semanaId)
    return response
  }

  const cargarTachosSemana = async (semanaId: number) => {    loading.value = true
    error.value = null

    try {
      const response = await tiradoProductosApi.getTachosSemana(semanaId)
      if (response.success) {
        tachosSemana.value = response.data || []
      } else {
        throw new Error(response.message || 'Error al cargar tachos')
      }
    } catch (e: any) {
      error.value = e.message
      toast.error('Error al cargar tachos: ' + e.message)
      tachosSemana.value = []
    } finally {
      loading.value = false
    }
  }

  return {
    // state
    items,
    tachosSemana,
    loading,
    error,
    filtro,
    semanaIdActual,
    // getters
    filtrados,
    totales,
    porCategoria,
    // actions
    cargarPorSemana,
    cambiarFiltro,
    cambiarFiltroYRecargar,
    crear,
    createBulk,
    actualizar,
    eliminar,
    registrarTirada,
    cargarTachosSemana,
  }
})