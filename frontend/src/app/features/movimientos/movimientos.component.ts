
import {
  ChangeDetectorRef,
  Component,
  inject,
  OnDestroy,
  OnInit,
} from '@angular/core';

import {
  FormsModule,
  NgForm,
} from '@angular/forms';

import { ApiErrorService } from '../../core/services/api-error.service';
import { FeedbackComponent } from '../../shared/components/feedback/feedback.component';

import { Product } from '../productos/producto.model';
import { ProductosService } from '../productos/productos.service';

import {
  EntradaRequest,
  SalidaRequest,
} from './movimiento.model';

import { MovimientosService } from './movimientos.service';

@Component({
  selector: 'app-movimientos',

  imports: [
    FormsModule,
    FeedbackComponent,
  ],

  templateUrl: './movimientos.component.html',
  styleUrl: './movimientos.component.css',
})
export class MovimientosComponent implements OnInit, OnDestroy {

  private readonly productosService =
    inject(ProductosService);

  private readonly movimientosService =
    inject(MovimientosService);

  private readonly apiError =
    inject(ApiErrorService);

  private readonly cdr =
    inject(ChangeDetectorRef);

  private temporizadorMensaje:
    ReturnType<typeof setTimeout> | null = null;

  productos: Product[] = [];

  busqueda = '';

  productoId = 0;

  stockActual = 0;

  tipo: 'entrada' | 'salida' = 'entrada';

  cantidad = 1;

  costo = 0;

  proveedor = '';

  motivo = '';

  referencia = '';

  enviando = false;

  mensaje = '';

  esError = true;

  // ======================================================
  // INICIALIZAR COMPONENTE
  // ======================================================

  ngOnInit(): void {
    this.buscar();
  }

  ngOnDestroy(): void {
    this.limpiarTemporizador();
  }

  // ======================================================
  // CONSULTAR PRODUCTOS ACTIVOS
  // ======================================================

  buscar(): void {

    if (this.enviando) {
      return;
    }

    this.productosService
      .listar(
        1,
        50,
        this.busqueda,
        true
      )
      .subscribe({

        next: resultado => {

          this.productos = resultado.items;

          // Verificar si el producto seleccionado
          // continúa disponible en los resultados.

          const productoSeleccionado =
            this.productos.find(
              producto => producto.id === this.productoId
            );

          if (!productoSeleccionado) {

            this.productoId = 0;

            this.stockActual = 0;

          } else {

            this.stockActual =
              productoSeleccionado.stock;

          }

          this.cdr.markForCheck();
        },

        error: error => {

          this.aviso(
            this.apiError.obtenerMensaje(error),
            true
          );

          this.cdr.markForCheck();
        },
      });
  }

  // ======================================================
  // CONSULTAR STOCK DE REFERENCIA
  // ======================================================

  stockDelProducto(id: number): number {

    return this.productos.find(
      producto => producto.id === id
    )?.stock ?? 0;
  }

  // ======================================================
  // REGISTRAR MOVIMIENTO
  // ======================================================

  registrar(form: NgForm): void {

    if (
      form.invalid ||
      !this.productoId ||
      this.enviando
    ) {
      return;
    }

    this.enviando = true;

    this.limpiarMensaje();

    this.cdr.markForCheck();

    if (this.tipo === 'entrada') {

      const request: EntradaRequest = {
        productoId: this.productoId,
        cantidad: this.cantidad,
        costoUnitario: this.costo,
        proveedor: this.proveedor || null,
        motivo: this.motivo || null,
        referencia: this.referencia || null,
        fecha: null,
      };

      this.movimientosService
        .registrarEntrada(request)
        .subscribe({

          next: resultado => {
            this.finalizarRegistro(resultado.id);
          },

          error: error => {
            this.mostrarError(error);
          },
        });

      return;
    }

    const request: SalidaRequest = {
      productoId: this.productoId,
      cantidad: this.cantidad,
      motivo: this.motivo,
      referencia: this.referencia || null,
      fecha: null,
    };

    this.movimientosService
      .registrarSalida(request)
      .subscribe({

        next: resultado => {
          this.finalizarRegistro(resultado.id);
        },

        error: error => {
          this.mostrarError(error);
        },
      });
  }

  // ======================================================
  // REGISTRO EXITOSO
  // ======================================================

  private finalizarRegistro(id: number): void {

    this.enviando = false;

    this.cantidad = 1;

    this.motivo = '';

    this.referencia = '';

    this.aviso(
      `Movimiento #${id} registrado correctamente.`,
      false
    );

    // Actualizar productos y stock desde PostgreSQL.

    this.buscar();

    // Actualizar el botón y el formulario.

    this.cdr.markForCheck();
  }

  // ======================================================
  // ERROR DE REGISTRO
  // ======================================================

  private mostrarError(error: unknown): void {

    this.enviando = false;

    this.aviso(
      this.apiError.obtenerMensaje(error),
      true
    );

    this.cdr.markForCheck();
  }

  // ======================================================
  // MENSAJES TEMPORALES - 10 SEGUNDOS
  // ======================================================

  private aviso(
    message: string,
    error: boolean
  ): void {

    this.limpiarTemporizador();

    this.mensaje = message;

    this.esError = error;

    this.cdr.markForCheck();

    this.temporizadorMensaje = setTimeout(() => {

      this.mensaje = '';

      this.temporizadorMensaje = null;

      this.cdr.markForCheck();

    }, 10000);
  }

  // ======================================================
  // LIMPIAR MENSAJES
  // ======================================================

  private limpiarMensaje(): void {

    this.limpiarTemporizador();

    this.mensaje = '';

    this.cdr.markForCheck();
  }

  private limpiarTemporizador(): void {

    if (this.temporizadorMensaje !== null) {

      clearTimeout(this.temporizadorMensaje);

      this.temporizadorMensaje = null;
    }
  }
}
