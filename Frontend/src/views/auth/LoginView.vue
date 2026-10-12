<!-- views/auth/LoginView.vue -->
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

<style scoped src="@/assets/styles/views/auth.css"></style>