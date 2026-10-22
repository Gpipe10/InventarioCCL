import { DecimalPipe } from '@angular/common';
import { Component, computed, input } from '@angular/core';

import { ActividadMensual } from '../../../features/dashboard/dashboard.model';

@Component({
  selector: 'app-dashboard-charts',
  standalone: true,
  imports: [DecimalPipe],
  template: `
    <div class="chart-grid">

      <section class="card chart-card">
        <div class="chart-head">
          <h2>Stock por unidad</h2>
          <span>Distribución actual</span>
        </div>

        @for (item of stockBars(); track item.label) {

          <div class="chart-row">
            <div class="chart-label">
              <span>{{ item.label }}</span>
              <strong>{{ item.value | number:'1.0-2' }}</strong>
            </div>

            <div
              class="chart-track"
              role="img"
              [attr.aria-label]="item.label + ': ' + item.value">
              <span
                class="chart-fill"
                [style.width.%]="item.percent">
              </span>
            </div>
          </div>

        } @empty {
          <p class="empty-state">
            Aún no hay existencias para graficar.
          </p>
        }
      </section>

      <section class="card chart-card">
        <div class="chart-head">
          <h2>Actividad mensual</h2>
          <span>Últimos 6 meses disponibles</span>
        </div>

        @for (item of monthBars(); track item.mes) {

          <div class="month-row">
            <span class="month-label">{{ item.mes }}</span>

            <div class="month-bars">
              <div
                class="chart-track"
                role="img"
                [attr.aria-label]="'Entradas ' + item.mes + ': ' + item.entradas">
                <span
                  class="chart-fill"
                  [style.width.%]="item.inPercent">
                </span>
              </div>

              <div
                class="chart-track"
                role="img"
                [attr.aria-label]="'Salidas ' + item.mes + ': ' + item.salidas">
                <span
                  class="chart-fill chart-fill--green"
                  [style.width.%]="item.outPercent">
                </span>
              </div>
            </div>

            <strong>
              {{ item.entradas }} / {{ item.salidas }}
            </strong>
          </div>

        } @empty {
          <p class="empty-state">
            Todavía no hay movimientos.
          </p>
        }

        <p class="chart-legend">
          <span class="legend-blue"></span>
          Entradas
          <span class="legend-green"></span>
          Salidas
        </p>

      </section>

    </div>
  `,
})
export class DashboardChartsComponent {

  readonly stockPorUnidad =
    input<Record<string, number>>({});

  readonly movimientosPorMes =
    input<ActividadMensual[]>([]);

  readonly stockBars = computed(() => {
    const entries = Object.entries(this.stockPorUnidad());

    const max = Math.max(
      1,
      ...entries.map(([, value]) => value)
    );

    return entries.map(([label, value]) => ({
      label,
      value,
      percent: Math.max(0, value / max * 100),
    }));
  });

  readonly monthBars = computed(() => {
    const months = this.movimientosPorMes().slice(-6);

    const max = Math.max(
      1,
      ...months.flatMap(month => [
        month.entradas,
        month.salidas,
      ])
    );

    return months.map(month => ({
      ...month,
      inPercent: month.entradas / max * 100,
      outPercent: month.salidas / max * 100,
    }));
  });
}