export interface InsumoSemana {
  id: number;
  semanaId: number;
  nombre: string;
  unidadMedida: string;
  cantidadRequerida: number;
  cantidadConsumida: number;
  stockDisponible: number;
  fechaCreacion: string;
}

export interface InsumoCreateDto {
  semanaId: number;
  nombre: string;
  unidadMedida: string;
  cantidadRequerida: number;
}

export interface InsumoUpdateDto {
  nombre: string;
  unidadMedida: string;
  cantidadRequerida: number;
  cantidadConsumida: number;
  stockDisponible: number;
}

export interface InsumoResponse {
  id: number;
  semanaId: number;
  nombre: string;
  unidadMedida: string;
  cantidadRequerida: number;
  cantidadConsumida: number;
  stockDisponible: number;
  fechaCreacion: string;
}