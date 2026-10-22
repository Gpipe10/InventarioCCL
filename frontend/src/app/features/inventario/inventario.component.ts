import { DecimalPipe } from '@angular/common';
import {
  ChangeDetectorRef,
  Component,
  inject,
  OnInit,
} from '@angular/core';
import { FormsModule } from '@angular/forms';

import { Page } from '../../core/models/page.model';
import { ApiErrorService } from '../../core/services/api-error.service';
import { ExportService } from '../../core/services/export.service';
import { FeedbackComponent } from '../../shared/components/feedback/feedback.component';

import { InventoryRow } from './inventario.model';
import { InventarioService } from './inventario.service';

@Component({
  selector: 'app-inventario',
  standalone: true,
  imports: [
    DecimalPipe,
    FormsModule,
    FeedbackComponent,
  ],
  template: `
    <h1>Inventario actual</h1>

    <section class="card filters">
      <input
        aria-label="Buscar inventario"
        [(ngModel)]="busqueda"
        (keyup.enter)="buscar()"
        placeholder="SKU o nombre" />

      <label class="check">
        <input
          type="checkbox"
          [(ngModel)]="soloBajoStock"
          (change)="buscar()" />
        Solo bajo mínimo
      </label>

      <button
        type="button"
        (click)="buscar()">
        Buscar
      </button>

      <button
        type="button"
        class="secondary"
        [disabled]="exportando"
        (click)="exportar()">
        {{ exportando ? 'Preparando archivo…' : 'Exportar CSV' }}
      </button>
    </section>

    <app-feedback [message]="error" />

    @if (cargando) {
      <p>Cargando inventario…</p>
    }

    <section class="card table-wrap">
      <table>
        <thead>
          <tr>
            <th>SKU</th>
            <th>Producto</th>
            <th>Categoría</th>
            <th>Unidad</th>
            <th>Stock</th>
            <th>Mínimo</th>
            <th>Costo prom.</th>
            <th>Valor</th>
            <th>Estado</th>
          </tr>
        </thead>
        <tbody>
          @for (producto of pagina?.items ?? []; track producto.productoId) {
            <tr [class.low]="producto.bajoStock">
              <td>{{ producto.sku }}</td>
              <td>{{ producto.nombre }}</td>
              <td>{{ producto.categoria }}</td>
              <td>{{ producto.unidadMedida }}</td>
              <td>{{ producto.stock | number:'1.0-2' }}</td>
              <td>{{ producto.stockMinimo | number:'1.0-2' }}</td>
              <td>{{ producto.costoPromedio | number:'1.2-2' }}</td>
              <td>{{ producto.valorInventario | number:'1.2-2' }}</td>
              <td>
                {{
                  producto.bajoStock
                    ? 'Bajo mínimo'
                    : (producto.activo ? 'Normal' : 'Inactivo')
                }}
              </td>
            </tr>
          } @empty {
            <tr>
              <td colspan="9">Sin resultados.</td>
            </tr>
          }
        </tbody>
      </table>
    </section>

    <div class="pagination">
      <button
        type="button"
        class="secondary"
        [disabled]="numero <= 1 || cargando"
        (click)="ir(numero - 1)">
        Anterior
      </button>

      <span>
        Página {{ numero }} ·
        {{ pagina?.total ?? 0 }} registros
      </span>

      <button
        type="button"
        class="secondary"
        [disabled]="!pagina || numero * 20 >= pagina.total || cargando"
        (click)="ir(numero + 1)">
        Siguiente
      </button>
    </div>
  `,
})
export class InventarioComponent implements OnInit {
  private readonly inventarioService = inject(InventarioService);
  private readonly exportService = inject(ExportService);
  private readonly apiError = inject(ApiErrorService);
  private readonly cdr = inject(ChangeDetectorRef);

  pagina: Page<InventoryRow> | null = null;

  busqueda = '';
  soloBajoStock = false;
  numero = 1;

  cargando = false;
  exportando = false;
  error = '';

  ngOnInit(): void {
    this.cargar();
  }

  buscar(): void {
    this.numero = 1;
    this.cargar();
  }

  ir(numero: number): void {
    this.numero = numero;
    this.cargar();
  }

  cargar(): void {
    this.error = '';
    this.cargando = true;
    this.cdr.markForCheck();

    this.inventarioService.listar({
      pagina: this.numero,
      tamano: 20,
      busqueda: this.busqueda,
      soloBajoStock: this.soloBajoStock,
    }).subscribe({
      next: resultado => {
        this.pagina = resultado;
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

  async exportar(): Promise<void> {
    if (this.exportando) {
      return;
    }

    this.exportando = true;
    this.error = '';
    this.cdr.markForCheck();

    try {
      await this.exportService.exportarInventario({
        busqueda: this.busqueda.trim(),
        soloBajoStock: this.soloBajoStock,
      });
    } catch (error) {
      this.error = this.apiError.obtenerMensaje(error);
    } finally {
      this.exportando = false;
      this.cdr.markForCheck();
    }
  }
}
