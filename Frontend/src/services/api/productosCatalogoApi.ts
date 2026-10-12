import { apiFetch, type ApiResponse } from '@/composables/useApiFetch'
import type {
  ProductoCatalogo,
  ProductoCatalogoCreateDto,
  ProductoCatalogoUpdateDto
} from '@/types/productoCatalogo'

const BASE = '/api/productos'

export const productosCatalogoApi = {
  // GET /api/productos
  getAll: (): Promise<ApiResponse<ProductoCatalogo[]>> =>
    apiFetch<ProductoCatalogo[]>(`${BASE}`, { method: 'GET' }),

  // GET /api/productos/{id}
  getById: (id: number): Promise<ApiResponse<ProductoCatalogo>> =>
    apiFetch<ProductoCatalogo>(`${BASE}/${id}`, { method: 'GET' }),

  // POST /api/productos
  create: (data: ProductoCatalogoCreateDto): Promise<ApiResponse<ProductoCatalogo>> =>
    apiFetch<ProductoCatalogo>(`${BASE}`, {
      method: 'POST',
      body: JSON.stringify(data),
    }),

  // PUT /api/productos/{id}
  update: (id: number, data: ProductoCatalogoUpdateDto): Promise<ApiResponse<ProductoCatalogo>> =>
    apiFetch<ProductoCatalogo>(`${BASE}/${id}`, {
      method: 'PUT',
      body: JSON.stringify(data),
    }),

  // DELETE /api/productos/{id}
  delete: (id: number): Promise<ApiResponse<null>> =>
    apiFetch<null>(`${BASE}/${id}`, { method: 'DELETE' }),
}
