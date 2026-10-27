import { DecimalPipe } from '@angular/common';
import { Component, computed, input } from '@angular/core';

import { ActividadMensual } from '../../../features/dashboard/dashboard.model';

@Component({
  selector: 'app-dashboard-charts',
  standalone: true,
  imports: [DecimalPipe],
  templateUrl: './dashboard-charts.component.html',
  styleUrl: './dashboard-charts.component.css',
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