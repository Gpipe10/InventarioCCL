import { Routes } from '@angular/router';

import {
  authGuard,
} from './core/auth/auth.guard';

export const routes: Routes = [

  // ======================================================
  // LOGIN
  // ======================================================

  {
    path: 'login',

    loadComponent: () =>
      import(
        './features/auth/login/login.component'
      ).then(m => m.LoginComponent),
  },

  // ======================================================
  // DASHBOARD
  // ======================================================

  {
    path: '',

    canActivate: [authGuard],

    loadComponent: () =>
      import(
        './features/dashboard/dashboard.component'
      ).then(m => m.DashboardComponent),
  },

  // ======================================================
  // PRODUCTOS
  // ======================================================

  {
    path: 'productos',

    canActivate: [authGuard],

    loadComponent: () =>
      import(
        './features/productos/productos.component'
      ).then(m => m.ProductosComponent),
  },

  // ======================================================
  // MOVIMIENTOS
  // ======================================================

  {
    path: 'movimientos',

    canActivate: [authGuard],

    loadComponent: () =>
      import(
        './features/movimientos/movimientos.component'
      ).then(m => m.MovimientosComponent),
  },

  // ======================================================
  // INVENTARIO
  // ======================================================

  {
    path: 'inventario',

    canActivate: [authGuard],

    loadComponent: () =>
      import(
        './features/inventario/inventario.component'
      ).then(m => m.InventarioComponent),
  },

  // ======================================================
  // KARDEX
  // ======================================================

{
  path: 'kardex',
  canActivate: [authGuard],
  loadComponent: () =>
    import('./features/kardex/kardex.component')
      .then(m => m.KardexComponent),
},

  // ======================================================
  // RUTAS DESCONOCIDAS
  // ======================================================

  {
    path: '**',
    redirectTo: '',
  },
];