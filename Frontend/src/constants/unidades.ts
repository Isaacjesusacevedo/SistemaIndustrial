// constants/unidades.ts
// Valores válidos centralizados para selects de unidad/envase.

export const UNIDADES_PRODUCTO = [
  'Galón (4 L)',
  '600 (600 ml)',
  'Perfume (50 ml)'
] as const

export const UNIDADES_INSUMO = [
  'kg', 'lt', 'un', 'm', 'm2', 'm3', 't', 'pza', 'bolsa', 'caja', 'pallet'
] as const

export type UnidadProducto = typeof UNIDADES_PRODUCTO[number]
export type UnidadInsumo = typeof UNIDADES_INSUMO[number]
