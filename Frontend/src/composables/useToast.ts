// composables/useToast.ts
import { ElMessage, ElMessageBox } from 'element-plus'

export function useToast() {
  const success = (message: string) => {
    ElMessage({ message, type: 'success', duration: 3000 })
  }
  
  const error = (message: string) => {
    ElMessage({ message, type: 'error', duration: 5000 })
  }
  
  const warning = (message: string) => {
    ElMessage({ message, type: 'warning', duration: 4000 })
  }
  
  const info = (message: string) => {
    ElMessage({ message, type: 'info', duration: 3000 })
  }
  
  const confirm = (message: string, title = 'Confirmar', options: { confirmButtonText?: string; cancelButtonText?: string; type?: 'warning' | 'info' | 'success' | 'error' } = {}) => {
    return ElMessageBox.confirm(message, title, {
      confirmButtonText: options.confirmButtonText || 'Confirmar',
      cancelButtonText: options.cancelButtonText || 'Cancelar',
      type: options.type || 'warning',
      distinguishCancelAndClose: true
    }).then(() => true).catch(() => false)
  }
  
  const alert = (message: string, title = 'Alerta', type: 'warning' | 'info' | 'success' | 'error' = 'info') => {
    return ElMessageBox.alert(message, title, {
      confirmButtonText: 'OK',
      type
    })
  }
  
  return { success, error, warning, info, confirm, alert }
}