import { DecimalPipe } from '@angular/common';
import {
  ChangeDetectorRef,
  Component,
  ElementRef,
  inject,
  OnInit,
  OnDestroy,
  ViewChild,
} from '@angular/core';
import { FormsModule, NgForm } from '@angular/forms';
import { AuthService } from '../../core/auth/auth.service';
import { Page } from '../../core/models/page.model';
import { ApiErrorService } from '../../core/services/api-error.service';
import { FeedbackComponent } from '../../shared/components/feedback/feedback.component';
import { Category, Product, ProductData } from './producto.model';
import { ProductosService } from './productos.service';

function emptyProduct(): ProductData {
  return {
    sku: '',
    nombre: '',
    descripcion: null,
    categoriaId: 0,
    unidadMedida: 1,
    precio: 0,
    stockMinimo: 0,
  };
}

@Component({
  selector: 'app-productos',
  imports: [DecimalPipe, FormsModule, FeedbackComponent],
  templateUrl: './productos.component.html',
  styleUrl: './productos.component.css',
})
export class ProductosComponent implements OnInit, OnDestroy {
  @ViewChild('editorPanel') private editorPanel?: ElementRef<HTMLElement>;

  private readonly productosService = inject(ProductosService);
  private readonly apiError = inject(ApiErrorService);
  private readonly cdr = inject(ChangeDetectorRef);
  readonly auth = inject(AuthService);

  pagina: Page<Product> | null = null;
  categorias: Category[] = [];
  datos: ProductData = emptyProduct();
  editId: number | null = null;
  editando = false;
  guardando = false;
  cargando = false;
  actualizandoEstadoId: number | null = null;
  eliminandoId: number | null = null;
  busqueda = '';
  numero = 1;
  mensaje = '';
  esError = true;
  private mensajeTimer: ReturnType<typeof setTimeout> | null = null;

  private readonly formatoCOP = new Intl.NumberFormat('es-CO', {
    style: 'currency',
    currency: 'COP',
    minimumFractionDigits: 0,
    maximumFractionDigits: 2,
  });

  formatearCOP(valor: number): string {
    return `${this.formatoCOP.format(valor)} COP`;
  }

  ngOnInit(): void {
    this.cargar();
    this.productosService.listarCategorias().subscribe({
      next: categorias => {
        this.categorias = categorias;
        this.cdr.markForCheck();
      },
      error: error => {
        this.fallo(error);
        this.cdr.markForCheck();
      },
    });
  }

  ngOnDestroy(): void {
    this.detenerTemporizadorMensaje();
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
    this.cargando = true;
    this.cdr.markForCheck();
    this.productosService.listar(this.numero, 20, this.busqueda).subscribe({
      next: resultado => {
        this.pagina = resultado;
        this.cargando = false;
        this.cdr.markForCheck();
      },
      error: error => {
        this.fallo(error);
        this.cargando = false;
        this.cdr.markForCheck();
      },
    });
  }

  nuevo(): void {
    this.limpiarMensaje();
    this.editId = null;
    this.datos = emptyProduct();
    this.editando = true;
    this.desplazarAlFormulario();
  }

  editar(producto: Product): void {
    this.limpiarMensaje();
    this.editId = producto.id;
    this.datos = {
      sku: producto.sku,
      nombre: producto.nombre,
      descripcion: producto.descripcion,
      categoriaId: producto.categoriaId,
      unidadMedida: producto.unidadMedida,
      precio: producto.precio,
      stockMinimo: producto.stockMinimo,
    };
    this.editando = true;
    this.desplazarAlFormulario();
  }

  cancelar(): void {
    this.limpiarMensaje();
    this.editando = false;
    this.editId = null;
    this.cdr.markForCheck();
  }

  private desplazarAlFormulario(): void {
    // Renderiza primero el bloque @if, para que el elemento exista en el DOM.
    this.cdr.detectChanges();
    requestAnimationFrame(() => {
      this.editorPanel?.nativeElement.scrollIntoView({
        behavior: 'smooth',
        block: 'start',
      });
    });
  }

  guardar(form: NgForm): void {
    if (form.invalid || !this.datos.categoriaId || this.datos.precio < 50 || this.guardando) return;

    this.guardando = true;
    this.cdr.markForCheck();
    const request = this.editId !== null
      ? this.productosService.editar(this.editId, this.datos)
      : this.productosService.crear(this.datos);

    request.subscribe({
      next: () => {
        this.guardando = false;
        this.editando = false;
        this.editId = null;
        this.aviso('Producto guardado correctamente.', false);
        this.cdr.markForCheck();
        this.cargar();
      },
      error: error => {
        this.guardando = false;
        this.fallo(error);
        this.cdr.markForCheck();
      },
    });
  }

  estado(producto: Product): void {
    if (this.actualizandoEstadoId !== null) return;

    const accion = producto.activo ? 'Desactivar' : 'Activar';
    if (!confirm(`${accion} ${producto.nombre}?`)) return;

    const nuevoEstado = !producto.activo;
    this.actualizandoEstadoId = producto.id;
    this.cdr.markForCheck();

    this.productosService.cambiarEstado(producto.id, nuevoEstado).subscribe({
      next: () => {
        // Refleja el cambio inmediatamente en la fila, sin abrir el editor.
        producto.activo = nuevoEstado;
        this.actualizandoEstadoId = null;
        this.aviso('Estado actualizado.', false);
        this.cdr.markForCheck();
        // Refresca después la información con el estado definitivo de la API.
        this.cargar();
      },
      error: error => {
        this.actualizandoEstadoId = null;
        this.fallo(error);
        this.cdr.markForCheck();
      },
    });
  }

  eliminar(producto: Product): void {
    if (this.eliminandoId !== null || this.actualizandoEstadoId !== null) return;

    const confirmado = confirm(
      `¿Eliminar definitivamente "${producto.nombre}" (${producto.sku})? ` +
      'Solo se permite si el stock es 0 y no existen movimientos. Esta acción no se puede deshacer.'
    );
    if (!confirmado) return;

    this.eliminandoId = producto.id;
    this.cdr.markForCheck();

    this.productosService.eliminar(producto.id).subscribe({
      next: () => {
        this.eliminandoId = null;
        if (this.editId === producto.id) this.cancelar();
        this.aviso('Producto eliminado correctamente.', false);
        this.cargar();
        this.cdr.markForCheck();
      },
      error: error => {
        this.eliminandoId = null;
        this.fallo(error);
        this.cdr.markForCheck();
      },
    });
  }

  private aviso(message: string, error: boolean): void {
    // Cada nuevo aviso reinicia el plazo de visibilidad.
    this.detenerTemporizadorMensaje();
    this.mensaje = message;
    this.esError = error;
    this.cdr.markForCheck();

    this.mensajeTimer = setTimeout(() => {
      this.mensaje = '';
      this.mensajeTimer = null;
      this.cdr.markForCheck();
    }, 10_000);
  }

  private limpiarMensaje(): void {
    this.detenerTemporizadorMensaje();
    this.mensaje = '';
    this.cdr.markForCheck();
  }

  private detenerTemporizadorMensaje(): void {
    if (this.mensajeTimer !== null) {
      clearTimeout(this.mensajeTimer);
      this.mensajeTimer = null;
    }
  }

  private fallo(error: unknown): void {
    this.aviso(this.apiError.obtenerMensaje(error), true);
  }
}
