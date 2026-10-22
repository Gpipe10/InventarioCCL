export interface Session {
  usuarioId: number;
  nombre: string;
  rol: number;
  accessToken: string;
  accessExpiraEn: string;
}