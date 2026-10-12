<!-- views/semanas/DetalleSemanaView.vue -->
<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useSemanasStore } from '@/stores/semanas'
import { useInsumosStore } from '@/stores/insumos'
import { useStockStore } from '@/stores/stock'
import { useTiradoProductosStore } from '@/stores/tiradoProductos'
import { useToast } from '@/composables/useToast'
import { useAuthStore } from '@/stores/auth'
import { ElMessageBox } from 'element-plus'
// ✅ Nombres correctos de iconos
import { ArrowLeft, Edit, Delete, Box, TakeawayBox, TrendCharts } from '@element-plus/icons-vue'
import KpiCard from '@/components/KpiCard.vue'

// ── Helpers locales ─────────────────────────────────────────────────
const formatDate = (d: string | Date) =>
  new Date(d).toLocaleDateString('es-AR', { day: '2-digit', month: 'short', year: 'numeric' })

const formatNumber = (n: number | null | undefined) =>
  (n ?? 0).toLocaleString('es-AR')

const router = useRouter()
const route = useRoute()
const store = useSemanasStore()
const insumosStore = useInsumosStore()
const stockStore = useStockStore()
const tiradoStore = useTiradoProductosStore()
const authStore = useAuthStore()
const toast = useToast()

const semanaId = computed(() => Number(route.params.id))
const semana = ref<any>(null)
const loading = ref(false)
const activeTab = ref('stock')

const tabs = [
  { key: 'insumos',       label: 'Insumos',        icon: Box },
  { key: 'stock',         label: 'Stock',          icon: TakeawayBox },
  { key: 'tirado',        label: 'Tirado',         icon: TrendCharts }
]

// ── Progreso de la semana (cobertura promedio) ───────────────────────
const totalProductos = computed(() => tiradoStore.items.length)

const progresoSemana = computed(() => {
  const items = tiradoStore.items
  if (items.length === 0) return 0

  const coberturas = items.map(item => {
    if (!item.pedidoSemanal || item.pedidoSemanal <= 0) return 100
    const disponible = item.stockInicial + item.cantidadTirada
    return Math.min(100, (disponible / item.pedidoSemanal) * 100)
  })

  return Math.round(coberturas.reduce((a, b) => a + b, 0) / items.length)
})

const progresoColor = computed(() => {
  if (progresoSemana.value >= 80) return '#22c55e'
  if (progresoSemana.value >= 50) return '#f59e0b'
  return '#ef4444'
})

// ── KPI Tirado ──────────────────────────────────────────────────────
const productosCompletos = computed(() => {
  return tiradoStore.items.filter(item => {
    if (!item.pedidoSemanal || item.pedidoSemanal <= 0) return true
    const disponible = item.stockInicial + item.cantidadTirada
    return disponible >= item.pedidoSemanal
  }).length
})

const tiradoTrend = computed(() =>
  totalProductos.value > 0 ? Math.round((productosCompletos.value / totalProductos.value) * 100) : 0
)

const tiradoColor = computed(() => {
  if (totalProductos.value === 0 || productosCompletos.value === 0) return 'danger'
  if (productosCompletos.value < totalProductos.value) return 'warning'
  return 'success'
})

const clasePorSigno = (n: number) =>
  n < 0 ? 'text-danger' : n === 0 ? 'text-success' : 'text-warning'

// ── Tab Tirado estilo planilla ──────────────────────────────────────
const vistaExpandida = ref(false)

type DiaLaboral = 'sabado' | 'lunes' | 'martes' | 'miercoles' | 'jueves' | 'viernes'

const DIAS_EXPANDIDOS: { key: DiaLaboral; label: string }[] = [
  { key: 'sabado', label: 'Sábado' },
  { key: 'lunes', label: 'Lunes' },
  { key: 'martes', label: 'Martes' },
  { key: 'miercoles', label: 'Miércoles' },
  { key: 'jueves', label: 'Jueves' },
  { key: 'viernes', label: 'Viernes' }
]

const diaKeyDeFecha = (fechaISO: string): DiaLaboral | null => {
  // Parseo como fecha local (solo YYYY-MM-DD) para evitar corrimientos por timezone
  const m = /^(\d{4})-(\d{2})-(\d{2})/.exec(fechaISO)
  const d = m
    ? new Date(Number(m[1]), Number(m[2]) - 1, Number(m[3]))
    : new Date(fechaISO)
  switch (d.getDay()) {
    case 6: return 'sabado'
    case 1: return 'lunes'
    case 2: return 'martes'
    case 3: return 'miercoles'
    case 4: return 'jueves'
    case 5: return 'viernes'
    default: return null
  }
}

const tiradasPorDia = computed(() => {
  const map: Record<number, Record<DiaLaboral, number>> = {}
  for (const t of tiradoStore.tachosSemana) {
    const dia = diaKeyDeFecha(t.fecha)
    if (!dia) continue
    if (!map[t.tiradoProductoId]) {
      map[t.tiradoProductoId] = { sabado: 0, lunes: 0, martes: 0, miercoles: 0, jueves: 0, viernes: 0 }
    }
    map[t.tiradoProductoId][dia] += t.cantidad ?? 0
  }
  return map
})

const cantidadDia = (productoId: number, dia: DiaLaboral) =>
  tiradasPorDia.value[productoId]?.[dia] ?? 0

const NOMBRES_DIA_CORTO: Record<string, string> = {
  lunes: 'Lun', martes: 'Mar', miercoles: 'Mié', jueves: 'Jue', viernes: 'Vie', sabado: 'Sáb'
}

// Último día con tiradas en la semana (por fecha de tirada más reciente)
const ultimoDiaConActividad = computed((): DiaLaboral | null => {
  const tiradas = tiradoStore.tachosSemana
  if (tiradas.length === 0) return null

  const ultima = [...tiradas].sort((a, b) =>
    new Date(b.fecha).getTime() - new Date(a.fecha).getTime()
  )[0]

  if (!ultima) return null
  return diaKeyDeFecha(ultima.fecha)
})

const labelUltimoDia = computed(() => {
  if (!ultimoDiaConActividad.value) return 'Últ. día'
  return `Últ. día (${NOMBRES_DIA_CORTO[ultimoDiaConActividad.value] ?? ''})`
})

// Cantidad tirada del último día con actividad para un producto
const tiradaUltimoDia = (productoId: number): number => {
  if (!ultimoDiaConActividad.value) return 0
  return cantidadDia(productoId, ultimoDiaConActividad.value)
}

const totalUltimoDia = computed(() =>
  tiradoStore.items.reduce((s, p) => s + tiradaUltimoDia(p.id), 0)
)

const tachosNetos = (item: { cantidadTirada: number; promedioPorTacho: number }): number => {
  if (!item.promedioPorTacho || item.promedioPorTacho <= 0) return 0
  return Math.max(0, Math.round(item.cantidadTirada / item.promedioPorTacho))
}

const totalTachosNetos = computed(() =>
  tiradoStore.items.reduce((s, p) => s + tachosNetos(p), 0)
)

const txtCantidad = (n: number) => (n === 0 ? '—' : formatNumber(n))

const clsTachos = (n: number) => (n < 0 ? 'text-danger' : n > 0 ? 'text-success' : '')

const totalPedido = computed(() =>
  tiradoStore.items.reduce((s, p) => s + (p.pedidoSemanal ?? 0), 0)
)

const totalStockInicial = computed(() =>
  tiradoStore.items.reduce((s, p) => s + (p.stockInicial ?? 0), 0)
)

const totalStock = computed(() =>
  tiradoStore.items.reduce((s, p) => s + (p.stockTotal ?? 0), 0)
)

const totalTachos = computed(() =>
  Math.round(tiradoStore.items.reduce((s, p) => s + (p.tachosEstimados ?? 0), 0))
)

const totalTachosReales = computed(() =>
  tiradoStore.items.reduce((s, p) => s + (p.cantidadTachos ?? 0), 0)
)

const totalDia = (dia: DiaLaboral) =>
  tiradoStore.items.reduce((s, p) => s + cantidadDia(p.id, dia), 0)

const cargarDetalle = async () => {
  loading.value = true
  try {
    const [s] = await Promise.all([
      store.fetchById(semanaId.value),
      insumosStore.fetchBySemana(semanaId.value),
      stockStore.fetchBySemana(semanaId.value),
      tiradoStore.cargarPorSemana(semanaId.value),
      tiradoStore.cargarTachosSemana(semanaId.value)
    ])
    semana.value = s
  } catch (e: any) {
    toast.error(e.message || 'Error al cargar detalle')
  } finally {
    loading.value = false
  }
}

const handleDelete = async () => {
  const confirmed = await ElMessageBox.confirm(
    '¿Eliminar esta semana? Se perderán todos sus datos asociados.',
    'Confirmar',
    { confirmButtonText: 'Eliminar', cancelButtonText: 'Cancelar', type: 'warning' }
  ).catch(() => false)

  if (!confirmed) return

  try {
    await store.delete(semanaId.value)
    toast.success('Semana eliminada')
    router.push({ name: 'semanas' })
  } catch (e: any) {
    toast.error(e.message || 'Error al eliminar')
  }
}

const handleEdit = () => {
  router.push({ name: 'editar-semana', params: { id: semanaId.value } })
}

const handleNuevoInsumo   = () => router.push({ name: 'insumos',        params: { semanaId: semanaId.value } })
const handleNuevoStock    = () => router.push({ name: 'stock',          params: { semanaId: semanaId.value } })

onMounted(cargarDetalle)
</script>

<template>
  <div class="section">
    <div class="section__header" v-if="!loading && semana">
      <div>
        <h1>Semana {{ semana.numeroSemana }}/{{ semana.anio }}</h1>
        <p class="section__subtitle">
          {{ formatDate(semana.fechaInicio) }} - {{ formatDate(semana.fechaFin) }} · {{ semana.estado }}
        </p>
      </div>
      <div class="section__actions">
        <router-link :to="{ name: 'semanas' }" class="btn btn-secondary">
          <ArrowLeft /> Volver
        </router-link>
        <router-link
          :to="{ name: 'editar-semana', params: { id: semana.id } }"
          class="btn btn-secondary"
          v-if="authStore.isSupervisor">
          <Edit /> Editar
        </router-link>
        <el-button type="danger" @click="handleDelete" v-if="authStore.isAdmin">
          <Delete /> Eliminar
        </el-button>
      </div>
    </div>

    <div v-if="loading" class="loading-center"><Spinner /></div>

    <div v-else-if="semana">
      <!-- Progreso de la semana -->
      <div class="card" style="margin-bottom: var(--spacing-lg);">
        <div style="display:flex; align-items:center; justify-content:space-between; gap:var(--spacing-lg);">
          <div style="flex:1; min-width:0;">
            <h2 style="margin:0;">Progreso de la semana</h2>
            <p class="section__subtitle">Cobertura promedio de la semana</p>
            <el-progress
              :percentage="progresoSemana"
              :color="progresoColor"
              :stroke-width="16"
              :show-text="false"
              style="margin-top:var(--spacing-md);" />
          </div>
          <div style="font-size:2.5rem; font-weight:700; line-height:1;" :style="{ color: progresoColor }">
            {{ progresoSemana }}%
          </div>
        </div>
      </div>

      <!-- KPIs resumen -->
      <div class="kpi-grid" style="margin-bottom: var(--spacing-lg);">
        <KpiCard title="Semana Activa" :value="`#${semana.numeroSemana}/${semana.anio}`" icon="📅" color="primary" />
        <KpiCard title="Insumos"         :value="insumosStore.items.length"       icon="📦" color="info" />
        <KpiCard title="Stock"           :value="stockStore.items.length"         icon="📦" color="success" />
        <KpiCard
          title="Tirado"
          :value="`${productosCompletos}/${totalProductos}`"
          :trend="tiradoTrend"
          trendLabel="% completos"
          icon="🛢️"
          :color="tiradoColor" />
      </div>

      <div class="card">
      <!-- Tabs -->
      <el-tabs v-model="activeTab" type="card" style="margin-bottom: var(--spacing-lg);">
        <!-- Stock -->
        <el-tab-pane label="Stock" name="stock">
          <div class="section__actions" style="margin-bottom: var(--spacing-md);">
            <el-button type="primary" @click="handleNuevoStock">Nuevo Stock</el-button>
          </div>
          <div class="table-container">
            <table class="table">
              <thead>
                <tr>
                  <th>Producto</th>
                  <th>Unidad</th>
                  <th class="num">Inicial</th>
                  <th class="num">Entradas</th>
                  <th class="num">Salidas</th>
                  <th class="num">Final</th>
                </tr>
              </thead>
              <tbody>
                <tr v-for="s in stockStore.items" :key="s.id">
                  <td>{{ s.producto }}</td>
                  <td>{{ s.unidadMedida }}</td>
                  <td class="num">{{ formatNumber(s.stockInicial) }}</td>
                  <td class="num">{{ formatNumber(s.entradas) }}</td>
                  <td class="num">{{ formatNumber(s.salidas) }}</td>
                  <td class="num">{{ formatNumber(s.stockFinal) }}</td>
                </tr>
                <tr v-if="stockStore.items.length === 0">
                  <td colspan="6" style="text-align:center; color:var(--text-3); padding:24px">
                    Sin stock registrado
                  </td>
                </tr>
              </tbody>
            </table>
          </div>
        </el-tab-pane>

        <!-- Tirado -->
        <el-tab-pane label="Tirado" name="tirado">
          <div class="section__actions" style="margin-bottom: var(--spacing-md); justify-content: space-between;">
            <router-link :to="{ name: 'tirado', params: { semanaId } }" class="btn btn-secondary">
              Ver Tirado completo →
            </router-link>
            <el-button type="primary" @click="vistaExpandida = !vistaExpandida">
              {{ vistaExpandida ? 'Vista del día' : 'Ver semana completa →' }}
            </el-button>
          </div>

          <!-- Vista compacta: solo el día actual -->
          <div v-if="!vistaExpandida" class="table-container">
            <table v-if="tiradoStore.items.length > 0" class="table">
              <thead>
                <tr>
                  <th>Producto</th>
                  <th class="num">Pedido</th>
                  <th class="num">Stock Inicial</th>
                  <th class="num" style="width:110px">{{ labelUltimoDia }}</th>
                  <th class="num">Stock Total</th>
                  <th class="num">Tachos</th>
                  <th class="num">Tachos Est.</th>
                </tr>
              </thead>
              <tbody>
                <tr v-for="p in tiradoStore.items" :key="p.id">
                  <td><strong>{{ p.nombre }}</strong></td>
                  <td class="num">{{ txtCantidad(p.pedidoSemanal) }}</td>
                  <td class="num">{{ txtCantidad(p.stockInicial) }}</td>
                  <td class="num" style="width:110px">
                    <span v-if="tiradaUltimoDia(p.id) > 0" style="font-family: var(--font-mono); font-weight:600;">
                      {{ formatNumber(tiradaUltimoDia(p.id)) }}
                    </span>
                    <span v-else style="color:var(--text-3);">—</span>
                  </td>
                  <td class="num">
                    <span :class="clasePorSigno(p.stockTotal)">{{ formatNumber(p.stockTotal) }}</span>
                  </td>
                  <td class="num">
                    <span class="badge">{{ tachosNetos(p) }}</span>
                  </td>
                  <td class="num">
                    <span :class="clsTachos(p.tachosEstimados)" :style="p.tachosEstimados === 0 ? 'color:var(--text-3)' : ''">{{ Math.round(p.tachosEstimados) }}</span>
                  </td>
                </tr>
              </tbody>
              <tfoot>
                <tr style="background:var(--bg-3); font-weight:700;">
                  <td>TOTAL</td>
                  <td class="num">{{ txtCantidad(totalPedido) }}</td>
                  <td class="num">{{ txtCantidad(totalStockInicial) }}</td>
                  <td class="num">{{ txtCantidad(totalUltimoDia) }}</td>
                  <td class="num">{{ formatNumber(totalStock) }}</td>
                  <td class="num">{{ totalTachosNetos }}</td>
                  <td class="num">{{ totalTachos }}</td>
                </tr>
              </tfoot>
            </table>
            <div
              v-else
              style="text-align:center; color:var(--text-3); padding:24px">
              No hay productos tirado registrados para esta semana
            </div>
          </div>

          <!-- Vista expandida: todos los días (Sáb a Vie) -->
          <div v-else class="table-container">
            <table v-if="tiradoStore.items.length > 0" class="table">
              <thead>
                <tr>
                  <th>Producto</th>
                  <th class="num">Pedido</th>
                  <th class="num">Stock Inicial</th>
                  <th v-for="d in DIAS_EXPANDIDOS" :key="d.key" class="num" style="width:70px">{{ d.label }}</th>
                  <th class="num">Stock Total</th>
                  <th class="num">Tachos</th>
                  <th class="num">Tachos Est.</th>
                </tr>
              </thead>
              <tbody>
                <tr v-for="p in tiradoStore.items" :key="p.id">
                  <td><strong>{{ p.nombre }}</strong></td>
                  <td class="num">{{ txtCantidad(p.pedidoSemanal) }}</td>
                  <td class="num">{{ txtCantidad(p.stockInicial) }}</td>
                  <td
                    v-for="d in DIAS_EXPANDIDOS"
                    :key="d.key"
                    class="num"
                    style="width:70px"
                    :style="cantidadDia(p.id, d.key) === 0 ? 'color:var(--text-3)' : ''">
                    {{ txtCantidad(cantidadDia(p.id, d.key)) }}
                  </td>
                  <td class="num">
                    <span :class="clasePorSigno(p.stockTotal)">{{ formatNumber(p.stockTotal) }}</span>
                  </td>
                  <td class="num">
                    <span class="badge">{{ tachosNetos(p) }}</span>
                  </td>
                  <td class="num">
                    <span :class="clsTachos(p.tachosEstimados)" :style="p.tachosEstimados === 0 ? 'color:var(--text-3)' : ''">{{ Math.round(p.tachosEstimados) }}</span>
                  </td>
                </tr>
              </tbody>
              <tfoot>
                <tr style="background:var(--bg-3); font-weight:700;">
                  <td>TOTAL</td>
                  <td class="num">{{ txtCantidad(totalPedido) }}</td>
                  <td class="num">{{ txtCantidad(totalStockInicial) }}</td>
                  <td v-for="d in DIAS_EXPANDIDOS" :key="d.key" class="num">{{ txtCantidad(totalDia(d.key)) }}</td>
                  <td class="num">{{ formatNumber(totalStock) }}</td>
                  <td class="num">{{ totalTachosNetos }}</td>
                  <td class="num">{{ totalTachos }}</td>
                </tr>
              </tfoot>
            </table>
            <div
              v-else
              style="text-align:center; color:var(--text-3); padding:24px">
              No hay productos tirado registrados para esta semana
            </div>
          </div>
        </el-tab-pane>

        <!-- Insumos -->
        <el-tab-pane label="Insumos" name="insumos">
          <div class="section__actions" style="margin-bottom: var(--spacing-md);">
            <el-button type="primary" @click="handleNuevoInsumo">Nuevo Insumo</el-button>
          </div>
          <div class="table-container">
            <table class="table">
              <thead>
                <tr>
                  <th>Insumo</th>
                  <th>Unidad</th>
                  <th class="num">Requerido</th>
                  <th class="num">Consumido</th>
                  <th class="num">Stock</th>
                </tr>
              </thead>
              <tbody>
                <tr v-for="i in insumosStore.items" :key="i.id">
                  <td>{{ i.nombre }}</td>
                  <td>{{ i.unidadMedida }}</td>
                  <td class="num">{{ formatNumber(i.cantidadRequerida) }}</td>
                  <td class="num">{{ formatNumber(i.cantidadConsumida) }}</td>
                  <td class="num">{{ formatNumber(i.stockDisponible) }}</td>
                </tr>
                <tr v-if="insumosStore.items.length === 0">
                  <td colspan="5" style="text-align:center; color:var(--text-3); padding:24px">
                    Sin insumos registrados
                  </td>
                </tr>
              </tbody>
            </table>
          </div>
        </el-tab-pane>
      </el-tabs>
      </div>
    </div>

    <div v-else class="loading-center"><Spinner /></div>
  </div>
</template>
