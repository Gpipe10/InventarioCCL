import { HttpClient } from '@angular/common/http';
import {
  computed,
  inject,
  Injectable,
  signal,
} from '@angular/core';

import {
  catchError,
  finalize,
  map,
  Observable,
  of,
  shareReplay,
  tap,
} from 'rxjs';

import { API } from '../config/api.config';
import { Session } from './session.model';

@Injectable({
  providedIn: 'root',
})
export class AuthService {

  private readonly http = inject(HttpClient);

  private readonly currentSession =
    signal<Session | null>(null);

  private refreshing?: Observable<Session>;

  readonly usuario = this.currentSession.asReadonly();

  readonly isAdmin = computed(() => {
    return this.currentSession()?.rol === 1;
  });

  // ======================================================
  // OBTENER ACCESS TOKEN
  // ======================================================

  get token(): string | null {

    const session = this.currentSession();

    if (!session) {
      return null;
    }

    const expiration = Date.parse(
      session.accessExpiraEn
    );

    if (expiration <= Date.now()) {
      return null;
    }

    return session.accessToken;
  }

  // ======================================================
  // INICIAR SESIÓN
  // ======================================================

  login(
    email: string,
    password: string
  ): Observable<Session> {

    return this.http.post<Session>(
      `${API}/auth/login`,
      {
        email,
        password,
      },
      {
        withCredentials: true,
      }
    ).pipe(
      tap(session => {
        this.currentSession.set(session);
      })
    );
  }

  // ======================================================
  // RENOVAR SESIÓN
  // ======================================================

  refresh(): Observable<Session> {

    if (!this.refreshing) {

      this.refreshing = this.http.post<Session>(
        `${API}/auth/refresh`,
        {},
        {
          withCredentials: true,
        }
      ).pipe(
        tap(session => {
          this.currentSession.set(session);
        }),

        finalize(() => {
          this.refreshing = undefined;
        }),

        shareReplay({
          bufferSize: 1,
          refCount: false,
        })
      );
    }

    return this.refreshing;
  }

  // ======================================================
  // VALIDAR QUE EXISTA SESIÓN
  // ======================================================

  ensureSession(): Observable<boolean> {

    if (this.token) {
      return of(true);
    }

    return this.refresh().pipe(
      map(() => true),

      catchError(() => {
        this.currentSession.set(null);
        return of(false);
      })
    );
  }

  // ======================================================
  // CERRAR SESIÓN
  // ======================================================

  logout(): Observable<void> {

    return this.http.post<void>(
      `${API}/auth/logout`,
      {},
      {
        withCredentials: true,
      }
    ).pipe(
      finalize(() => {
        this.currentSession.set(null);
      })
    );
  }

  // ======================================================
  // LIMPIAR SESIÓN LOCAL
  // ======================================================

  clear(): void {
    this.currentSession.set(null);
  }
}