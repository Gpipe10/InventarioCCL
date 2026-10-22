export interface InventoryRow {
  productoId: number;
  sku: string;
  nombre: string;
  categoria: string;
  unidadMedida: string;
  precio: number;
  stock: number;
  stockMinimo: number;
  costoPromedio: number;
  valorInventario: number;
  bajoStock: boolean;
  activo: boolean;
}

export interface KardexRow {
  movimientoId: number;
  productoId: number;
  sku: string;
  producto: string;
  tipo: number;
  cantidad: number;
  costoUnitario: number;
  valor: number;
  stockResultante: number;
  proveedor: string | null;
  motivo: string | null;
  referencia: string | null;
  fecha: string;
  registradoEn: string;
  usuario: string;
}

export interface InventarioFiltro {
  pagina: number;
  tamano: number;
  busqueda?: string;
  soloBajoStock?: boolean;
}

export interface KardexFiltro {
  pagina: number;
  tamano: number;
  productoId?: number | null;
  tipo?: number | null;
  desde?: string;
  hasta?: string;
}