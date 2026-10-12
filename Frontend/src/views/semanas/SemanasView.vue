<script setup lang="ts">
import { onMounted, computed } from 'vue'
import { RouterLink } from 'vue-router'
import { useSemanasStore } from '@/stores/semanas'
import { useSemanaActivaStore } from '@/stores/SemanaActiva'
import { useToast } from '@/composables/useToast'
import { ElMessageBox } from 'element-plus'
import { Delete } from '@element-plus/icons-vue'

const store = useSemanasStore()
const semanaActiva = useSemanaActivaStore()
const toast = useToast()

const semanas = computed(() => store.items)

const formatDate = (d: string) =>
  new Date(d).toLocaleDateString('es-AR', { day: '2-digit', month: 'short', year: 'numeric' })

const diasEntreFechas = (inicio: string, fin: string) => {
  const diff = new Date(fin).getTime() - new Date(inicio).getTime()
  return Math.round(diff / (1000 * 60 * 60 * 24)) + 1
}

const confirmDelete = async (id: number) => {
  const semana = semanas.value.find(s => s.id === id)
  const label = semana
    ? `Semana ${semana.numeroSemana}/${semana.anio}`
    : 'esta semana'

  const confirmed = await ElMessageBox.confirm(
    `¿Eliminar ${label}? Se perderán todos sus datos asociados (insumos, stock, registros y productos).`,
    'Confirmar eliminación',
    {
      confirmButtonText: 'Eliminar',
      cancelButtonText: 'Cancelar',
      type: 'warning'
    }
  ).catch(() => false)

  if (!confirmed) return

  try {
    await store.delete(id)             // ✅ el store expone `delete`
    toast.success('Semana eliminada')

    // Si era la semana activa, limpiarla y re-seleccionar la más reciente
    if (semanaActiva.semanaId === id) {
      semanaActiva.setSemana(null)
      await semanaActiva.inicializar()
    }
  } catch (e: any) {
    toast.error(e.message || 'Error al eliminar la semana')
  }
}

onMounted(() => store.fetchAll())
</script>

<template>
  <div class="page">
    <div class="page-header">
      <div class="page-eyebrow">Producción</div>
      <div style="display:flex; align-items:center; justify-content:space-between">
        <h1 class="page-title">Semanas registradas</h1>
        <RouterLink to="/semanas/nueva" class="btn btn-primary">
          <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5">
            <path d="M12 5v14M5 12h14"/>
          </svg>
          Nueva semana
        </RouterLink>
      </div>
      <p class="page-subtitle">Historial completo de producción semanal</p>
    </div>

    <!-- Error -->
    <div v-if="store.error" class="alert alert-error" style="margin-bottom:24px">
      {{ store.error }}
    </div>

    <!-- Loading -->
    <div v-if="store.loading" style="display:flex; flex-direction:column; gap:12px">
      <div v-for="i in 4" :key="i" class="skeleton" style="height:72px"></div>
    </div>

    <!-- Vacío -->
    <div v-else-if="semanas.length === 0" class="card" style="text-align:center; padding:60px">
      <svg width="40" height="40" viewBox="0 0 24 24" fill="none" stroke="var(--text-3)" stroke-width="1.5" style="margin:0 auto 16px">
        <path d="M8 2v4M16 2v4M3 10h18M5 4h14a2 2 0 0 1 2 2v14a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2V6a2 2 0 0 1 2-2z"/>
      </svg>
      <p style="color:var(--text-3); font-size:14px">No hay semanas registradas todavía</p>
      <RouterLink to="/semanas/nueva" class="btn btn-primary" style="margin-top:20px">
        Registrar primera semana
      </RouterLink>
    </div>

    <!-- Tabla -->
    <div v-else class="table-wrap">
      <table>
        <thead>
          <tr>
            <th>Semana</th>
            <th>Período</th>
            <th class="num">Duración</th>
            <th></th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="s in semanas" :key="s.id">
            <td>
              <span style="font-family:var(--font-mono); font-size:18px; font-weight:500; color:var(--text-0)">
                #{{ String(s.numeroSemana).padStart(2, '0') }}
              </span>
              <span
                v-if="semanaActiva.semanaId === s.id"
                class="badge badge-blue"
                style="margin-left:8px; font-size:10px;">
                ACTIVA
              </span>
            </td>
            <td>
              <span style="color:var(--text-1)">{{ formatDate(s.fechaInicio) }}</span>
              <span style="color:var(--text-3); margin:0 8px">→</span>
              <span style="color:var(--text-1)">{{ formatDate(s.fechaFin) }}</span>
            </td>
            <td class="num">
              <span class="badge badge-blue">{{ diasEntreFechas(s.fechaInicio, s.fechaFin) }} días</span>
            </td>
            <td style="text-align:right; display:flex; gap:8px; justify-content:flex-end;">
              <RouterLink :to="`/semanas/${s.id}`" class="btn btn-ghost" style="padding:6px 14px; font-size:13px">
                Ver detalle →
              </RouterLink>
              <button
                @click="confirmDelete(s.id)"
                class="btn btn-ghost"
                style="padding:6px 10px; font-size:13px; color:var(--red); border-color:rgba(239,68,68,0.3);"
                title="Eliminar semana">
                <Delete style="width:14px; height:14px;" />
              </button>
            </td>
          </tr>
        </tbody>
      </table>
    </div>
  </div>
</template>