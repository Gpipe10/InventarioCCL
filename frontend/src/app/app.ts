import {
  Component,
  inject,
} from '@angular/core';

import {
  Router,
  RouterLink,
  RouterLinkActive,
  RouterOutlet,
} from '@angular/router';

import { AuthService } from './core/auth/auth.service';

@Component({
  selector: 'app-root',

  imports: [
    RouterOutlet,
    RouterLink,
    RouterLinkActive,
  ],

  templateUrl: './app.html',
})
export class App {

  readonly auth = inject(AuthService);

  private readonly router = inject(Router);

  // ======================================================
  // CERRAR SESIÓN
  // ======================================================

  salir(): void {

    this.auth.logout().subscribe({

      next: () => {
        void this.router.navigateByUrl('/login');
      },

      error: () => {
        void this.router.navigateByUrl('/login');
      },
    });
  }
}