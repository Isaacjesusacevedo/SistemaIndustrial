export interface Categoria {
  id: number
  nombre: string
  orden: number
  activa: boolean
  productosCount: number
}

export interface CategoriaCreateDto {
  nombre: string
  orden: number
}

export interface CategoriaUpdateDto {
  nombre: string
  orden: number
  activa: boolean
}