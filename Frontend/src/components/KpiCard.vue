<!-- components/KpiCard.vue -->
<script setup lang="ts">
import { computed } from 'vue'

interface Props {
  title: string
  value: string | number
  trend?: number
  trendLabel?: string
  icon?: string
  color?: 'primary' | 'success' | 'warning' | 'danger' | 'info'
}

const props = withDefaults(defineProps<Props>(), {
  trend: 0,
  color: 'primary'
})

const trendClass = computed(() => {
  if (props.trend > 0) return 'kpi-card__trend--up'
  if (props.trend < 0) return 'kpi-card__trend--down'
  return 'kpi-card__trend--neutral'
})

const trendIcon = computed(() => {
  if (props.trend > 0) return '↑'
  if (props.trend < 0) return '↓'
  return '→'
})
</script>

<template>
  <div class="kpi-card" :class="`kpi-card--${color}`">
    <div class="kpi-card__header">
      <h3 class="kpi-card__title">{{ title }}</h3>
      <span v-if="icon" class="kpi-card__icon">{{ icon }}</span>
    </div>
    <div class="kpi-card__value">{{ value }}</div>
    <div v-if="trend !== 0" class="kpi-card__trend" :class="trendClass">
      <span class="kpi-card__trend-icon">{{ trendIcon }}</span>
      <span class="kpi-card__trend-value">{{ Math.abs(trend) }}%</span>
      <span v-if="trendLabel" class="kpi-card__trend-label">{{ trendLabel }}</span>
    </div>
  </div>
</template>

<style scoped src="@/assets/styles/components/cards.css"></style>