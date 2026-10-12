export interface TiradoProducto {
  id: number
  semanaId: number
  categoriaId: number | null
  categoriaNombre: string | null
  nombre: string
  unidadMedida: string
  pedidoSemanal: number
  stockInicial: number
  promedioPorTacho: number
  cantidadTirada: number
  cantidadTachos: number
  stockTotal: number
  tachosEstimados: number
  estado: 'Falta' | 'Listo' | 'Sobra'
  observaciones: string | null
  fechaCreacion: string
  fechaActualizacion: string | null
}

export interface TiradoProductoCreateDto {
  semanaId: number
  nombre: string
  unidadMedida: string
  pedidoSemanal: number
  stockInicial: number
  promedioPorTacho: number
  categoriaId: number | null
  observaciones: string | null
}

export interface TiradoProductoUpdateDto {
  nombre: string
  unidadMedida: string
  pedidoSemanal: number
  stockInicial: number
  promedioPorTacho: number
  categoriaId: number | null
  observaciones: string | null
  cantidadTirada?: number | null
}

export interface RegistrarTiradaDto {
  cantidad: number
  fecha?: string | null
  observaciones: string | null
}

export type FiltroTirado = 'totales' | 'faltantes' | 'sobrantes'

export interface TiradoProductoBulkItemDto {
  nombre: string
  unidadMedida: string
  promedioPorTacho: number
  stockInicial: number
  pedidoSemanal: number
  categoriaId: number | null
  observaciones: string | null
}

export interface TiradoProductoBulkCreateDto {
  items: TiradoProductoBulkItemDto[]
}

export interface TiradoProductoBulkResult {
  creados: number
  ignorados: number
  errores: string[]
  productosCreados: TiradoProducto[]
}