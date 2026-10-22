import {
  HttpClient,
  HttpParams,
} from '@angular/common/http';

import {
  inject,
  Injectable,
} from '@angular/core';

import { Observable } from 'rxjs';

import { API } from '../../core/config/api.config';

import { Page } from '../../core/models/page.model';

import {
  Category,
  Product,
  ProductData,
} from './producto.model';

@Injectable({
  providedIn: 'root',
})
export class ProductosService {

  private readonly http = inject(HttpClient);

  // ======================================================
  // LISTAR PRODUCTOS
  // ======================================================

  listar(
    pagina = 1,
    tamano = 20,
    busqueda = '',
    activo?: boolean
  ): Observable<Page<Product>> {

    let params = new HttpParams()
      .set('pagina', pagina)
      .set('tamano', tamano);

    if (busqueda.trim()) {
      params = params.set(
        'busqueda',
        busqueda.trim()
      );
    }

    if (activo !== undefined) {
      params = params.set(
        'activo',
        activo
      );
    }

    return this.http.get<Page<Product>>(
      `${API}/productos`,
      { params }
    );
  }

  // ======================================================
  // CONSULTAR PRODUCTO
  // ======================================================

  obtener(id: number): Observable<Product> {

    return this.http.get<Product>(
      `${API}/productos/${id}`
    );
  }

  // ======================================================
  // CONSULTAR CATEGORÍAS
  // ======================================================

  listarCategorias(): Observable<Category[]> {

    return this.http.get<Category[]>(
      `${API}/productos/categorias`
    );
  }

  // ======================================================
  // CREAR PRODUCTO
  // ======================================================

  crear(
    producto: ProductData
  ): Observable<Product> {

    return this.http.post<Product>(
      `${API}/productos`,
      producto
    );
  }

  // ======================================================
  // EDITAR PRODUCTO
  // ======================================================

  editar(
    id: number,
    producto: ProductData
  ): Observable<Product> {

    return this.http.put<Product>(
      `${API}/productos/${id}`,
      producto
    );
  }

  // ======================================================
  // ACTIVAR O DESACTIVAR PRODUCTO
  // ======================================================

  cambiarEstado(
    id: number,
    activo: boolean
  ): Observable<Product> {

    return this.http.patch<Product>(
      `${API}/productos/${id}/estado`,
      { activo }
    );
  }

  // ELIMINAR DEFINITIVAMENTE UN PRODUCTO SIN MOVIMIENTOS
  eliminar(id: number): Observable<void> {
    return this.http.delete<void>(`${API}/productos/${id}`);
  }

}