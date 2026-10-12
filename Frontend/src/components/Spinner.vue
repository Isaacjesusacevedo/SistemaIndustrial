<!-- components/Spinner.vue -->
<script setup lang="ts">
interface Props {
  size?: 'sm' | 'md' | 'lg'
  color?: string
}

const props = withDefaults(defineProps<Props>(), {
  size: 'md',
  color: 'currentColor'
})

const sizeClasses = {
  sm: 'spinner--sm',
  md: 'spinner--md',
  lg: 'spinner--lg'
}
</script>

<template>
  <div 
    class="spinner" 
    :class="sizeClasses[size]"
    :style="{ borderTopColor: color, borderLeftColor: color, borderRightColor: color, borderBottomColor: 'transparent' }"
    role="status"
    aria-label="Cargando..."
  >
    <span class="sr-only">Cargando...</span>
  </div>
</template>

<style scoped src="@/assets/styles/components/spinner.css"></style>