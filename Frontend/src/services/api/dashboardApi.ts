import { apiFetch, type ApiResponse } from '@/composables/useApiFetch'
import type { DashboardMetricasDto, DashboardTendenciaDto, DashboardAlertaDto } from '@/types/dashboard'

const dashboardApi = {
  getMetricas: (semanaId: number): Promise<ApiResponse<any>> =>
    apiFetch(`/api/dashboard/metricas/${semanaId}`, { method: 'GET' }),

  getTendencias: (semanasAtras: number = 12): Promise<ApiResponse<any[]>> =>
    apiFetch(`/api/dashboard/tendencias?semanasAtras=${semanasAtras}`, { method: 'GET' }),

  getAlertas: (semanaId: number): Promise<ApiResponse<any[]>> =>
    apiFetch(`/api/dashboard/alertas/${semanaId}`, { method: 'GET' }),
}

export { dashboardApi }