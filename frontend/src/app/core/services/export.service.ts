import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { firstValueFrom } from 'rxjs';

import { API } from '../config/api.config';
import { Page } from '../models/page.model';

import {
  InventoryRow,
  KardexRow,
} from '../../features/inventario/inventario.model';

type Filters = Record<
  string,
  string | number | boolean | null | undefined
>;

type Column<T> = {
  title: string;
  value: (row: T) => string | number | null | undefined;
};

@Injectable({
  providedIn: 'root',
})
export class ExportService {

  private readonly http = inject(HttpClient);

  exportarInventario(filtros: Filters = {}): Promise<void> {
    return this.exportar<InventoryRow>(
      'inventario',
      filtros,
      'inventario',
      [
        { title: 'SKU', value: x => x.sku },
        { title: 'Producto', value: x => x.nombre },
        { title: 'Categoría', value: x => x.categoria },
        { title: 'Unidad', value: x => x.unidadMedida },
        { title: 'Stock', value: x => x.stock },
        { title: 'Stock mínimo', value: x => x.stockMinimo },
        { title: 'Costo promedio', value: x => x.costoPromedio },
        { title: 'Valor inventario', value: x => x.valorInventario },
        { title: 'Bajo stock', value: x => x.bajoStock ? 'Sí' : 'No' },
      ]
    );
  }

  exportarKardex(filtros: Filters = {}): Promise<void> {
    return this.exportar<KardexRow>(
      'inventario/kardex',
      filtros,
      'kardex',
      [
        { title: 'Fecha UTC', value: x => x.fecha },
        { title: 'SKU', value: x => x.sku },
        { title: 'Producto', value: x => x.producto },
        { title: 'Tipo', value: x => x.tipo === 1 ? 'Entrada' : 'Salida' },
        { title: 'Cantidad', value: x => x.cantidad },
        { title: 'Costo unitario', value: x => x.costoUnitario },
        { title: 'Valor', value: x => x.valor },
        { title: 'Stock resultante', value: x => x.stockResultante },
        { title: 'Motivo', value: x => x.motivo },
        { title: 'Referencia', value: x => x.referencia },
        { title: 'Usuario', value: x => x.usuario },
      ]
    );
  }

  private async exportar<T>(
    path: string,
    filtros: Filters,
    nombre: string,
    columns: Column<T>[]
  ): Promise<void> {

    const all: T[] = [];
    let pageNumber = 1;

    for (;;) {
      let params = new HttpParams()
        .set('pagina', pageNumber)
        .set('tamano', 100);

      for (const [key, value] of Object.entries(filtros)) {
        if (value !== undefined && value !== null && value !== '') {
          params = params.set(key, value);
        }
      }

      const page = await firstValueFrom(
        this.http.get<Page<T>>(
          `${API}/${path}`,
          { params }
        )
      );

      all.push(...page.items);

      if (all.length >= page.total || page.items.length === 0) {
        break;
      }

      pageNumber++;
    }

    const cell = (
      value: string | number | null | undefined
    ): string => {

      let text = value == null ? '' : String(value);

      if (
        typeof value === 'string' &&
        /^[\s]*[=+@-]/.test(text)
      ) {
        text = `'${text}`;
      }

      return `"${text.replace(/"/g, '""')}"`;
    };

    const lines = [
      columns.map(column => cell(column.title)).join(';'),
      ...all.map(row =>
        columns.map(column => cell(column.value(row))).join(';')
      ),
    ];

    const blob = new Blob(
      ['\uFEFF', lines.join('\r\n')],
      { type: 'text/csv;charset=utf-8' }
    );

    const url = URL.createObjectURL(blob);
    const link = document.createElement('a');

    link.href = url;
    link.download =
      `${nombre}-${new Date().toISOString().slice(0, 10)}.csv`;

    document.body.append(link);
    link.click();
    link.remove();

    setTimeout(() => URL.revokeObjectURL(url), 1000);
  }
}