import { HttpClient } from '@angular/common/http';

import {
  inject,
  Injectable,
} from '@angular/core';

import { Observable } from 'rxjs';

import { API } from '../../core/config/api.config';

import {
  EntradaRequest,
  SalidaRequest,
  MovimientoRegistrado,
} from './movimiento.model';

@Injectable({
  providedIn: 'root',
})
export class MovimientosService {

  private readonly http = inject(HttpClient);

  // ======================================================
  // REGISTRAR ENTRADA
  // ======================================================

  registrarEntrada(
    request: EntradaRequest
  ): Observable<MovimientoRegistrado> {

    return this.http.post<MovimientoRegistrado>(
      `${API}/movimientos/entradas`,
      request
    );
  }

  // ======================================================
  // REGISTRAR SALIDA
  // ======================================================

  registrarSalida(
    request: SalidaRequest
  ): Observable<MovimientoRegistrado> {

    return this.http.post<MovimientoRegistrado>(
      `${API}/movimientos/salidas`,
      request
    );
  }
}