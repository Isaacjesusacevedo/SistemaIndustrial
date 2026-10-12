<!-- components/NavBarUserMenu.vue -->
<script setup lang="ts">
import { useAuthStore } from '@/stores/auth'
import { useToast } from '@/composables/useToast'
import { useResponsive } from '@/composables/useResponsive'
import { User, Setting, Lock, Logout } from '@element-plus/icons-vue'
import { ref } from 'vue'
import { ElDropdown, ElDropdownMenu, ElDropdownItem } from 'element-plus'

const authStore = useAuthStore()
const toast = useToast()
const { isMobile } = useResponsive()
const showChangePassword = ref(false)
const passwordForm = ref({ current: '', new: '', confirm: '' })
const loading = ref(false)

const handleLogout = async () => {
  const confirmed = await confirmDialog('¿Cerrar sesión?')
  if (!confirmed) return
  
  try {
    await authStore.logout()
    toast.success('Sesión cerrada')
  } catch (error) {
    toast.error('Error al cerrar sesión')
  }
}

const handleChangePassword = async () => {
  if (passwordForm.value.new !== passwordForm.value.confirm) {
    toast.error('Las contraseñas no coinciden')
    return
  }
  
  if (passwordForm.value.new.length < 8) {
    toast.error('La contraseña debe tener al menos 8 caracteres')
    return
  }
  
  loading.value = true
  try {
    await authStore.changePassword(passwordForm.value.current, passwordForm.value.new)
    toast.success('Contraseña cambiada correctamente. Inicia sesión nuevamente.')
    showChangePassword.value = false
    passwordForm.value = { current: '', new: '', confirm: '' }
  } catch (error: any) {
    toast.error(error.message || 'Error al cambiar contraseña')
  } finally {
    loading.value = false
  }
}
</script>

<style scoped src="@/assets/styles/layouts/app-shell.css"></style>
  <el-dropdown trigger="click">
    <div class="user-menu__trigger" style="display: flex; align-items: center; gap: var(--spacing-sm); cursor: pointer; padding: var(--spacing-xs); border-radius: var(--radius-md);">
      <div class="user-menu__avatar" style="width: 36px; height: 36px; border-radius: 50%; background: var(--color-primary); color: white; display: flex; align-items: center; justify-content: center; font-weight: 600; font-size: 0.875rem;">
        {{ authStore.user?.nombre?.charAt(0).toUpperCase() || 'U' }}
      </div>
      <span class="user-menu__name" style="font-weight: 500; color: var(--color-text-primary);">{{ authStore.user?.nombre }}</span>
      <el-icon style="color: var(--color-text-tertiary);"><User /></el-icon>
    </div>
    
    <template #dropdown>
      <el-dropdown-menu>
        <el-dropdown-item :disabled="true" style="padding: var(--spacing-md); border-bottom: 1px solid var(--color-border);">
          <div style="font-weight: 600; color: var(--color-text-primary);">{{ authStore.user?.nombre }}</div>
          <div style="font-size: 0.75rem; color: var(--color-text-tertiary);">{{ authStore.user?.email }}</div>
          <el-tag :type="authStore.userRole === 'Admin' ? 'danger' : authStore.userRole === 'Supervisor' ? 'warning' : 'info'" style="margin-top: var(--spacing-xs); font-size: 0.625rem;">{{ authStore.userRole }}</el-tag>
        </el-dropdown-item>
        
        <el-dropdown-item @click="showChangePassword = true">
          <el-icon><Lock /></el-icon>
          <span>Cambiar contraseña</span>
        </el-dropdown-item>
        
        <el-dropdown-item divided @click="handleLogout">
          <el-icon><Logout /></el-icon>
          <span style="color: var(--color-danger);">Cerrar sesión</span>
        </el-dropdown-item>
      </el-dropdown-menu>
    </template>
  </el-dropdown>
  
  <!-- Modal Cambiar Contraseña -->
  <el-dialog v-model="showChangePassword" title="Cambiar Contraseña" width="400px" :fullscreen="isMobile" :before-close="() => { passwordForm.value = { current: '', new: '', confirm: '' }; return true; }">
    <form @submit.prevent="handleChangePassword" class="form">
      <el-form-item label="Contraseña actual">
        <el-input v-model="passwordForm.current" type="password" placeholder="Contraseña actual" autocomplete="current-password" />
      </el-form-item>
      <el-form-item label="Nueva contraseña">
        <el-input v-model="passwordForm.new" type="password" placeholder="Nueva contraseña (mín. 8 caracteres)" autocomplete="new-password" />
      </el-form-item>
      <el-form-item label="Confirmar nueva contraseña">
        <el-input v-model="passwordForm.confirm" type="password" placeholder="Repetir nueva contraseña" autocomplete="new-password" />
      </el-form-item>
    </form>
    <template #footer>
      <div style="display: flex; justify-content: flex-end; gap: var(--spacing-sm);">
        <el-button @click="showChangePassword = false">Cancelar</el-button>
        <el-button type="primary" :loading="loading" @click="handleChangePassword">Guardar</el-button>
      </div>
    </template>
  </el-dialog>
</template>