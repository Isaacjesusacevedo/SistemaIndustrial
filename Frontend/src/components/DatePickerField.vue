<!-- components/DatePickerField.vue -->
<script setup lang="ts">
import { computed } from 'vue'

interface Props {
  label: string
  modelValue: string | Date
  type?: 'date' | 'datetime-local' | 'month' | 'week' | 'time'
  placeholder?: string
  error?: string
  hint?: string
  required?: boolean
  disabled?: boolean
  readonly?: boolean
  autocomplete?: string
  min?: string
  max?: string
}

const props = defineProps<Props>()
const emit = defineEmits<{ 'update:modelValue': [value: string] }>()

const inputId = computed(() => `date-${props.label.toLowerCase().replace(/\s+/g, '-')}`)
</script>

<style scoped src="@/assets/styles/components/forms.css"></style>
  <div class="form-field" :class="{ 'form-field--error': error, 'form-field--required': required, 'form-field--disabled': disabled }">
    <label :for="inputId" class="form-field__label">
      {{ label }}
      <span v-if="required" class="form-field__asterisk" aria-hidden="true">*</span>
    </label>
    
    <div class="form-field__input-wrapper">
      <input
        :id="inputId"
        :type="type || 'date'"
        :value="typeof modelValue === 'string' ? modelValue : modelValue ? new Date(modelValue).toISOString().slice(0, type === 'datetime-local' ? 16 : 10) : ''"
        @input="emit('update:modelValue', $event.target.value)"
        :placeholder="placeholder"
        :disabled="disabled"
        :readonly="readonly"
        :autocomplete="autocomplete"
        :min="min"
        :max="max"
        :aria-invalid="!!error"
        :aria-describedby="error ? `${inputId}-error` : hint ? `${inputId}-hint` : undefined"
        class="form-field__input"
      />
    </div>
    
    <span v-if="error" :id="`${inputId}-error`" class="form-field__error" role="alert">{{ error }}</span>
    <span v-else-if="hint" :id="`${inputId}-hint`" class="form-field__hint">{{ hint }}</span>
  </div>
</template>