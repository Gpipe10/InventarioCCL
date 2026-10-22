import {
  HttpClient,
  HttpParams,
} from '@angular/common/http';

import {
  inject,
  Injectable,
} from '@angular/core';

import { Observable } from 'rxjs';

import { API } from '../../core/config/api.config';

import { Page } from '../../core/models/page.model';

import {
  InventoryRow,
  KardexRow,
  InventarioFiltro,
  KardexFiltro,
} from './inventario.model';

@Injectable({
  providedIn: 'root',
})
export class InventarioService {

  private readonly http = inject(HttpClient);

  // ======================================================
  // CONSULTAR INVENTARIO ACTUAL
  // ======================================================

  listar(
    filtro: InventarioFiltro
  ): Observable<Page<InventoryRow>> {

    let params = new HttpParams()
      .set('pagina', filtro.pagina)
      .set('tamano', filtro.tamano)
      .set(
        'soloBajoStock',
        filtro.soloBajoStock ?? false
      );

    if (filtro.busqueda?.trim()) {
      params = params.set(
        'busqueda',
        filtro.busqueda.trim()
      );
    }

    return this.http.get<Page<InventoryRow>>(
      `${API}/inventario`,
      { params }
    );
  }

  // ======================================================
  // CONSULTAR KARDEX
  // ======================================================

  consultarKardex(
    filtro: KardexFiltro
  ): Observable<Page<KardexRow>> {

    let params = new HttpParams()
      .set('pagina', filtro.pagina)
      .set('tamano', filtro.tamano);

    if (filtro.productoId) {
      params = params.set(
        'productoId',
        filtro.productoId
      );
    }

    if (
      filtro.tipo !== null &&
      filtro.tipo !== undefined
    ) {
      params = params.set(
        'tipo',
        filtro.tipo
      );
    }

    if (filtro.desde) {
      params = params.set(
        'desde',
        `${filtro.desde}T00:00:00Z`
      );
    }

    if (filtro.hasta) {
      const siguienteDia = new Date(
        `${filtro.hasta}T00:00:00Z`
      );

      siguienteDia.setUTCDate(
        siguienteDia.getUTCDate() + 1
      );

      params = params.set(
        'hasta',
        siguienteDia.toISOString()
      );
    }

    return this.http.get<Page<KardexRow>>(
      `${API}/inventario/kardex`,
      { params }
    );
  }
}