import { apiFetch, type ApiResponse } from '@/composables/useApiFetch'
import type { InsumoSemana, InsumoCreateDto, InsumoUpdateDto, InsumoResponse } from '@/types/insumo'

const insumosApi = {
  getBySemana: (semanaId: number): Promise<ApiResponse<any[]>> =>
    apiFetch(`/api/insumos?semanaId=${semanaId}`, { method: 'GET' }),

  getById: (id: number): Promise<ApiResponse<any>> =>
    apiFetch(`/api/insumos/${id}`, { method: 'GET' }),

  create: (data: { semanaId: number; nombre: string; unidadMedida: string; cantidadRequerida: number }): Promise<ApiResponse<any>> =>
    apiFetch('/api/insumos', {
      method: 'POST',
      body: JSON.stringify(data),
    }),

  update: (id: number, data: { nombre: string; unidadMedida: string; cantidadRequerida: number; cantidadConsumida: number; stockDisponible: number }): Promise<ApiResponse<any>> =>
    apiFetch(`/api/insumos/${id}`, {
      method: 'PUT',
      body: JSON.stringify(data),
    }),

  delete: (id: number): Promise<ApiResponse<null>> =>
    apiFetch(`/api/insumos/${id}`, { method: 'DELETE' }),
}

export { insumosApi }