export interface ActividadMensual {
  mes: string;
  entradas: number;
  salidas: number;
}

export interface DashboardData {
  productosActivos: number;
  stockTotal: number;
  valorInventario: number;
  productosBajoStock: number;
  stockPorUnidad: Record<string, number>;
  movimientosPorMes: ActividadMensual[];
}