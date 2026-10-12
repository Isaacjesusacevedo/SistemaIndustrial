<!-- components/ChartContainer.vue -->
<script setup lang="ts">
import { ref, onMounted, onUnmounted, watch } from 'vue'

interface Props {
  title: string
  height?: number
  type?: 'line' | 'bar' | 'doughnut' | 'pie'
}

const props = withDefaults(defineProps<Props>(), {
  height: 300,
  type: 'line'
})

const emit = defineEmits<{ render: [ctx: CanvasRenderingContext2D] }>()

const canvasRef = ref<HTMLCanvasElement | null>(null)
let chartInstance: any = null

const initChart = () => {
  if (!canvasRef.value) return
  
  const ctx = canvasRef.value.getContext('2d')
  if (!ctx) return
  
  // Clear any existing chart
  if (chartInstance) {
    chartInstance.destroy()
  }
  
  // Emit the context for parent to create chart
  emit('render', ctx)
}

onMounted(() => {
  initChart()
})

onUnmounted(() => {
  if (chartInstance) {
    chartInstance.destroy()
  }
})

watch(() => props.type, () => {
  initChart()
})
</script>

<template>
  <div class="chart-container" :style="{ height: `${height}px` }">
    <h3 class="chart-container__title">{{ title }}</h3>
    <canvas 
      ref="canvasRef" 
      class="chart-container__canvas"
      :height="height"
      :width="width"
    />
  </div>
</template>

<style scoped src="@/assets/styles/components/chart-container.css"></style>