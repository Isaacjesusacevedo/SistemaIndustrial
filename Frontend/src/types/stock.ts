export interface StockSemana {
  id: number;
  semanaId: number;
  producto: string;
  unidadMedida: string;
  stockInicial: number;
  entradas: number;
  salidas: number;
  stockFinal: number;
  fechaCreacion: string;
}

export interface StockCreateDto {
  semanaId: number;
  producto: string;
  unidadMedida: string;
  stockInicial: number;
}

export interface StockUpdateDto {
  producto: string;
  unidadMedida: string;
  stockInicial: number;
  entradas: number;
  salidas: number;
}

export interface StockResponse {
  id: number;
  semanaId: number;
  producto: string;
  unidadMedida: string;
  stockInicial: number;
  entradas: number;
  salidas: number;
  stockFinal: number;
  fechaCreacion: string;
}