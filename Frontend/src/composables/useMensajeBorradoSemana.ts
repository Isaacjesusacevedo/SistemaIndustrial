// composables/useMensajeBorradoSemana.ts
import { useInsumosStore } from '@/stores/insumos'
import { useStockStore } from '@/stores/stock'
import { useTiradoProductosStore } from '@/stores/tiradoProductos'

const plural = (n: number, singular: string, pluralTxt: string) =>
  `${n} ${n === 1 ? singular : pluralTxt}`

/**
 * Texto del diálogo de confirmación al eliminar una semana.
 * Si los stores tienen cargados los datos de ESA semana, detalla cuántos
 * productos de tirado, insumos y registros de stock se van a borrar;
 * si no, muestra un texto genérico (no hace peticiones extra).
 */
export function useMensajeBorradoSemana() {
  const insumosStore = useInsumosStore()
  const stockStore = useStockStore()
  const tiradoStore = useTiradoProductosStore()

  return (semanaId: number, label = 'esta semana') => {
    const cargados =
      tiradoStore.semanaIdCargada === semanaId &&
      insumosStore.semanaIdCargada === semanaId &&
      stockStore.semanaIdCargada === semanaId

    if (!cargados) {
      return `¿Eliminar ${label}? Se perderán todos sus datos asociados (productos de tirado, insumos y stock).`
    }

    const productos = tiradoStore.items.length
    const insumos = insumosStore.items.length
    const stock = stockStore.items.length

    if (productos + insumos + stock === 0) {
      return `¿Eliminar ${label}? No tiene productos de tirado, insumos ni stock cargados.`
    }

    return (
      `¿Eliminar ${label}? Se van a borrar ` +
      `${plural(productos, 'producto de tirado', 'productos de tirado')}, ` +
      `${plural(insumos, 'insumo', 'insumos')} y ` +
      `${plural(stock, 'registro de stock', 'registros de stock')}.`
    )
  }
}
