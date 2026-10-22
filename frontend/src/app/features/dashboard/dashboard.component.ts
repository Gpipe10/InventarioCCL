import { DecimalPipe } from '@angular/common';
import { ChangeDetectorRef, Component, inject, OnInit } from '@angular/core';

import { ApiErrorService } from '../../core/services/api-error.service';
import { FeedbackComponent } from '../../shared/components/feedback/feedback.component';
import { DashboardData } from './dashboard.model';
import { DashboardService } from './dashboard.service';

@Component({
  selector: 'app-dashboard',
  imports: [DecimalPipe, FeedbackComponent],
  template: `
    <div class="dashboard-heading">
      <div>
        <h1>Panel de inventario</h1>
        <p>Indicadores generales y actividad de los últimos seis meses.</p>
      </div>
    </div>

    <app-feedback [message]="error" />

    @if (cargando) {
      <p>Cargando indicadores...</p>
    }

    @if (data; as dashboard) {
      <div class="cards summary-cards">
        <article class="card summary-card">
          <span>Productos activos</span>
          <strong>{{ dashboard.productosActivos }}</strong>
          <small>Productos disponibles en el catálogo</small>
        </article>

        <article class="card summary-card">
          <span>Stock agregado</span>
          <strong>{{ dashboard.stockTotal | number:'1.0-2' }}</strong>
          <small>Suma referencial de existencias</small>
        </article>

        <article class="card summary-card">
          <span>Valor del inventario</span>
          <strong>{{ dashboard.valorInventario | number:'1.2-2' }}</strong>
          <small>Valor según costo de inventario</small>
        </article>

        <article class="card summary-card" [class.warning-card]="dashboard.productosBajoStock > 0">
          <span>Productos bajo mínimo</span>
          <strong>{{ dashboard.productosBajoStock }}</strong>
          <small>Productos que requieren revisión</small>
        </article>
      </div>

      <div class="charts-grid">
        <section class="card chart-card">
          <div class="chart-header">
            <div>
              <h2>Movimientos por mes</h2>
              <p>Número de operaciones de entrada y salida</p>
            </div>
            <div class="legend">
              <span><i class="legend-dot entry"></i> Entradas</span>
              <span><i class="legend-dot exit"></i> Salidas</span>
            </div>
          </div>

          <div class="bar-chart" role="img" aria-label="Comparación mensual de operaciones de entrada y salida">
            @for (mes of dashboard.movimientosPorMes; track mes.mes) {
              <div class="month-group">
                <div class="bar-pair">
                  <div class="bar-track">
                    <div
                      class="bar entry-bar"
                      [style.height.%]="alturaBarra(mes.entradas)"
                      [title]="mes.mes + ': ' + mes.entradas + ' entradas'">
                    </div>
                  </div>
                  <div class="bar-track">
                    <div
                      class="bar exit-bar"
                      [style.height.%]="alturaBarra(mes.salidas)"
                      [title]="mes.mes + ': ' + mes.salidas + ' salidas'">
                    </div>
                  </div>
                </div>
                <span class="month-label">{{ nombreMes(mes.mes) }}</span>
                <small class="month-year">{{ mes.mes.slice(0, 4) }}</small>
              </div>
            } @empty {
              <p>Sin movimientos registrados.</p>
            }
          </div>
        </section>

        <section class="card chart-card balance-card">
          <div class="chart-header">
            <div>
              <h2>Actividad del periodo</h2>
              <p>Distribución de operaciones registradas</p>
            </div>
          </div>

          <div class="donut-layout">
            <div
              class="donut"
              [style.background]="fondoDonut()"
              role="img"
              [attr.aria-label]="'Entradas: ' + totalEntradas() + ', salidas: ' + totalSalidas()">
              <div class="donut-hole">
                <strong>{{ totalMovimientos() }}</strong>
                <span>movimientos</span>
              </div>
            </div>

            <div class="donut-stats">
              <div class="stat-line">
                <span><i class="legend-dot entry"></i> Entradas</span>
                <strong>{{ totalEntradas() }}</strong>
              </div>
              <div class="stat-line">
                <span><i class="legend-dot exit"></i> Salidas</span>
                <strong>{{ totalSalidas() }}</strong>
              </div>
              <p class="chart-note">Se cuentan operaciones, no cantidades de productos.</p>
            </div>
          </div>
        </section>
      </div>

      <section class="card history-card">
        <h2>Detalle mensual</h2>
        <div class="table-wrap">
          <table>
            <thead>
              <tr>
                <th>Mes</th>
                <th>Entradas</th>
                <th>Salidas</th>
                <th>Total de operaciones</th>
              </tr>
            </thead>
            <tbody>
              @for (mes of dashboard.movimientosPorMes; track mes.mes) {
                <tr>
                  <td>{{ mes.mes }}</td>
                  <td>{{ mes.entradas }}</td>
                  <td>{{ mes.salidas }}</td>
                  <td>{{ mes.entradas + mes.salidas }}</td>
                </tr>
              } @empty {
                <tr><td colspan="4">Sin movimientos registrados.</td></tr>
              }
            </tbody>
          </table>
        </div>
      </section>
    }
  `,
  styles: [`
    .dashboard-heading { display: flex; justify-content: space-between; margin-bottom: 20px; }
    .dashboard-heading h1 { margin-bottom: 6px; }
    .dashboard-heading p, .chart-header p { margin: 0; color: #61708a; font-size: .88rem; }
    .summary-cards { margin-bottom: 22px; }
    .summary-card { display: flex; flex-direction: column; gap: 9px; border-top: 3px solid #00449e; }
    .summary-card strong { color: #141d38; font-size: clamp(1.65rem, 2vw, 2.25rem); }
    .summary-card small { color: #63718a; }
    .summary-card.warning-card { border-top-color: #dc9136; }
    .charts-grid { display: grid; grid-template-columns: minmax(0, 1.55fr) minmax(280px, 1fr); gap: 20px; margin-bottom: 22px; }
    .chart-card { min-width: 0; }
    .chart-header { display: flex; justify-content: space-between; align-items: flex-start; flex-wrap: wrap; gap: 12px; margin-bottom: 22px; }
    .chart-header h2, .history-card h2 { margin: 0 0 7px; }
    .legend { display: flex; flex-wrap: wrap; gap: 13px; font-size: .82rem; color: #59667b; }
    .legend span, .stat-line span { display: inline-flex; gap: 7px; align-items: center; }
    .legend-dot { width: 10px; height: 10px; display: inline-block; border-radius: 3px; }
    .legend-dot.entry { background: #00449e; }
    .legend-dot.exit { background: #60c659; }
    .bar-chart { display: flex; gap: 10px; align-items: flex-end; justify-content: space-around; min-height: 230px; border-bottom: 1px solid #dce4f0; padding: 8px 0 0; }
    .month-group { display: flex; flex: 1; min-width: 36px; align-items: center; flex-direction: column; gap: 3px; }
    .bar-pair { height: 180px; width: 100%; display: flex; justify-content: center; align-items: flex-end; gap: 5px; }
    .bar-track { width: min(22px, 35%); height: 100%; display: flex; flex-direction: column; justify-content: flex-end; }
    .bar { width: 100%; min-height: 0; border-radius: 6px 6px 0 0; transition: height .2s ease; }
    .entry-bar { background: #00449e; }
    .exit-bar { background: #60c659; }
    .month-label { margin-top: 7px; color: #253653; font-weight: 600; font-size: .83rem; }
    .month-year { color: #758097; font-size: .73rem; }
    .donut-layout { display: flex; align-items: center; justify-content: center; gap: 24px; min-height: 215px; }
    .donut { width: 170px; height: 170px; flex: 0 0 170px; border-radius: 50%; display: grid; place-items: center; }
    .donut-hole { width: 115px; height: 115px; border-radius: 50%; background: #fff; display: flex; justify-content: center; align-items: center; flex-direction: column; gap: 4px; }
    .donut-hole strong { color: #141d38; font-size: 2rem; }
    .donut-hole span { font-size: .74rem; color: #66758c; }
    .donut-stats { flex: 1; min-width: 125px; }
    .stat-line { display: flex; justify-content: space-between; gap: 14px; padding: 10px 0; border-bottom: 1px solid #e5eaf2; }
    .stat-line strong { color: #141d38; }
    .chart-note { color: #697890; font-size: .78rem; line-height: 1.5; margin-top: 15px; }
    @media (max-width: 950px) {
      .charts-grid { grid-template-columns: 1fr; }
    }
    @media (max-width: 520px) {
      .donut-layout { flex-direction: column; }
      .donut-stats { width: 100%; }
      .bar-chart { gap: 3px; }
      .bar-pair { gap: 2px; }
    }
  `],
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
