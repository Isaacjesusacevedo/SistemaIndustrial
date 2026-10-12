// composables/useApiFetch.ts
import { useAuthStore } from '@/stores/auth'
import { useToast } from './useToast'

// ⚠️ FIX 1: fallback para evitar "undefined/api/..."
const API_BASE = import.meta.env.VITE_API_URL || 'http://localhost:5127'

let isRefreshing = false
let refreshSubscribers: Array<(token: string | null) => void> = []

function subscribeTokenRefresh(callback: (token: string | null) => void) {
  refreshSubscribers.push(callback)
}

function onTokenRefreshed(newToken: string | null) {
  refreshSubscribers.forEach(cb => cb(newToken))
  refreshSubscribers = []
}

export async function apiFetch<T = any>(
  endpoint: string,
  options: RequestInit = {}
): Promise<ApiResponse<T>> {
  const authStore = useAuthStore()
  const toast = useToast()

  const buildHeaders = (overrideToken?: string): HeadersInit => {
    const token = overrideToken ?? authStore.token
    return token
      ? { 'Content-Type': 'application/json', Authorization: `Bearer ${token}` }
      : { 'Content-Type': 'application/json' }
  }

  const url = `${API_BASE}${endpoint}`

  let response = await fetch(url, {
    ...options,
    headers: { ...buildHeaders(), ...options.headers }
  })

  // ── Manejo de 401: token expirado ─────────────────────────────────
  if (response.status === 401 && !endpoint.includes('/auth/')) {
    // Si ya hay un refresh en curso, esperar a que termine
    if (isRefreshing) {
      return new Promise<ApiResponse<T>>((resolve) => {
        subscribeTokenRefresh((newToken) => {
          if (!newToken) {
            // El refresh falló → devolver el error original
            resolve(handleResponse<T>(response))
            return
          }
          fetch(url, {
            ...options,
            headers: { ...buildHeaders(newToken), ...options.headers }
          }).then(res => resolve(handleResponse<T>(res)))
        })
      })
    }

    // Primer 401 → hacemos el refresh
    isRefreshing = true

    try {
      const refreshResponse = await authStore.refresh()

      // ✅ FIX 3: notificar ANTES de resetear el flag
      onTokenRefreshed(refreshResponse.token)

      // Reintentar la request original con el nuevo token
      response = await fetch(url, {
        ...options,
        headers: { ...buildHeaders(refreshResponse.token), ...options.headers }
      })
    } catch {
      // ✅ FIX 3: notificar a los que esperaban que el refresh falló
      onTokenRefreshed(null)

      authStore.clearSession()
      toast.error('Sesión expirada. Inicia sesión nuevamente.')
      if (routerInstance) {
        routerInstance.push({ name: 'login' })
      }
      throw new Error('Authentication failed')
    } finally {
      isRefreshing = false
    }
  }

  return handleResponse<T>(response)
}

async function handleResponse<T>(response: Response): Promise<ApiResponse<T>> {
  const contentType = response.headers.get('content-type')
  const isJson = contentType?.includes('application/json')

  if (!response.ok) {
    const errorData = isJson
      ? await response.json().catch(() => ({}))
      : { message: response.statusText }

    return {
      success: false,
      message: errorData?.message || `HTTP ${response.status}`,
      data: null,
      errors: errorData?.errors ?? null
    }
  }

  if (response.status === 204) {
    return { success: true, message: 'OK', data: null, errors: null }
  }

  const data = isJson ? await response.json() : null

  // Backend envuelve en { success, message, data, errors }
  if (data && typeof data === 'object' && 'success' in data) {
    return data as ApiResponse<T>
  }

  // Respuesta directa (sin wrapper)
  return { success: true, message: 'OK', data: data as T, errors: null }
}

// ── Helpers de métodos HTTP ─────────────────────────────────────────
export const api = {
  get: <T>(endpoint: string) => apiFetch<T>(endpoint, { method: 'GET' }),
  post: <T>(endpoint: string, body: any) =>
    apiFetch<T>(endpoint, { method: 'POST', body: JSON.stringify(body) }),
  put: <T>(endpoint: string, body: any) =>
    apiFetch<T>(endpoint, { method: 'PUT', body: JSON.stringify(body) }),
  patch: <T>(endpoint: string, body: any) =>
    apiFetch<T>(endpoint, { method: 'PATCH', body: JSON.stringify(body) }),
  remove: <T>(endpoint: string) => apiFetch<T>(endpoint, { method: 'DELETE' })
}

// ── Tipos ───────────────────────────────────────────────────────────
export interface ApiResponse<T> {
  success: boolean
  message: string
  data: T | null
  errors: string[] | null
}

// ── Router (para redirigir a login cuando expira la sesión) ─────────
let routerInstance: ReturnType<typeof import('vue-router').useRouter> | null = null

export function setRouter(router: ReturnType<typeof import('vue-router').useRouter>) {
  routerInstance = router
}