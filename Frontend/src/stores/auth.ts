// stores/auth.ts
import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { authApi } from '@/services/api'
import type { UserInfo, LoginRequest, LoginResponse } from '@/types/auth'

function loadUserFromStorage(): UserInfo | null {
  const raw = localStorage.getItem('auth_user')
  if (!raw) return null
  try {
    return JSON.parse(raw) as UserInfo
  } catch {
    return null
  }
}

export const useAuthStore = defineStore('auth', () => {
  // ── State ────────────────────────────────────────────────────────
  const token = ref<string | null>(localStorage.getItem('auth_token'))
  const refreshToken = ref<string | null>(localStorage.getItem('auth_refresh_token'))
  const user = ref<UserInfo | null>(loadUserFromStorage())

  // ── Getters ──────────────────────────────────────────────────────
  const isAuthenticated = computed(() => !!token.value)
  const userRole = computed(() => user.value?.role)
  const isAdmin = computed(() => userRole.value === 'Admin')
  const isSupervisor = computed(() => userRole.value === 'Supervisor' || userRole.value === 'Admin')

  // ── Actions ──────────────────────────────────────────────────────
  function persistSession(loginResponse: LoginResponse) {
    token.value = loginResponse.token
    refreshToken.value = loginResponse.refreshToken
    user.value = loginResponse.user

    localStorage.setItem('auth_token', loginResponse.token)
    localStorage.setItem('auth_refresh_token', loginResponse.refreshToken)
    localStorage.setItem('auth_user', JSON.stringify(loginResponse.user))
  }

  function clearSession() {
    token.value = null
    refreshToken.value = null
    user.value = null
    localStorage.removeItem('auth_token')
    localStorage.removeItem('auth_refresh_token')
    localStorage.removeItem('auth_user')
  }

  async function login(credentials: LoginRequest) {
    const response = await authApi.login(credentials)
    if (response.success && response.data) {
      persistSession(response.data)
      return response.data
    }
    throw new Error(response.message || 'Credenciales inválidas')
  }

  async function refresh() {
    if (!refreshToken.value) throw new Error('No refresh token available')

    const response = await authApi.refresh(refreshToken.value)
    if (response.success && response.data) {
      persistSession(response.data)
      return response.data
    }
    clearSession()
    throw new Error('Error al renovar token')
  }

  async function logout() {
    try {
      if (token.value) {
        await authApi.logout()
      }
    } catch {
      // Ignorar errores de logout en backend
    } finally {
      clearSession()
    }
  }

  async function changePassword(currentPassword: string, newPassword: string) {
    await authApi.changePassword(currentPassword, newPassword)
    // Backend invalida refresh tokens → forzar re-login
    clearSession()
  }

  return {
    // State
    token,
    refreshToken,
    user,
    // Getters
    isAuthenticated,
    userRole,
    isAdmin,
    isSupervisor,
    // Actions
    login,
    refresh,
    logout,
    changePassword,
    persistSession,
    clearSession
  }
})