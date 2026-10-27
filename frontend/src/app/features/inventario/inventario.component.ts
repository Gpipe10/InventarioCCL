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
  templateUrl: './inventario.component.html',
  styleUrl: './inventario.component.css',
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
