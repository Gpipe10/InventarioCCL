import { Component, inject } from '@angular/core';
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
  template: `
    <div class="login-page">

      <div class="login-container">

        <!-- PANEL INSTITUCIONAL -->
        <section class="login-brand">

          <div class="brand-header">

            <div class="brand-logo">
              <span>CCL</span>
            </div>

            <div class="brand-information">
              <span class="brand-name">
                Corporación Colombiana
                de Logística
              </span>

              <span class="brand-description">
                Sistema de gestión empresarial
              </span>
            </div>

          </div>

          <div class="brand-content">

            <div class="brand-badge">
              SISTEMA DE INVENTARIO
            </div>

            <h1>
              Gestión inteligente
              de inventarios.
            </h1>

            <p>
              Administra productos, controla existencias
              y registra movimientos de inventario
              desde un solo lugar.
            </p>

            <div class="brand-features">

              <div class="feature">
                <span class="feature-icon">✓</span>
                <span>Control de productos</span>
              </div>

              <div class="feature">
                <span class="feature-icon">✓</span>
                <span>Entradas y salidas</span>
              </div>

              <div class="feature">
                <span class="feature-icon">✓</span>
                <span>Inventario en tiempo real</span>
              </div>

              <div class="feature">
                <span class="feature-icon">✓</span>
                <span>Seguimiento de movimientos</span>
              </div>

            </div>

          </div>

          <div class="brand-footer">
            Corporación Colombiana de Logística
          </div>

        </section>

        <!-- FORMULARIO DE AUTENTICACIÓN -->
        <section class="login-form-panel">

          <div class="form-content">

            <div class="form-icon">
              <svg
                viewBox="0 0 24 24"
                width="27"
                height="27"
                fill="none"
                stroke="currentColor"
                stroke-width="1.8"
                aria-hidden="true">

                <rect
                  x="5"
                  y="10"
                  width="14"
                  height="11"
                  rx="2" />

                <path
                  d="M8 10V7a4 4 0 0 1 8 0v3" />

                <path d="M12 14v3" />

              </svg>
            </div>

            <div class="form-heading">

              <span class="form-eyebrow">
                BIENVENIDO
              </span>

              <h2>Iniciar sesión</h2>

              <p>
                Ingresa tus credenciales para acceder
                al sistema de inventario.
              </p>

            </div>

            <form
              #form="ngForm"
              (ngSubmit)="entrar(form)"
              novalidate>

              <!-- CORREO -->
              <div class="form-group">

                <label for="email">
                  Correo electrónico
                </label>

                <input
                  id="email"
                  name="email"
                  type="email"
                  placeholder="usuario@empresa.com"
                  autocomplete="username"
                  [(ngModel)]="email"
                  required
                  email />

              </div>

              <!-- CONTRASEÑA -->
              <div class="form-group">

                <label for="password">
                  Contraseña
                </label>

                <div class="password-wrapper">

                  <input
                    id="password"
                    name="password"
                    [type]="mostrarPassword ? 'text' : 'password'"
                    placeholder="Ingresa tu contraseña"
                    autocomplete="current-password"
                    [(ngModel)]="password"
                    required />

                  <button
                    type="button"
                    class="toggle-password"
                    (click)="mostrarPassword = !mostrarPassword"
                    [attr.aria-label]="
                      mostrarPassword
                        ? 'Ocultar contraseña'
                        : 'Mostrar contraseña'
                    ">

                    {{ mostrarPassword ? 'Ocultar' : 'Mostrar' }}

                  </button>

                </div>

              </div>

              <!-- MENSAJES -->
              <app-feedback
                [message]="error" />

              <!-- BOTÓN -->
              <button
                type="submit"
                class="login-button"
                [disabled]="form.invalid || enviando">

                @if (enviando) {

                  <span class="spinner"></span>
                  Ingresando...

                } @else {

                  Ingresar al sistema

                  <span class="button-arrow">→</span>

                }

              </button>

            </form>

            <div class="form-security">

              <span class="security-icon">●</span>

              Acceso exclusivo para usuarios autorizados

            </div>

          </div>

          <div class="form-footer">

            Inventario CCL

            <span>·</span>

            Sistema de gestión de inventarios

          </div>

        </section>

      </div>

    </div>
  `,
  styles: [`

    :host {
      display: block;
    }

    * {
      box-sizing: border-box;
    }

    .login-page {
      min-height: calc(100vh - 145px);
      display: flex;
      align-items: center;
      justify-content: center;
      padding: 40px 20px;
    }

    .login-container {
      width: 100%;
      max-width: 1050px;
      min-height: 570px;
      display: grid;
      grid-template-columns: 46% 54%;
      background: #ffffff;
      border: 1px solid #dce4ee;
      border-radius: 20px;
      overflow: hidden;
      box-shadow: 0 20px 55px rgba(20, 29, 56, .10);
    }

    /* ========================================
       PANEL INSTITUCIONAL
    ======================================== */

    .login-brand {
      position: relative;
      display: flex;
      flex-direction: column;
      justify-content: space-between;
      padding: 35px;
      background:
        radial-gradient(
          circle at 90% 90%,
          rgba(96, 198, 89, .16),
          transparent 35%
        ),
        linear-gradient(
          145deg,
          #00449e 0%,
          #073574 55%,
          #141d38 100%
        );
      color: white;
    }

    .brand-header {
      display: flex;
      align-items: center;
      gap: 15px;
    }

    .brand-logo {
      display: flex;
      align-items: center;
      justify-content: center;
      width: 78px;
      min-width: 78px;
      height: 70px;
      background: #ffffff;
      color: #00449e;
      border-radius: 13px;
      font-size: 26px;
      font-weight: 900;
      font-style: italic;
      letter-spacing: -2px;
    }

    .brand-information {
      display: flex;
      flex-direction: column;
      gap: 5px;
    }

    .brand-name {
      max-width: 225px;
      font-weight: 750;
      font-size: 15px;
      line-height: 1.4;
    }

    .brand-description {
      color: rgba(255, 255, 255, .7);
      font-size: 11px;
      letter-spacing: .3px;
    }

    .brand-content {
      margin: 55px 0;
    }

    .brand-badge {
      display: inline-flex;
      padding: 9px 13px;
      border-radius: 6px;
      background: rgba(255, 255, 255, .12);
      color: #ffffff;
      font-size: 10px;
      font-weight: 750;
      letter-spacing: 1.5px;
    }

    .brand-content h1 {
      max-width: 380px;
      margin: 25px 0 16px;
      color: #ffffff;
      font-size: clamp(29px, 3vw, 40px);
      line-height: 1.18;
      font-weight: 800;
      letter-spacing: -1px;
    }

    .brand-content p {
      max-width: 355px;
      color: rgba(255, 255, 255, .82);
      font-size: 14px;
      line-height: 1.8;
    }

    .brand-features {
      display: grid;
      gap: 15px;
      margin-top: 35px;
    }

    .feature {
      display: flex;
      align-items: center;
      gap: 12px;
      color: #ffffff;
      font-size: 13px;
    }

    .feature-icon {
      display: flex;
      align-items: center;
      justify-content: center;
      width: 23px;
      height: 23px;
      border-radius: 50%;
      background: #60c659;
      color: #141d38;
      font-size: 13px;
      font-weight: 800;
    }

    .brand-footer {
      color: rgba(255, 255, 255, .6);
      font-size: 11px;
    }

    /* ========================================
       FORMULARIO
    ======================================== */

    .login-form-panel {
      display: flex;
      flex-direction: column;
      justify-content: center;
      padding: 60px 52px 25px;
    }

    .form-content {
      width: 100%;
      max-width: 380px;
      margin: auto;
    }

    .form-icon {
      width: 58px;
      height: 58px;
      display: flex;
      align-items: center;
      justify-content: center;
      margin-bottom: 25px;
      border-radius: 14px;
      background: #edf4ff;
      color: #00449e;
    }

    .form-eyebrow {
      display: block;
      margin-bottom: 10px;
      color: #00449e;
      font-size: 11px;
      font-weight: 800;
      letter-spacing: 1.8px;
    }

    .form-heading h2 {
      margin: 0 0 12px;
      color: #141d38;
      font-size: 30px;
      font-weight: 800;
      letter-spacing: -1px;
    }

    .form-heading p {
      margin: 0 0 35px;
      color: #69768c;
      font-size: 14px;
      line-height: 1.7;
    }

    .form-group {
      margin-bottom: 22px;
    }

    .form-group label {
      display: block;
      margin-bottom: 9px;
      color: #26354d;
      font-size: 13px;
      font-weight: 650;
    }

    .form-group input {
      width: 100%;
      height: 47px;
      padding: 0 14px;
      background: #ffffff;
      border: 1px solid #cbd5e1;
      border-radius: 9px;
      color: #141d38;
      font-size: 14px;
      transition: .2s;
    }

    .form-group input:focus {
      outline: none;
      border-color: #00449e;
      box-shadow: 0 0 0 3px rgba(0, 68, 158, .10);
    }

    .form-group input::placeholder {
      color: #9aa6b7;
    }

    .password-wrapper {
      position: relative;
    }

    .password-wrapper input {
      padding-right: 88px;
    }

    .toggle-password {
      position: absolute;
      top: 50%;
      right: 10px;
      transform: translateY(-50%);
      padding: 7px 5px;
      border: 0;
      background: transparent;
      color: #00449e;
      font-size: 12px;
      font-weight: 650;
      cursor: pointer;
    }

    .toggle-password:hover {
      background: transparent;
      color: #003676;
      transform: translateY(-50%);
    }

    /* ========================================
       BOTÓN LOGIN
    ======================================== */

    .login-button {
      width: 100%;
      min-height: 49px;
      display: flex;
      justify-content: center;
      align-items: center;
      gap: 12px;
      margin-top: 25px;
      padding: 12px 16px;
      border: none;
      border-radius: 9px;
      background: #00449e;
      color: #ffffff;
      font-size: 14px;
      font-weight: 750;
      cursor: pointer;
      transition: background .2s, transform .2s;
    }

    .login-button:hover:not(:disabled) {
      background: #003676;
      transform: translateY(-1px);
    }

    .login-button:disabled {
      background: #e2e8f0;
      color: #475569;
      opacity: 1;
      cursor: not-allowed;
    }

    .button-arrow {
      font-size: 19px;
    }

    .spinner {
      width: 17px;
      height: 17px;
      border: 2px solid rgba(255, 255, 255, .3);
      border-top-color: #ffffff;
      border-radius: 50%;
      animation: spin .8s linear infinite;
    }

    @keyframes spin {
      to {
        transform: rotate(360deg);
      }
    }

    .form-security {
      display: flex;
      align-items: center;
      justify-content: center;
      gap: 8px;
      margin-top: 25px;
      color: #718096;
      font-size: 11px;
      text-align: center;
    }

    .security-icon {
      color: #60c659;
      font-size: 10px;
    }

    .form-footer {
      margin-top: 35px;
      text-align: center;
      color: #9aa6b7;
      font-size: 11px;
    }

    .form-footer span {
      margin: 0 6px;
    }

    /* ========================================
       RESPONSIVE
    ======================================== */

    @media (max-width: 850px) {

      .login-container {
        max-width: 500px;
        grid-template-columns: 1fr;
      }

      .login-brand {
        padding: 25px;
      }

      .brand-content,
      .brand-features,
      .brand-footer {
        display: none;
      }

      .login-form-panel {
        padding: 35px 30px 25px;
      }

      .form-content {
        max-width: 100%;
      }

      .form-heading h2 {
        font-size: 26px;
      }
    }

    @media (max-width: 480px) {

      .login-page {
        padding: 20px 12px;
      }

      .login-brand {
        padding: 20px;
      }

      .brand-logo {
        width: 60px;
        min-width: 60px;
        height: 60px;
        font-size: 21px;
      }

      .brand-name {
        font-size: 13px;
      }

      .login-form-panel {
        padding: 30px 22px 20px;
      }
    }

  `],
})
export class LoginComponent {

  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  private readonly apiError = inject(ApiErrorService);

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

        void this.router.navigateByUrl('/');
      },

      error: error => {
        this.error = this.apiError.obtenerMensaje(error);
        this.enviando = false;
      },
    });
  }
}