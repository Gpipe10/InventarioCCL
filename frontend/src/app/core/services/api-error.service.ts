import { HttpErrorResponse } from '@angular/common/http';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class ApiErrorService {

  obtenerMensaje(error: unknown): string {

    if (error instanceof HttpErrorResponse) {

      const body = error.error;

      if (body?.errors && typeof body.errors === 'object') {

        const messages = Object.values(body.errors)
          .flat()
          .filter((x): x is string => typeof x === 'string');

        if (messages.length > 0) {
          return messages.join(' · ');
        }
      }

      if (typeof body?.title === 'string') {
        return body.title;
      }

      if (error.status === 0) {
        return 'No se pudo conectar con la API. Verifica que esté ejecutándose.';
      }

      if (error.status === 401) {
        return 'La sesión no es válida o ha expirado.';
      }

      if (error.status === 403) {
        return 'No tienes permisos para realizar esta operación.';
      }

      return `La solicitud falló (HTTP ${error.status}).`;
    }

    return 'No se pudo completar la operación.';
  }
}