import { apiFetch, type ApiResponse } from '@/composables/useApiFetch'
import type { Categoria, CategoriaCreateDto, CategoriaUpdateDto } from '@/types/categoria'

const BASE = '/api/categorias'

export const categoriasApi = {
  // GET /api/categorias
  getAll: (): Promise<ApiResponse<Categoria[]>> =>
    apiFetch<ApiResponse<Categoria[]>>(`${BASE}`, { method: 'GET' }),

  // GET /api/categorias/{id}
  getById: (id: number): Promise<ApiResponse<Categoria>> =>
    apiFetch(`${BASE}/${id}`, { method: 'GET' }),

  // POST /api/categorias
  create: (data: CategoriaCreateDto): Promise<ApiResponse<Categoria>> =>
    apiFetch(`${BASE}`, {
      method: 'POST',
      body: JSON.stringify(data),
    }),

  // PUT /api/categorias/{id}
  update: (id: number, data: CategoriaUpdateDto): Promise<ApiResponse<Categoria>> =>
    apiFetch(`${BASE}/${id}`, {
      method: 'PUT',
      body: JSON.stringify(data),
    }),

  // DELETE /api/categorias/{id}
  delete: (id: number): Promise<ApiResponse<null>> =>
    apiFetch(`${BASE}/${id}`, { method: 'DELETE' }),
}