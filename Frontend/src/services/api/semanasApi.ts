import { apiFetch, type ApiResponse } from '@/composables/useApiFetch'
import type { Semana, SemanaCreateDto, SemanaUpdateDto, SemanaResponse } from '@/types/semana'

const semanasApi = {
  getAll: (params?: { anio?: number; estado?: string }): Promise<ApiResponse<Semana[]>> => {
    const query = new URLSearchParams()
    if (params?.anio) query.append('anio', params.anio.toString())
    if (params?.estado) query.append('estado', params.estado)
    const qs = query.toString()
    return apiFetch(`/api/semanas${qs ? '?' + qs : ''}`, { method: 'GET' })
  },

  getById: (id: number): Promise<ApiResponse<Semana>> =>
    apiFetch(`/api/semanas/${id}`, { method: 'GET' }),

  create: (data: SemanaCreateDto): Promise<ApiResponse<SemanaResponse>> =>
    apiFetch('/api/semanas', {
      method: 'POST',
      body: JSON.stringify(data),
    }),

  update: (id: number, data: SemanaUpdateDto): Promise<ApiResponse<SemanaResponse>> =>
    apiFetch(`/api/semanas/${id}`, {
      method: 'PUT',
      body: JSON.stringify(data),
    }),

  delete: (id: number): Promise<ApiResponse<null>> =>
    apiFetch(`/api/semanas/${id}`, { method: 'DELETE' }),
}

export { semanasApi }