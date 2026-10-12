export interface Semana {
  id: number;
  numeroSemana: number;
  anio: number;
  fechaInicio: string;
  fechaFin: string;
  estado: 'Planificada' | 'EnProgreso' | 'Completada' | 'Cancelada';
  fechaCreacion: string;
  insumosCount?: number;
  registrosTachoCount?: number;
}

export interface SemanaCreateDto {
  numeroSemana: number;
  anio: number;
  fechaInicio: string;
  fechaFin: string;
}

export interface SemanaUpdateDto {
  numeroSemana: number;
  anio: number;
  fechaInicio: string;
  fechaFin: string;
  estado: 'Planificada' | 'EnProgreso' | 'Completada' | 'Cancelada';
}

export interface SemanaResponse {
  id: number;
  numeroSemana: number;
  anio: number;
  fechaInicio: string;
  fechaFin: string;
  estado: string;
  fechaCreacion: string;
  insumosCount?: number;
  registrosTachoCount?: number;
}