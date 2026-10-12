export function formatNumber(n: number | null | undefined): string {
    if (n === null || n === undefined || isNaN(n)) return '0'
    return n.toLocaleString('es-AR')
  }