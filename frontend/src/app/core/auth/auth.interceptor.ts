import { HttpInterceptorFn } from '@angular/common/http';

import { inject } from '@angular/core';

import { Router } from '@angular/router';

import {
  catchError,
  switchMap,
  throwError,
} from 'rxjs';

import { API } from '../config/api.config';

import { AuthService } from './auth.service';

export const authInterceptor: HttpInterceptorFn = (
  req,
  next
) => {

  // Solo interceptar nuestra API.
  if (!req.url.startsWith(`${API}/`)) {
    return next(req);
  }

  const auth = inject(AuthService);

  const router = inject(Router);

  const isAuthRequest =
    req.url.startsWith(`${API}/auth/`);

  const token = auth.token;

  // ======================================================
  // AGREGAR TOKEN
  // ======================================================

  const authenticatedRequest =
    !isAuthRequest && token
      ? req.clone({
          setHeaders: {
            Authorization: `Bearer ${token}`,
          },
        })
      : req;

  // Evitar que login, refresh y logout
  // entren en el mecanismo de renovación.
  if (isAuthRequest) {
    return next(authenticatedRequest);
  }

  // ======================================================
  // EJECUTAR PETICIÓN
  // ======================================================

  return next(authenticatedRequest).pipe(

    catchError(error => {

      if (error.status !== 401) {
        return throwError(() => error);
      }

      // Intentar renovar el access token.
      return auth.refresh().pipe(

        switchMap(session => {

          const retryRequest = req.clone({
            setHeaders: {
              Authorization:
                `Bearer ${session.accessToken}`,
            },
          });

          return next(retryRequest);
        }),

        catchError(refreshError => {

          auth.clear();

          void router.navigateByUrl('/login');

          return throwError(() => refreshError);
        })
      );
    })
  );
};