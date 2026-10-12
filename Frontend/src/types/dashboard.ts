export interface DashboardMetricasDto {
  semanaId: number;
  numeroSemana: number;
  anio: number;
  totalProducido: number;
  totalRechazado: number;
  porcentajeCalidad: number;
  insumosConsumidos: number;
  insumosRequeridos: number;
  stockActual: number;
  stockMinimo: number;
  turnosRegistrados: number;
  operadoresUnicos: number;
  tendenciaProduccion: number;
  tendenciaCalidad: number;
  tendenciaStock: number;
}

export interface DashboardTendenciaDto {
  periodo: string;
  produccion: number;
  rechazos: number;
  calidad: number;
}

export interface DashboardAlertaDto {
  tipo: 'StockBajo' | 'CalidadBaja' | 'SinRegistros';
  mensaje: string;
  severidad: 'Info' | 'Warning' | 'Critical';
  semanaId?: number;
  detalle?: string;
}