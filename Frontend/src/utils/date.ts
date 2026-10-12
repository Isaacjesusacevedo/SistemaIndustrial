export function formatDate(d: string | Date | null | undefined): string {
    if (!d) return '—'
    const date = typeof d === 'string' ? new Date(d) : d
    if (isNaN(date.getTime())) return '—'
    return date.toLocaleDateString('es-AR', {
      day: '2-digit', month: 'short', year: 'numeric'
    })
  }
  // src/utils/date.ts
export function formatDateTime(d: string | Date | null | undefined): string {
    if (!d) return '—'
    const date = typeof d === 'string' ? new Date(d) : d
    if (isNaN(date.getTime())) return '—'
    return date.toLocaleString('es-AR', {
      day: '2-digit', month: '2-digit', year: 'numeric',
      hour: '2-digit', minute: '2-digit'
    })
  }