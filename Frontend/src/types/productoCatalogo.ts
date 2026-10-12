export interface ProductoCatalogo {
  id: number
  nombre: string
  unidadMedida: string
  pedidoSemanalAproximado: number
  promedioPorTacho: number
  activo: boolean
  observaciones?: string | null
  fechaCreacion: string
}

export interface ProductoCatalogoCreateDto {
  nombre: string
  unidadMedida: string
  pedidoSemanalAproximado: number
  promedioPorTacho: number
  observaciones?: string | null
}

export interface ProductoCatalogoUpdateDto {
  nombre: string
  unidadMedida: string
  pedidoSemanalAproximado: number
  promedioPorTacho: number
  activo: boolean
  observaciones?: string | null
}

export type ProductoCatalogoResponse = ProductoCatalogo
