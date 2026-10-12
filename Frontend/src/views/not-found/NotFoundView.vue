<!-- views/not-found/NotFoundView.vue -->
<script setup lang="ts">
import { useRouter } from 'vue-router'
import { House, ArrowLeft } from '@element-plus/icons-vue'

const router = useRouter()

const goHome = () => router.push({ name: 'semanas' })
const goBack = () => router.back()
</script>

<template>
  <div class="not-found">
    <div class="not-found__content">
      <div class="not-found__code">404</div>
      <h1 class="not-found__title">Página no encontrada</h1>
      <p class="not-found__message">
        Lo sentimos, la página que buscas no existe o ha sido movida.
      </p>
      <div class="not-found__actions">
        <router-link to="/" class="btn btn-primary">
          <House /> Volver al inicio
        </router-link>
        <button @click="goBack" class="btn btn-secondary">
          <ArrowLeft /> Volver atrás
        </button>
      </div>
    </div>
  </div>
</template>

<style scoped src="@/assets/styles/views/not-found.css"></style>