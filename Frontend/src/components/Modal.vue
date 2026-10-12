<!-- components/Modal.vue -->
<script setup lang="ts">
import { ref, watch } from 'vue'
import { Teleport } from 'vue'

interface Props {
  modelValue: boolean
  title?: string
  size?: 'sm' | 'md' | 'lg' | 'xl' | 'full'
  closeOnOverlayClick?: boolean
  showCloseButton?: boolean
}

const props = withDefaults(defineProps<Props>(), {
  size: 'md',
  closeOnOverlayClick: true,
  showCloseButton: true
})

const emit = defineEmits<{ 'update:modelValue': [value: boolean] }>()

const close = () => emit('update:modelValue', false)

watch(() => props.modelValue, (val) => {
  if (val) document.body.style.overflow = 'hidden'
  else document.body.style.overflow = ''
})
</script>

<template>
  <Teleport to="body">
    <Transition name="modal">
      <div v-if="modelValue" class="modal-overlay" @click.self="closeOnOverlayClick && close">
        <div class="modal" :class="`modal--${size}`" @click.stop>
          <div class="modal__header">
            <h3 class="modal__title">{{ title }}</h3>
            <button v-if="showCloseButton" @click="close" class="modal__close" aria-label="Cerrar">×</button>
          </div>
          <div class="modal__body">
            <slot />
          </div>
          <div class="modal__footer">
            <slot name="footer">
              <button @click="close" class="btn btn-secondary">Cancelar</button>
              <button @click="$emit('confirm')" class="btn btn-primary">Confirmar</button>
            </slot>
          </div>
        </div>
      </div>
    </Transition>
  </Teleport>
</template>

<style scoped src="@/assets/styles/components/modal.css"></style>