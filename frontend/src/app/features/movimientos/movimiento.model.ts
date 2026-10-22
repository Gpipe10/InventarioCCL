export interface EntradaRequest {
  productoId: number;
  cantidad: number;
  costoUnitario: number;
  proveedor: string | null;
  motivo: string | null;
  referencia: string | null;
  fecha: string | null;
}

export interface SalidaRequest {
  productoId: number;
  cantidad: number;
  motivo: string;
  referencia: string | null;
  fecha: string | null;
}

export interface MovimientoRegistrado {
  id: number;
}