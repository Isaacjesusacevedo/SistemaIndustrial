// src/types/common.ts

/**
 * Columna genérica para DataTable.
 * El `render` recibe el item y devuelve string | VNode.
 */
export interface Column<T = any> {
    key: string
    label: string
    width?: string
    align?: 'left' | 'center' | 'right'
    render?: (item: T) => string | number | any
  }
  
  /**
   * Respuesta estándar del backend.
   */
  export interface ApiResponse<T = any> {
    success: boolean
    message?: string
    data?: T | null
    errors?: string[] | null
  }
  
  /**
   * Filtro base para listados paginados.
   */
  export interface PaginationParams {
    page?: number
    pageSize?: number
    sortBy?: string
    sortOrder?: 'asc' | 'desc'
  }
  
  /**
   * Estado genérico de carga.
   */
  export interface LoadingState {
    loading: boolean
    error: string | null
  }