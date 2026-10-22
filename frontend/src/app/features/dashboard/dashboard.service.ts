import { HttpClient } from '@angular/common/http';

import {
  inject,
  Injectable,
} from '@angular/core';

import { Observable } from 'rxjs';

import { API } from '../../core/config/api.config';

import { DashboardData } from './dashboard.model';

@Injectable({
  providedIn: 'root',
})
export class DashboardService {

  private readonly http = inject(HttpClient);

  // ======================================================
  // CONSULTAR INDICADORES
  // ======================================================

  obtenerResumen(): Observable<DashboardData> {

    return this.http.get<DashboardData>(
      `${API}/inventario/dashboard`
    );
  }
}