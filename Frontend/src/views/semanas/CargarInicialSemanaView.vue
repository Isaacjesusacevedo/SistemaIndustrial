<!-- views/semanas/CargarInicialSemanaView.vue -->
<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { useSemanasStore } from '@/stores/semanas'
import { useProductosCatalogoStore } from '@/stores/productosCatalogo'
import { useTiradoProductosStore } from '@/stores/tiradoProductos'
import { useToast } from '@/composables/useToast'
import { formatDate } from '@/utils/date'
import type { ProductoCatalogo } from '@/types/productoCatalogo'

const router = useRouter()
const route = useRoute()
const semanasStore = useSemanasStore()
const catalogoStore = useProductosCatalogoStore()
const tiradoStore = useTiradoProductosStore()
const toast = useToast()

const semanaId = computed(() => Number(route.params.id))
const loading = ref(false)
const guardando = ref(false)

const semana = ref<any>(null)

// Fila: producto del catálogo + inputs del usuario
interface FilaInicial {
  producto: ProductoCatalogo
  stockInicial: number
  pedidoSemanal: number
}

const filas = ref<FilaInicial[]>([])

// Cargar
const cargar = async () => {
  loading.value = true
  try {
    const [s] = await Promise.all([
      semanasStore.fetchById(semanaId.value),
      catalogoStore.fetchActivos()
    ])
    semana.value = s

    // Construir filas con valores por defecto
    filas.value = catalogoStore.activos.map((p: ProductoCatalogo) => ({
      producto: p,
      stockInicial: 0,
      pedidoSemanal: p.pedidoSemanalAproximado
    }))
  } catch (e: any) {
    toast.error(e.message || 'Error al cargar datos')
  } finally {
    loading.value = false
  }
}

// Guardar
const guardar = async () => {
  // Validar
  const invalidos = filas.value.filter(f =>
    f.stockInicial < 0 || f.pedidoSemanal < 0
  )
  if (invalidos.length > 0) {
    toast.error('Ningún valor puede ser negativo')
    return
  }

  const conDatos = filas.value.filter(f =>
    f.stockInicial > 0 || f.pedidoSemanal > 0
  )

  if (conDatos.length === 0) {
    toast.error('Ingresá al menos un stock o pedido')
    return
  }

  guardando.value = true
  try {
    const result = await tiradoStore.createBulk(semanaId.value, {
      items: conDatos.map(f => ({
        nombre: f.producto.nombre,
        unidadMedida: f.producto.unidadMedida,
        promedioPorTacho: Number(f.producto.promedioPorTacho),
        stockInicial: Number(f.stockInicial),
        pedidoSemanal: Number(f.pedidoSemanal),
        categoriaId: null,
        observaciones: null
      }))
    })

    toast.success(result.message || `${result.data?.creados ?? 0} productos cargados`)

    // Redirigir al detalle
    router.push({ name: 'detalle-semana', params: { id: semanaId.value } })
  } catch (e: any) {
    toast.error(e.message || 'Error al guardar')
  } finally {
    guardando.value = false
  }
}

const saltar = () => {
  router.push({ name: 'detalle-semana', params: { id: semanaId.value } })
}

onMounted(cargar)
</script>

<template>
  <div class="section">
    <div class="section__header">
      <div>
        <div style="font-size:0.75rem; color:var(--text-3); letter-spacing:0.05em; text-transform:uppercase; margin-bottom:4px;">
          Paso 2 de 2
        </div>
        <h1>Cargar datos iniciales</h1>
        <p class="section__subtitle" v-if="semana">
          Semana {{ semana.numeroSemana }}/{{ semana.anio }} ·
          {{ formatDate(semana.fechaInicio) }} al {{ formatDate(semana.fechaFin) }}
        </p>
      </div>
    </div>

    <div v-if="loading" class="loading-center"><Spinner /></div>

    <div v-else>
      <div class="card" style="margin-bottom: var(--spacing-md); padding: var(--spacing-md); background: var(--bg-3);">
        <p style="margin:0; color: var(--text-1); font-size:0.875rem;">
          Definí el <strong>stock inicial</strong> y el <strong>pedido semanal</strong> de cada producto.
          Podés dejar en 0 los que no apliquen.
        </p>
      </div>

      <div v-if="filas.length === 0" class="card" style="text-align:center; padding:60px;">
        <p style="color:var(--text-3);">
          No hay productos en el catálogo.
          <router-link to="/productos" style="color:var(--accent);">
            Crear productos primero
          </router-link>
        </p>
      </div>

      <div v-else class="table-wrap">
        <table style="width:100%;">
          <thead>
            <tr>
              <th style="text-align:left;">Producto</th>
              <th>Unidad</th>
              <th class="num">Stock inicial</th>
              <th class="num">Pedido semanal</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="fila in filas" :key="fila.producto.id">
              <td><strong>{{ fila.producto.nombre }}</strong></td>
              <td>{{ fila.producto.unidadMedida }}</td>
              <td class="num">
                <input
                  type="number"
                  v-model.number="fila.stockInicial"
                  min="0"
                  step="1"
                  style="width:120px; text-align:right; padding:6px 10px; background: var(--bg-1); border:1px solid var(--border); border-radius: var(--radius-md); color: var(--text-0); font-family: var(--font-mono);"
                />
              </td>
              <td class="num">
                <input
                  type="number"
                  v-model.number="fila.pedidoSemanal"
                  min="0"
                  step="1"
                  style="width:120px; text-align:right; padding:6px 10px; background: var(--bg-1); border:1px solid var(--border); border-radius: var(--radius-md); color: var(--text-0); font-family: var(--font-mono);"
                />
              </td>
            </tr>
          </tbody>
        </table>
      </div>

      <div style="display:flex; justify-content: space-between; margin-top: var(--spacing-lg);">
        <button @click="saltar" class="btn btn-secondary" :disabled="guardando">
          Saltar por ahora
        </button>
        <button @click="guardar" class="btn btn-primary" :disabled="guardando || filas.length === 0">
          <span v-if="guardando">Guardando...</span>
          <span v-else>Guardar y continuar</span>
        </button>
      </div>
    </div>
  </div>
</template>
