# Autenticación Frontend

## Visión General

Sistema de autenticación basado en **JWT (Access Token + Refresh Token)** con:
- **Access Token**: 8 horas, enviado en header `Authorization: Bearer`
- **Refresh Token**: 7 días, rotación en cada uso, almacenado en localStorage
- **Almacenamiento**: `localStorage` (persistente entre sesiones)
- **Guards de ruta**: Actualmente **comentados en modo demo** (acceso libre); las rutas conservan `meta.requiereAuth`
- **Interceptores**: Auto-refresh transparente en 401

> **Modo demo**: en `src/router/index.ts` el `beforeEach` está comentado (`// MODO DEMO: comentado para acceso libre`). Al salir del modo demo, descomentarlo para exigir sesión en las rutas con `requiereAuth` y redirigir a `login` con `?redirect=`.

---

## Almacenamiento (localStorage)

```typescript
// Claves
const TOKEN_KEY = 'auth_token'
const REFRESH_TOKEN_KEY = 'auth_refresh_token'
const USER_KEY = 'auth_user'

// Estructura guardada
{
  auth_token: "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  auth_refresh_token: "dGhpcyBpcyBhIHJlZnJlc2ggdG9rZW4...",
  auth_user: "{\"id\":1,\"email\":\"admin@industrial.com\",\"nombre\":\"Admin\",\"role\":\"Admin\"}"
}
```

---

## Store de Autenticación (`stores/auth.ts`)

```typescript
import { defineStore } from 'pinia'
import type { UserInfo, LoginRequest, LoginResponse } from '@/types/auth'
import { authApi } from '@/services/api/authApi'
import { useApiFetch } from '@/composables/useApiFetch'

export const useAuthStore = defineStore('auth', () => {
  // State
  const token = ref<string | null>(localStorage.getItem(TOKEN_KEY))
  const refreshToken = ref<string | null>(localStorage.getItem(REFRESH_TOKEN_KEY))
  const user = ref<UserInfo | null>(() => {
    const raw = localStorage.getItem(USER_KEY)
    try { return raw ? JSON.parse(raw) : null } catch { return null }
  })
  
  // Getters
  const isAuthenticated = computed(() => !!token.value)
  const userRole = computed(() => user.value?.role)
  const isAdmin = computed(() => userRole.value === 'Admin')
  const isSupervisor = computed(() => userRole.value === 'Supervisor' || userRole.value === 'Admin')
  
  // Actions
  function persistSession(loginResponse: LoginResponse) {
    token.value = loginResponse.token
    refreshToken.value = loginResponse.refreshToken
    user.value = loginResponse.user
    
    localStorage.setItem(TOKEN_KEY, loginResponse.token)
    localStorage.setItem(REFRESH_TOKEN_KEY, loginResponse.refreshToken)
    localStorage.setItem(USER_KEY, JSON.stringify(loginResponse.user))
  }
  
  function clearSession() {
    token.value = null
    refreshToken.value = null
    user.value = null
    localStorage.removeItem(TOKEN_KEY)
    localStorage.removeItem(REFRESH_TOKEN_KEY)
    localStorage.removeItem(USER_KEY)
  }
  
  async function login(credentials: LoginRequest) {
    const response = await authApi.login(credentials)
    persistSession(response)
    return response
  }
  
  async function refresh() {
    if (!refreshToken.value) throw new Error('No refresh token available')
    
    try {
      const response = await authApi.refresh(refreshToken.value)
      persistSession(response)
      return response
    } catch (error) {
      clearSession()
      throw error
    }
  }
  
  async function logout() {
    try {
      // Notificar al backend (opcional, para invalidar refresh token en BD)
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
```

> El store real no tiene `initAuth`: la sesión se rehidrata desde `localStorage` al crear el store. No documentar `initAuth` como existente.

---

## Composable useAuth (`composables/useAuth.ts`)

```typescript
import { useAuthStore } from '@/stores/auth'
import { useRouter } from 'vue-router'

export function useAuth() {
  const authStore = useAuthStore()
  const router = useRouter()
  
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
```

---

## Interceptor HTTP (`composables/useApiFetch.ts`)

```typescript
import { useAuthStore } from '@/stores/auth'
import { useToast } from '@/composables/useToast'

const API_BASE = import.meta.env.VITE_API_URL
let isRefreshing = false
let refreshSubscribers: Array<(token: string) => void> = []

function subscribeTokenRefresh(callback: (token: string) => void) {
  refreshSubscribers.push(callback)
}

function onTokenRefreshed(newToken: string) {
  refreshSubscribers.forEach(callback => callback(newToken))
  refreshSubscribers = []
}

export async function apiFetch<T = any>(
  endpoint: string,
  options: RequestInit = {}
): Promise<ApiResponse<T>> {
  const authStore = useAuthStore()
  const toast = useToast()
  
  const headers = {
    ...authHeaders(),
    ...options.headers
  } as HeadersInit
  
  const url = `${API_BASE}${endpoint}`
  
  let response = await fetch(url, { ...options, headers })
  
  // Manejar 401 - Token expirado
  if (response.status === 401 && !endpoint.includes('/auth/')) {
    const originalRequest = { endpoint, options }
    
    if (!isRefreshing) {
      isRefreshing = true
      
      try {
        const refreshResponse = await authStore.refresh()
        isRefreshing = false
        onTokenRefreshed(refreshResponse.token)
        
        // Reintentar request original con nuevo token
        const newHeaders = { ...headers, Authorization: `Bearer ${refreshResponse.token}` }
        response = await fetch(url, { ...options, headers: newHeaders })
      } catch (error) {
        isRefreshing = false
        refreshSubscribers = []
        
        // Refresh falló → logout
        authStore.clearSession()
        toast.error('Sesión expirada. Inicia sesión nuevamente.')
        router.push({ name: 'login' })
        throw new Error('Authentication failed')
      }
    } else {
      // Esperar a que termine el refresh en curso
      return new Promise((resolve, reject) => {
        subscribeTokenRefresh((newToken) => {
          const newHeaders = { ...headers, Authorization: `Bearer ${newToken}` }
          fetch(url, { ...options, headers: newHeaders })
            .then(res => resolve(handleResponse<T>(res)))
            .catch(reject)
        })
      })
    }
  }
  
  return handleResponse<T>(response)
}

async function handleResponse<T>(response: Response): Promise<ApiResponse<T>> {
  const contentType = response.headers.get('content-type')
  const isJson = contentType?.includes('application/json')
  
  if (!response.ok) {
    const errorData = isJson ? await response.json() : { message: response.statusText }
    return {
      success: false,
      message: errorData.message || `HTTP ${response.status}`,
      data: null,
      errors: errorData.errors
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

// Helpers para métodos HTTP
export const api = {
  get: <T>(endpoint: string) => apiFetch<T>(endpoint, { method: 'GET' }),
  post: <T>(endpoint: string, body: any) => apiFetch<T>(endpoint, { 
    method: 'POST', 
    body: JSON.stringify(body) 
  }),
  put: <T>(endpoint: string, body: any) => apiFetch<T>(endpoint, { 
    method: 'PUT', 
    body: JSON.stringify(body) 
  }),
  delete: <T>(endpoint: string) => apiFetch<T>(endpoint, { method: 'DELETE' }),
  patch: <T>(endpoint: string, body: any) => apiFetch<T>(endpoint, { 
    method: 'PATCH', 
    body: JSON.stringify(body) 
  })
}
```

---

## Login View (`views/auth/LoginView.vue`)

```vue
<script setup lang="ts">
import { ref, reactive } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import { useToast } from '@/composables/useToast'
import FormField from '@/components/FormField.vue'

const router = useRouter()
const route = useRoute()
const authStore = useAuthStore()
const toast = useToast()

const form = reactive({
  email: '',
  password: '',
  remember: false
})

const errors = reactive<Record<string, string>>({})
const loading = ref(false)

const validate = () => {
  errors.value = {}
  if (!form.email) errors.value.email = 'Email es requerido'
  else if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(form.email)) errors.value.email = 'Email inválido'
  if (!form.password) errors.value.password = 'Contraseña es requerida'
  else if (form.password.length < 8) errors.value.password = 'Mínimo 8 caracteres'
  return Object.keys(errors.value).length === 0
}

const handleSubmit = async () => {
  if (!validate()) return
  
  loading.value = true
  try {
    await authStore.login({ email: form.email, password: form.password })
    toast.success('Bienvenido, ' + authStore.user?.nombre)
    
    // Redirect a ruta guardada o home
    const redirect = route.query.redirect as string || '/'
    router.push(redirect)
  } catch (error: any) {
    toast.error(error.message || 'Credenciales inválidas')
  } finally {
    loading.value = false
  }
}
</script>

<template>
  <div class="layout-auth">
    <div class="card card--login">
      <div class="card-header">
        <h1 class="card-title">Sistema Industrial</h1>
        <p class="card-subtitle">Iniciar sesión</p>
      </div>
      
      <form @submit.prevent="handleSubmit" class="form">
        <FormField
          label="Email"
          v-model="form.email"
          type="email"
          :error="errors.email"
          placeholder="admin@industrial.com"
          required
          autocomplete="email"
        />
        
        <FormField
          label="Contraseña"
          v-model="form.password"
          type="password"
          :error="errors.password"
          placeholder="••••••••"
          required
          autocomplete="current-password"
          @keyup.enter="handleSubmit"
        />
        
        <div class="form-row form-row--align-center">
          <label class="checkbox-wrapper">
            <input type="checkbox" v-model="form.remember" />
            <span>Recordarme</span>
          </label>
        </div>
        
        <button 
          type="submit" 
          class="btn btn-primary btn--block"
          :disabled="loading"
        >
          <span v-if="loading" class="spinner"></span>
          <span v-else>Iniciar Sesión</span>
        </button>
      </form>
    </div>
  </div>
</template>

<style scoped>
.card--login { max-width: 400px; margin: 2rem auto; }
.form-row--align-center { justify-content: center; }
.checkbox-wrapper { display: flex; align-items: center; gap: 0.5rem; cursor: pointer; }
.btn--block { width: 100%; }
.spinner { display: inline-block; width: 16px; height: 16px; border: 2px solid currentColor; border-right-color: transparent; border-radius: 50%; animation: spin 0.8s linear infinite; }
@keyframes spin { to { transform: rotate(360deg); } }
</style>
```

---

## Guards de Ruta (Integración)

```typescript
// router/index.ts - guard real (comentado en modo demo)
// router.beforeEach((to) => {
//   const authStore = useAuthStore()
//
//   if (to.meta.requiereAuth && !authStore.isAuthenticated) {
//     return { name: 'login', query: { redirect: to.fullPath } }
//   }
//
//   if (to.name === 'login' && authStore.isAuthenticated) {
//     return { name: 'dashboard' }
//   }
// })
```

> No existe `router/guards.ts` ni validación de roles en el router real. Descomentar al salir del modo demo.

---

## Logout y Limpieza

```typescript
// En NavBarUserMenu.vue
const handleLogout = async () => {
  const confirmed = await confirmDialog('¿Cerrar sesión?')
  if (!confirmed) return
  
  try {
    await authStore.logout()
    toast.success('Sesión cerrada')
    router.push({ name: 'login' })
  } catch (error) {
    toast.error('Error al cerrar sesión')
  }
}
```

---

## Manejo de Expiración Silenciosa (propuesta, no implementado en `App.vue`/`main.ts`)

```typescript
// En App.vue o main.ts - verificar periódicamente
let authCheckInterval: number

function startAuthCheck() {
  authCheckInterval = window.setInterval(async () => {
    const authStore = useAuthStore()
    if (authStore.token && !authStore.isAuthenticated) {
      try {
        await authStore.refresh()
      } catch {
        authStore.clearSession()
        router.push({ name: 'login' })
      }
    }
  }, 5 * 60 * 1000) // Cada 5 minutos
}

function stopAuthCheck() {
  clearInterval(authCheckInterval)
}

// Lifecycle
onMounted(startAuthCheck)
onUnmounted(stopAuthCheck)
```

---

## Seguridad Adicional

### HttpOnly Cookies (Futuro - Más Seguro)
```typescript
// Backend: Set-Cookie en login/refresh
res.cookie('access_token', token, {
  httpOnly: true,
  secure: true,        // Solo HTTPS
  sameSite: 'strict',  // CSRF protection
  maxAge: 8 * 60 * 60 * 1000 // 8 horas
})

res.cookie('refresh_token', refreshToken, {
  httpOnly: true,
  secure: true,
  sameSite: 'strict',
  maxAge: 7 * 24 * 60 * 60 * 1000 // 7 días
})

// Frontend: Credentials automáticas
fetch(url, { credentials: 'include' }) // Cookies enviadas automáticamente
```

### Ventajas Cookies HttpOnly
- **XSS Protection**: JavaScript no puede leer tokens
- **CSRF Protection**: `SameSite=Strict` + CSRF token opcional
- **Auto-sent**: No header manual necesario

### Migración Planificada
1. Backend: Implementar cookie-based auth paralelo a JWT header
2. Frontend: Detectar modo (header vs cookie) via feature flag
3. Deprecation: Migrar gradualmente, eliminar localStorage