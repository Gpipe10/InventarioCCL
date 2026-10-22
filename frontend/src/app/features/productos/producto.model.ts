export interface Category {
  id: number;
  nombre: string;
}

export interface Product {
  id: number;
  sku: string;
  nombre: string;
  descripcion: string | null;
  categoriaId: number;
  unidadMedida: number;
  precio: number;
  stockMinimo: number;
  stock: number;
  costoPromedio: number;
  activo: boolean;
  creadoEn: string;
  actualizadoEn: string;
}

export interface ProductData {
  sku: string;
  nombre: string;
  descripcion: string | null;
  categoriaId: number;
  unidadMedida: number;
  precio: number;
  stockMinimo: number;
}