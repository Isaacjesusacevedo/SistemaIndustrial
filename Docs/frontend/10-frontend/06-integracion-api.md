# Integración API

## Cliente HTTP (`composables/useApiFetch.ts`)

> No existe `services/api/baseApi.ts` ni `createApiClient`. El cliente HTTP real es `apiFetch`/`api` en `composables/useApiFetch.ts` (fetch nativo + `ApiResponse<T>`), y cada servicio de `services/api/*.ts` lo usa directamente con sus endpoints. Los tipos de filtro/paginado viven en `types/common.ts` y `types/api.ts`.

```typescript
// Patrón real usado por los servicios (ej. semanasApi.ts)
import { apiFetch } from '@/composables/useApiFetch'
import type { ApiResponse } from '@/types/api'

export const semanasApi = {
  getAll: (params?: { anio?: number; estado?: string }) =>
    apiFetch<ApiResponse<Semana[]>>(`/api/semanas?${new URLSearchParams(params as any)}`),
  getById: (id: number) =>
    apiFetch<ApiResponse<Semana>>(`/api/semanas/${id}`),
  // ... create / update / delete con fetch + JSON
}
```

---

## Servicios API por Dominio

Servicios reales en `services/api/`: `authApi`, `semanasApi`, `insumosApi`, `stockApi`, `tiradoProductosApi`, `productosCatalogoApi`, `categoriasApi`, `registroTachoApi`, `dashboardApi`.

> No existe `objetivosApi` (sin módulo de objetivos). El barrel `index.ts` solo exporta `authApi`, `semanasApi`, `insumosApi`, `stockApi`, `dashboardApi`, `tiradoProductosApi` y `productosCatalogoApi` (no exporta `registroTachoApi` ni `categoriasApi).

### Auth API (`services/api/authApi.ts`)
```typescript
import { apiFetch, type ApiResponse } from '@/composables/useApiFetch'
import type { LoginRequest, LoginResponse, UserInfo, ChangePasswordRequest } from '@/types/auth'

const authApi = {
  login: (credentials: LoginRequest): Promise<ApiResponse<LoginResponse>> =>
    apiFetch('/api/auth/login', {
      method: 'POST',
      body: JSON.stringify(credentials)
    }),
  
  refresh: (refreshToken: string): Promise<ApiResponse<LoginResponse>> =>
    apiFetch('/api/auth/refresh', {
      method: 'POST',
      body: JSON.stringify({ refreshToken })
    }),
  
  logout: (): Promise<ApiResponse<null>> =>
    apiFetch('/api/auth/logout', { method: 'POST' }),
  
  register: (data: { email: string; password: string; nombre: string; role: string }): Promise<ApiResponse<UserInfo>> =>
    apiFetch('/api/auth/register', {
      method: 'POST',
      body: JSON.stringify(data)
    }),
  
  changePassword: (currentPassword: string, newPassword: string): Promise<ApiResponse<null>> =>
    apiFetch('/api/auth/password', {
      method: 'PUT',
      body: JSON.stringify({ currentPassword, newPassword })
    }),
  
  validateToken: (token: string): Promise<ApiResponse<{ isValid: boolean }>> =>
    apiFetch('/api/auth/validate', {
      method: 'POST',
      body: JSON.stringify({ token })
    }),
  
  getProfile: (): Promise<ApiResponse<UserInfo>> =>
    apiFetch('/api/auth/profile', { method: 'GET' })
}

export { authApi }
```

### Semanas API (`services/api/semanasApi.ts`)
```typescript
import { apiFetch } from '@/composables/useApiFetch'
import type { Semana, SemanaCreateDto, SemanaUpdateDto } from '@/types/semana'

export const semanasApi = {
  // GET all con filtros (anio, estado), GET por id,
  // POST create, PUT update, PATCH cambio de estado, DELETE
  // ...todos vía apiFetch con sus endpoints /api/semanas
}
```

### Objetivos API — NO EXISTE

No hay `services/api/objetivosApi.ts` ni tipos de objetivo: el frontend no tiene módulo de objetivos.

### Catálogo y Sectores (`productosCatalogoApi.ts`, `tiradoProductosApi.ts`, `categoriasApi.ts`)
```typescript
// Patrón real: cada servicio usa apiFetch con sus endpoints
// - productosCatalogoApi: CRUD del catálogo global (/api/productos...)
// - tiradoProductosApi: productos de tirado por semana
// - categoriasApi: categorías de productos/insumos
// - registroTachoApi: registros de tacho por semana/turno
// - insumosApi / stockApi: CRUD por semanaId
```

### Insumos API (`services/api/insumosApi.ts`)
```typescript
// CRUD de insumos por semanaId vía apiFetch (ver archivo real)
export const insumosApi = { /* getBySemana, create, update, ... */ }
```

### Stock API (`services/api/stockApi.ts`)
```typescript
// CRUD de stock por semanaId vía apiFetch (ver archivo real)
export const stockApi = { /* getBySemana, create, update, ... */ }
```

### Registro Tacho API (`services/api/registroTachoApi.ts`)
```typescript
// Registros por semana/turno vía apiFetch (ver archivo real).
// Nota: existe pero no está exportado en el barrel index.ts.
export const registroTachoApi = { /* getBySemana, getBySemanaTurno, create, update, ... */ }
```

### Stock API (`services/api/stockApi.ts`)
```typescript
// CRUD de stock por semanaId vía apiFetch (ver archivo real)
export const stockApi = { /* getBySemana, create, update, ... */ }
```

### Registro Tacho API (`services/api/registroTachoApi.ts`)
```typescript
// Registros por semana/turno vía apiFetch (ver archivo real).
// Nota: existe pero no está exportado en el barrel index.ts.
export const registroTachoApi = { /* getBySemana, getBySemanaTurno, create, update, ... */ }
```

### Registro Tacho API (`services/api/registroTachoApi.ts`)
```typescript
// Registros por semana/turno vía apiFetch (ver archivo real).
// Nota: existe pero no está exportado en el barrel index.ts.
export const registroTachoApi = { /* getBySemana, getBySemanaTurno, create, update, ... */ }
```

### Dashboard API (`services/api/dashboardApi.ts`)
```typescript
import { apiFetch, type ApiResponse } from '@/composables/useApiFetch'
import type { DashboardMetricasDto, DashboardTendenciaDto, DashboardAlertaDto } from '@/types/dashboard'

export const dashboardApi = {
  getMetricas: (semanaId: number): Promise<ApiResponse<DashboardMetricasDto>> =>
    apiFetch(`/api/dashboard/metricas/${semanaId}`, { method: 'GET' }),
  
  getTendencias: (semanasAtras: number = 12): Promise<ApiResponse<DashboardTendenciaDto[]>> =>
    apiFetch(`/api/dashboard/tendencias?semanasAtras=${semanasAtras}`, { method: 'GET' }),
  
  getAlertas: (semanaId: number): Promise<ApiResponse<DashboardAlertaDto[]>> =>
    apiFetch(`/api/dashboard/alertas/${semanaId}`, { method: 'GET' })
}
```

---

## Index Barrel (`services/api/index.ts`)

```typescript
// Barrel real (services/api/index.ts)
export { authApi } from './authApi'
export { semanasApi } from './semanasApi'
export { insumosApi } from './insumosApi'
export { stockApi } from './stockApi'
export { dashboardApi } from './dashboardApi'
export { tiradoProductosApi } from './tiradoProductosApi'
export { productosCatalogoApi } from './productosCatalogoApi'
// Pendiente: exportar registroTachoApi y categoriasApi
```

---

## Uso en Stores (`stores/semanas.ts`)

```typescript
import { defineStore } from 'pinia'
import { semanasApi } from '@/services/api'
import type { Semana, SemanaCreateDto, SemanaUpdateDto, SemanaResponse } from '@/types/semana'
import { useToast } from '@/composables/useToast'

export const useSemanasStore = defineStore('semanas', () => {
  const toast = useToast()
  
  // State
  const items = ref<SemanaResponse[]>([])
  const current = ref<SemanaResponse | null>(null)
  const loading = ref(false)
  const error = ref<string | null>(null)
  
  // Actions
  async function fetchAll(params?: { anio?: number; estado?: string }) {
    loading.value = true
    error.value = null
    try {
      const response = await semanasApi.getAll(params)
      if (response.success) {
        items.value = response.data || []
      } else {
        throw new Error(response.message)
      }
    } catch (e: any) {
      error.value = e.message
      toast.error('Error al cargar semanas')
    } finally {
      loading.value = false
    }
  }
  
  async function fetchById(id: number) {
    loading.value = true
    try {
      const response = await semanasApi.getById(id)
      if (response.success) {
        current.value = response.data
        return response.data
      }
      throw new Error(response.message)
    } catch (e: any) {
      error.value = e.message
      throw e
    } finally {
      loading.value = false
    }
  }
  
  async function create(data: SemanaCreateDto) {
    const response = await semanasApi.create(data)
    if (!response.success) throw new Error(response.message)
    items.value.unshift(response.data!)
    return response.data!
  }
  
  async function update(id: number, data: SemanaUpdateDto) {
    const response = await semanasApi.update(id, data)
    if (!response.success) throw new Error(response.message)
    
    const index = items.value.findIndex(i => i.id === id)
    if (index !== -1) items.value[index] = response.data!
    if (current.value?.id === id) current.value = response.data!
    
    return response.data!
  }
  
  async function deleteItem(id: number) {
    const response = await semanasApi.delete(id)
    if (!response.success) throw new Error(response.message)
    items.value = items.value.filter(i => i.id !== id)
    if (current.value?.id === id) current.value = null
  }
  
  return {
    items,
    current,
    loading,
    error,
    fetchAll,
    fetchById,
    create,
    update,
    delete: deleteItem
  }
})
```

---

## Manejo de Errores en Vistas

```typescript
// En cualquier vista
const store = useSemanasStore()
const toast = useToast()

const handleCreate = async () => {
  try {
    await store.create(formData)
    toast.success('Semana creada correctamente')
    router.push({ name: 'detalle-semana', params: { id: store.current?.id } })
  } catch (error: any) {
    // Error ya mostrado en store, pero podemos agregar contexto
    if (error.message.includes('duplicado')) {
      toast.error('Ya existe una semana con ese número y año')
    }
    // Los errores de validación del backend vienen en response.errors
  }
}
```

---

## Tipado de Respuestas (`types/api.ts`)

```typescript
// Wrapper estándar del backend
export interface ApiResponse<T> {
  success: boolean
  message: string
  data: T | null
  errors: string[] | null
}

// Para listas paginadas (futuro)
export interface PagedResponse<T> {
  items: T[]
  totalCount: number
  page: number
  pageSize: number
  totalPages: number
}

// Parámetros de filtro comunes
export interface FilterParams {
  page?: number
  pageSize?: number
  sortBy?: string
  sortDir?: 'asc' | 'desc'
  search?: string
  [key: string]: any
}
```

---

## Testing de API (pendiente)

No hay carpeta `tests/` ni MSW instalado en el código real. El testing con mocks (MSW/Vitest/Cypress) queda como trabajo futuro.