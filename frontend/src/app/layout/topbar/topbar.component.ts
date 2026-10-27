import { Component, inject, signal } from '@angular/core';
import { Router, RouterLink, RouterLinkActive } from '@angular/router';

import { AuthService } from '../../core/auth/auth.service';

@Component({
  selector: 'app-topbar',
  imports: [RouterLink, RouterLinkActive],
  templateUrl: './topbar.component.html',
  styleUrl: './topbar.component.css'
})
export class TopbarComponent {
  readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  readonly menuAbierto = signal(false);

  alternarMenu(): void {
    this.menuAbierto.update(abierto => !abierto);
  }

  cerrarMenu(): void {
    this.menuAbierto.set(false);
  }

  salir(): void {
    this.cerrarMenu();
    this.auth.logout().subscribe({
      next: () => { void this.router.navigateByUrl('/login'); },
      error: () => { void this.router.navigateByUrl('/login'); }
    });
  }
}
