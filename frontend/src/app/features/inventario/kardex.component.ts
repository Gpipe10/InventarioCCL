import { DatePipe, DecimalPipe } from '@angular/common';
import { ChangeDetectorRef, Component, inject, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { firstValueFrom } from 'rxjs';

import { Page } from '../../core/models/page.model';
import { ApiErrorService } from '../../core/services/api-error.service';
import { ExportService } from '../../core/services/export.service';
import { FeedbackComponent } from '../../shared/components/feedback/feedback.component';
import { ProductosService } from '../productos/productos.service';

import { KardexRow } from './inventario.model';
import { InventarioService } from './inventario.service';

@Component({
  selector: 'app-kardex',
  standalone: true,
  imports: [
    DatePipe,
    DecimalPipe,
    FormsModule,
    FeedbackComponent,
  ],
  template: `
    <h1>Kardex de movimientos</h1>

    <section class="card filters">

      <label>
        SKU producto
        <input
          type="text"
          [(ngModel)]="skuProducto"
          (keyup.enter)="buscar()"
          placeholder="SKU del producto" />
      </label>

      <label>
        Tipo
        <select [(ngModel)]="tipo">
          <option value="">Todos</option>
          <option value="1">Entradas</option>
          <option value="2">Salidas</option>
        </select>
      </label>

      <label>
        Desde
        <input
          type="date"
          [(ngModel)]="desde" />
      </label>

      <label>
        Hasta
        <input
          type="date"
          [(ngModel)]="hasta" />
      </label>

      <button
        type="button"
        [disabled]="cargando || exportando"
        (click)="buscar()">
        Consultar
      </button>

      <button
        type="button"
        class="secondary"
        [disabled]="exportando || cargando"
        (click)="exportar()">
        {{ exportando ? 'Preparando archivo…' : 'Exportar CSV' }}
      </button>

    </section>

    <app-feedback [message]="error" />

    @if (cargando) {
      <p>Cargando kardex…</p>
    }

    <section class="card table-wrap">

      <table>
        <thead>
          <tr>
            <th>Fecha</th>
            <th>Producto</th>
            <th>Tipo</th>
            <th>Cantidad</th>
            <th>Costo</th>
            <th>Valor</th>
            <th>Stock final</th>
            <th>Referencia</th>
            <th>Usuario</th>
          </tr>
        </thead>

        <tbody>

          @for (movimiento of pagina?.items ?? []; track movimiento.movimientoId) {

            <tr>
              <td>{{ movimiento.fecha | date:'short' }}</td>
              <td>{{ movimiento.sku }} · {{ movimiento.producto }}</td>
              <td>{{ movimiento.tipo === 1 ? 'Entrada' : 'Salida' }}</td>
              <td>{{ movimiento.cantidad | number:'1.0-2' }}</td>
              <td>{{ movimiento.costoUnitario | number:'1.2-2' }}</td>
              <td>{{ movimiento.valor | number:'1.2-2' }}</td>
              <td>{{ movimiento.stockResultante | number:'1.0-2' }}</td>
              <td>{{ movimiento.referencia || '—' }}</td>
              <td>{{ movimiento.usuario }}</td>
            </tr>

          } @empty {
            <tr>
              <td colspan="9">Sin movimientos.</td>
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
export class KardexComponent implements OnInit {

  private readonly inventarioService = inject(InventarioService);
  private readonly productosService = inject(ProductosService);
  private readonly exportService = inject(ExportService);
  private readonly apiError = inject(ApiErrorService);
  private readonly cdr = inject(ChangeDetectorRef);

  pagina: Page<KardexRow> | null = null;

  skuProducto = '';
  tipo = '';
  desde = '';
  hasta = '';

  numero = 1;
  cargando = false;
  exportando = false;
  error = '';

  ngOnInit(): void {
    void this.cargar();
  }

  buscar(): void {
    this.numero = 1;
    void this.cargar();
  }

  ir(numero: number): void {
    this.numero = numero;
    void this.cargar();
  }

  // El formulario recibe el SKU; el backend conserva su filtro por ID.
  // Se resuelve el identificador internamente, sin mostrárselo al usuario.
  private async resolverProductoId(): Promise<number | null> {
    const sku = this.skuProducto.trim();

    if (!sku) {
      return null; // Sin SKU: consultar todos los productos.
    }

    const resultado = await firstValueFrom(
      this.productosService.listar(1, 50, sku)
    );

    const producto = resultado.items.find(
      item => item.sku.trim().toLowerCase() === sku.toLowerCase()
    );

    if (!producto) {
      throw new Error(`No se encontró un producto con el SKU "${sku}".`);
    }

    return producto.id;
  }

  async cargar(): Promise<void> {
    if (this.cargando) {
      return;
    }

    this.error = '';
    this.cargando = true;
    this.cdr.markForCheck();

    try {
      const productoId = await this.resolverProductoId();

      const resultado = await firstValueFrom(
        this.inventarioService.consultarKardex({
          pagina: this.numero,
          tamano: 20,
          productoId,
          tipo: this.tipo ? Number(this.tipo) : null,
          desde: this.desde,
          hasta: this.hasta,
        })
      );

      this.pagina = resultado;
    } catch (error) {
      this.pagina = null;
      this.error = error instanceof Error && error.message.startsWith('No se encontró')
        ? error.message
        : this.apiError.obtenerMensaje(error);
    } finally {
      this.cargando = false;
      this.cdr.markForCheck();
    }
  }

  async exportar(): Promise<void> {
    if (this.exportando || this.cargando) {
      return;
    }

    this.exportando = true;
    this.error = '';
    this.cdr.markForCheck();

    try {
      const productoId = await this.resolverProductoId();
      let hastaExclusivo: string | undefined;

      if (this.hasta) {
        const siguienteDia = new Date(`${this.hasta}T00:00:00Z`);
        siguienteDia.setUTCDate(siguienteDia.getUTCDate() + 1);
        hastaExclusivo = siguienteDia.toISOString();
      }

      await this.exportService.exportarKardex({
        productoId,
        tipo: this.tipo ? Number(this.tipo) : null,
        desde: this.desde ? `${this.desde}T00:00:00Z` : undefined,
        hasta: hastaExclusivo,
      });
    } catch (error) {
      this.error = error instanceof Error && error.message.startsWith('No se encontró')
        ? error.message
        : this.apiError.obtenerMensaje(error);
    } finally {
      this.exportando = false;
      this.cdr.markForCheck();
    }
  }
}
