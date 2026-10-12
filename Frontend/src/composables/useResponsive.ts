import { ref, onMounted, onUnmounted, computed } from 'vue'

export function useResponsive() {
  const width = ref(window.innerWidth)

  const updateWidth = () => {
    width.value = window.innerWidth
  }

  onMounted(() => {
    window.addEventListener('resize', updateWidth)
  })

  onUnmounted(() => {
    window.removeEventListener('resize', updateWidth)
  })

  return {
    width,
    isMobile: computed(() => width.value <= 640),
    isTablet: computed(() => width.value > 640 && width.value <= 1024),
    isDesktop: computed(() => width.value > 1024),
  }
}
