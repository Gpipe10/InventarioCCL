import {
  ChangeDetectorRef,
  Component,
  inject
} from '@angular/core';

import { FormsModule, NgForm } from '@angular/forms';
import { Router } from '@angular/router';

import { AuthService } from '../../../core/auth/auth.service';
import { ApiErrorService } from '../../../core/services/api-error.service';
import { FeedbackComponent } from '../../../shared/components/feedback/feedback.component';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [
    FormsModule,
    FeedbackComponent
  ],
  templateUrl: './login.component.html',
  styleUrl: './login.component.css',
})
export class LoginComponent {

  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  private readonly apiError = inject(ApiErrorService);
  private readonly cdr = inject(ChangeDetectorRef);

  email = '';
  password = '';

  mostrarPassword = false;

  enviando = false;
  error = '';

entrar(form: NgForm): void {

  if (form.invalid || this.enviando) {
    return;
  }

  this.error = '';
  this.enviando = true;

  this.auth.login(
    this.email,
    this.password
  ).subscribe({

    next: () => {
      this.enviando = false;
      this.password = '';

      this.cdr.markForCheck();

      void this.router.navigateByUrl('/');
    },

    error: error => {
      this.error = this.apiError.obtenerMensaje(error);
      this.enviando = false;

      this.cdr.markForCheck();
    }
  });
}
}