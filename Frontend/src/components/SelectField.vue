<!-- components/SelectField.vue -->
<script setup lang="ts">
import { computed } from 'vue'

interface Props {
  label: string
  modelValue: any
  options: { value: any; label: string }[]
  placeholder?: string
  error?: string
  hint?: string
  required?: boolean
  disabled?: boolean
  autocomplete?: string
}

const props = defineProps<Props>()
const emit = defineEmits<{ 'update:modelValue': [value: any] }>()

const inputId = computed(() => `select-${props.label.toLowerCase().replace(/\s+/g, '-')}`)
</script>

<template>
  <div class="form-field" :class="{ 'form-field--error': error, 'form-field--required': required, 'form-field--disabled': disabled }">
    <label :for="inputId" class="form-field__label">
      {{ label }}
      <span v-if="required" class="form-field__asterisk" aria-hidden="true">*</span>
    </label>
    
    <div class="form-field__input-wrapper">
      <select
        :id="inputId"
        :value="modelValue"
        @change="emit('update:modelValue', $event.target.value)"
        :disabled="disabled"
        :aria-invalid="!!error"
        :aria-describedby="error ? `${inputId}-error` : hint ? `${inputId}-hint` : undefined"
        class="form-field__input form-field__input--select"
      >
        <option v-if="placeholder" value="" disabled>{{ placeholder }}</option>
        <option v-for="opt in options" :key="opt.value" :value="opt.value">{{ opt.label }}</option>
      </select>
    </div>
    
    <span v-if="error" :id="`${inputId}-error`" class="form-field__error" role="alert">{{ error }}</span>
    <span v-else-if="hint" :id="`${inputId}-hint`" class="form-field__hint">{{ hint }}</span>
  </div>
</template>

<style scoped src="@/assets/styles/components/forms.css"></style>