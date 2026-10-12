export interface Tirada {
  id: number
  tiradoProductoId: number
  productoNombre: string
  fecha: string
  cantidad: number
  observaciones: string | null
  fechaCreacion: string
}