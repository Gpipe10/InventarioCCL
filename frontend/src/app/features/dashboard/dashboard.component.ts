import { DecimalPipe } from '@angular/common';
import { ChangeDetectorRef, Component, inject, OnInit } from '@angular/core';

import { ApiErrorService } from '../../core/services/api-error.service';
import { FeedbackComponent } from '../../shared/components/feedback/feedback.component';
import { DashboardData } from './dashboard.model';
import { DashboardService } from './dashboard.service';

@Component({
  selector: 'app-dashboard',
  imports: [DecimalPipe, FeedbackComponent],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.css',
})
export class DashboardComponent implements OnInit {
  private readonly dashboardService = inject(DashboardService);
  private readonly apiError = inject(ApiErrorService);
  private readonly cdr = inject(ChangeDetectorRef);

  data: DashboardData | null = null;
  cargando = false;
  error = '';

  ngOnInit(): void {
    this.cargar();
  }

  cargar(): void {
    this.error = '';
    this.cargando = true;
    this.dashboardService.obtenerResumen().subscribe({
      next: resultado => {
        this.data = resultado;
        this.cargando = false;
        this.cdr.markForCheck();
      },
      error: error => {
        this.error = this.apiError.obtenerMensaje(error);
        this.cargando = false;
        this.cdr.markForCheck();
      },
    });
  }

  private maximoMensual(): number {
    return Math.max(
      1,
      ...(this.data?.movimientosPorMes ?? []).flatMap(mes => [mes.entradas, mes.salidas]),
    );
  }

  alturaBarra(valor: number): number {
    return Math.max(0, Math.min(100, (valor / this.maximoMensual()) * 100));
  }

  nombreMes(mes: string): string {
    const meses = ['Ene', 'Feb', 'Mar', 'Abr', 'May', 'Jun', 'Jul', 'Ago', 'Sep', 'Oct', 'Nov', 'Dic'];
    const indice = Number(mes.slice(5, 7)) - 1;
    return meses[indice] ?? mes;
  }

  totalEntradas(): number {
    return (this.data?.movimientosPorMes ?? []).reduce((total, mes) => total + mes.entradas, 0);
  }

  totalSalidas(): number {
    return (this.data?.movimientosPorMes ?? []).reduce((total, mes) => total + mes.salidas, 0);
  }

  totalMovimientos(): number {
    return this.totalEntradas() + this.totalSalidas();
  }

  fondoDonut(): string {
    const total = this.totalMovimientos();
    if (total === 0) {
      return '#e5eaf2';
    }
    const entradas = (this.totalEntradas() / total) * 100;
    return `conic-gradient(#00449e 0% ${entradas}%, #60c659 ${entradas}% 100%)`;
  }
}
