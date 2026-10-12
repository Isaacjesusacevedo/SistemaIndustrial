<!-- components/FormField.vue -->
<script setup lang="ts">
import { computed } from 'vue'

interface Props {
  label: string
  modelValue: any
  type?: 'number' | 'select' | 'textarea' | 'text' | 'email' | 'password' | 'date' | 'time' | 'datetime-local'
  options?: { value: any; label: string }[]
  placeholder?: string
  error?: string
  hint?: string
  required?: boolean
  disabled?: boolean
  readonly?: boolean
  autocomplete?: string
  rows?: number
}

const props = defineProps<Props>()
const emit = defineEmits<{ 'update:modelValue': [value: any] }>()

const inputId = computed(() =>
  `field-${props.label.toLowerCase().replace(/\s+/g, '-')}`
)
</script>

<template>
  <div
    class="form-field"
    :class="{
      'form-field--error': error,
      'form-field--required': required,
      'form-field--disabled': disabled
    }">
    <label :for="inputId" class="form-field__label">
      {{ label }}
      <span v-if="required" class="form-field__asterisk" aria-hidden="true">*</span>
    </label>

    <div class="form-field__input-wrapper">
      <!-- ✅ input genérico: text/number/email/password/date/time/datetime-local -->
      <input
        v-if="type !== 'select' && type !== 'textarea'"
        :id="inputId"
        :type="type || 'text'"
        :value="modelValue"
        @input="emit('update:modelValue', ($event.target as HTMLInputElement).value)"
        :placeholder="placeholder"
        :disabled="disabled"
        :readonly="readonly"
        :autocomplete="autocomplete"
        :aria-invalid="!!error"
        :aria-describedby="error ? `${inputId}-error` : hint ? `${inputId}-hint` : undefined"
        class="form-field__input"
      />

      <!-- ✅ select -->
      <select
        v-else-if="type === 'select'"
        :id="inputId"
        :value="modelValue"
        @change="emit('update:modelValue', ($event.target as HTMLSelectElement).value)"
        :disabled="disabled"
        :aria-invalid="!!error"
        class="form-field__input">
        <option v-if="placeholder" value="" disabled>{{ placeholder }}</option>
        <option v-for="opt in options" :key="opt.value" :value="opt.value">
          {{ opt.label }}
        </option>
      </select>

      <!-- ✅ textarea -->
      <textarea
        v-else
        :id="inputId"
        :value="modelValue"
        @input="emit('update:modelValue', ($event.target as HTMLTextAreaElement).value)"
        :placeholder="placeholder"
        :disabled="disabled"
        :readonly="readonly"
        :rows="rows || 4"
        :aria-invalid="!!error"
        class="form-field__input form-field__input--textarea" />
    </div>

    <span
      v-if="error"
      :id="`${inputId}-error`"
      class="form-field__error"
      role="alert">
      {{ error }}
    </span>
    <span v-else-if="hint" :id="`${inputId}-hint`" class="form-field__hint">
      {{ hint }}
    </span>
  </div>
</template>

<style scoped src="@/assets/styles/components/forms.css"></style>