// composables/useAuth.ts
import { useAuthStore } from '@/stores/auth'
import { useRouter } from 'vue-router'

let routerInstance: ReturnType<typeof useRouter> | null = null

export function setRouter(router: ReturnType<typeof useRouter>) {
  routerInstance = router
}

export function useAuth() {
  const authStore = useAuthStore()
  const router = routerInstance || useRouter()
  
  const getToken = () => authStore.token
  const getRefreshToken = () => authStore.refreshToken
  const getUser = () => authStore.user
  const isAuthenticated = () => authStore.isAuthenticated
  const userRole = () => authStore.userRole
  
  const authHeaders = () => {
    const token = authStore.token
    return token
      ? { 'Content-Type': 'application/json', Authorization: `Bearer ${token}` }
      : { 'Content-Type': 'application/json' }
  }
  
  const login = async (credentials: { email: string; password: string }) => {
    return authStore.login(credentials)
  }
  
  const logout = async () => {
    await authStore.logout()
    router.push({ name: 'login' })
  }
  
  const requireAuth = () => {
    if (!authStore.isAuthenticated) {
      router.push({ name: 'login', query: { redirect: router.currentRoute.value.fullPath } })
      return false
    }
    return true
  }
  
  const requireRole = (...roles: string[]) => {
    if (!authStore.isAuthenticated) return false
    if (!roles.includes(authStore.userRole!)) {
      router.push({ name: 'home' })
      return false
    }
    return true
  }
  
  return {
    getToken,
    getRefreshToken,
    getUser,
    isAuthenticated,
    userRole,
    authHeaders,
    login,
    logout,
    requireAuth,
    requireRole
  }
}