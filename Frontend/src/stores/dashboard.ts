// stores/dashboard.ts
import { defineStore } from 'pinia'
import { ref } from 'vue'
import { dashboardApi } from '@/services/api'
import type { DashboardMetricasDto, DashboardTendenciaDto, DashboardAlertaDto } from '@/types/dashboard'

export const useDashboardStore = defineStore('dashboard', () => {
  const metricas = ref<DashboardMetricasDto | null>(null)
  const tendencias = ref<DashboardTendenciaDto[]>([])
  const alertas = ref<DashboardAlertaDto[]>([])
  const loading = ref(false)
  const error = ref<string | null>(null)
  
  async function fetchMetricas(semanaId: number) {
    loading.value = true
    error.value = null
    try {
      const response = await dashboardApi.getMetricas(semanaId)
      if (response.success) {
        metricas.value = response.data
      } else {
        throw new Error(response.message)
      }
    } catch (e: any) {
      error.value = e.message
      throw e
    } finally {
      loading.value = false
    }
  }
  
  async function fetchTendencias(semanasAtras = 12) {
    loading.value = true
    error.value = null
    try {
      const response = await dashboardApi.getTendencias(semanasAtras)
      if (response.success) {
        tendencias.value = response.data || []
      } else {
        throw new Error(response.message)
      }
    } catch (e: any) {
      error.value = e.message
      throw e
    } finally {
      loading.value = false
    }
  }
  
  async function fetchAlertas(semanaId: number) {
    loading.value = true
    error.value = null
    try {
      const response = await dashboardApi.getAlertas(semanaId)
      if (response.success) {
        alertas.value = response.data || []
      } else {
        throw new Error(response.message)
      }
    } catch (e: any) {
      error.value = e.message
      throw e
    } finally {
      loading.value = false
    }
  }
  
  return { metricas, tendencias, alertas, loading, error, fetchMetricas, fetchTendencias, fetchAlertas }
})