import { apiFetch, type ApiResponse } from '@/composables/useApiFetch'
import type { StockSemana, StockCreateDto, StockUpdateDto, StockResponse } from '@/types/stock'

const stockApi = {
  getBySemana: (semanaId: number): Promise<ApiResponse<any[]>> =>
    apiFetch(`/api/stock?semanaId=${semanaId}`, { method: 'GET' }),

  getById: (id: number): Promise<ApiResponse<any>> =>
    apiFetch(`/api/stock/${id}`, { method: 'GET' }),

  create: (data: { semanaId: number; producto: string; unidadMedida: string; stockInicial: number }): Promise<ApiResponse<any>> =>
    apiFetch('/api/stock', {
      method: 'POST',
      body: JSON.stringify(data),
    }),

  update: (id: number, data: { producto: string; unidadMedida: string; stockInicial: number; entradas: number; salidas: number }): Promise<ApiResponse<any>> =>
    apiFetch(`/api/stock/${id}`, {
      method: 'PUT',
      body: JSON.stringify(data),
    }),

  delete: (id: number): Promise<ApiResponse<null>> =>
    apiFetch(`/api/stock/${id}`, { method: 'DELETE' }),

  syncFromTirado: (semanaId: number): Promise<ApiResponse<any[]>> =>
    apiFetch(`/api/stock/sync-from-tirado/${semanaId}`, { method: 'POST' }),
}

export { stockApi }