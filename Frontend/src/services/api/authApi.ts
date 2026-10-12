import { apiFetch, type ApiResponse } from '@/composables/useApiFetch'
import type { LoginRequest, LoginResponse, UserInfo, RegisterRequest, ChangePasswordRequest, RefreshTokenRequest } from '@/types/auth'

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
  
  register: (data: RegisterRequest): Promise<ApiResponse<UserInfo>> =>
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