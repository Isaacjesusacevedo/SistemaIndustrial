import { apiFetch, type ApiResponse } from '@/composables/useApiFetch'
import type {
  TiradoProducto,
  TiradoProductoCreateDto,
  TiradoProductoUpdateDto,
  RegistrarTiradaDto,
  FiltroTirado
} from '@/types/tiradoProducto'
import type { Tirada } from '@/types/tirada'
import type { TiradoProductoBulkCreateDto, TiradoProductoBulkResult } from '@/types/tiradoProducto'

const BASE = '/api/tirado'

export const tiradoProductosApi = {
  // GET /api/tirado/semana/{semanaId}
  getBySemana: (semanaId: number, filtro?: FiltroTirado): Promise<ApiResponse<TiradoProducto[]>> => {
    const params = new URLSearchParams()
    if (filtro) params.append('filtro', filtro)
    return apiFetch<ApiResponse<TiradoProducto[]>>(`${BASE}/semana/${semanaId}?${params.toString()}`, { method: 'GET' })
  },

  // GET /api/tirado/semana/{semanaId}/filtro
  getBySemanaConFiltro: (semanaId: number, filtro?: string): Promise<ApiResponse<TiradoProducto[]>> => {
    const params = new URLSearchParams()
    if (filtro) params.append('filtro', filtro)
    return apiFetch<ApiResponse<TiradoProducto[]>>(`${BASE}/semana/${semanaId}/filtro?${params.toString()}`, { method: 'GET' })
  },

  // GET /api/tirado/{id}
  getById: (id: number): Promise<ApiResponse<TiradoProducto>> =>
    apiFetch(`${BASE}/${id}`, { method: 'GET' }),

  // POST /api/tirado
  create: (data: TiradoProductoCreateDto): Promise<ApiResponse<TiradoProducto>> =>
    apiFetch(`${BASE}`, {
      method: 'POST',
      body: JSON.stringify(data),
    }),

  // PUT /api/tirado/{id}
  update: (id: number, data: TiradoProductoUpdateDto): Promise<ApiResponse<TiradoProducto>> =>
    apiFetch(`${BASE}/${id}`, {
      method: 'PUT',
      body: JSON.stringify(data),
    }),

  // PATCH /api/tirado/{id}/tirar
  registrarTirada: (id: number, data: RegistrarTiradaDto): Promise<ApiResponse<TiradoProducto>> =>
    apiFetch(`${BASE}/${id}/tirar`, {
      method: 'PATCH',
      body: JSON.stringify(data),
    }),

  // DELETE /api/tirado/{id}
  delete: (id: number): Promise<ApiResponse<null>> =>
    apiFetch(`${BASE}/${id}`, { method: 'DELETE' }),

  // GET /api/tirado/semana/{semanaId}/tachos
  getTachosSemana: (semanaId: number): Promise<ApiResponse<Tirada[]>> =>
    apiFetch<ApiResponse<Tirada[]>>(`${BASE}/semana/${semanaId}/tachos`, { method: 'GET' }),

  // POST /api/tirado/semana/{semanaId}/bulk
  createBulk: (semanaId: number, dto: TiradoProductoBulkCreateDto): Promise<ApiResponse<TiradoProductoBulkResult>> =>
    apiFetch<TiradoProductoBulkResult>(`${BASE}/semana/${semanaId}/bulk`, {
      method: 'POST',
      body: JSON.stringify(dto),
    }),
}