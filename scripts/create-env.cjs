// Solo para desarrollo local. Nunca incluir el .env generado en Git.
const { randomBytes } = require('node:crypto');
const { writeFileSync } = require('node:fs');
const { resolve } = require('node:path');

const ruta = resolve(__dirname, '..', '.env');
const secretoJwt = randomBytes(48).toString('hex');
const contenido = [
  '# Credenciales LOCALES de ejemplo; cambiar para cualquier despliegue publico',
  'POSTGRES_PASSWORD=1234',
  'APP_DB_PASSWORD=1234',
  'SEED_ADMIN_PASSWORD=Inventario2026!',
  `JWT_SECRET=${secretoJwt}`,
  '',
].join('\n');

try {
  writeFileSync(ruta, contenido, { flag: 'wx', mode: 0o600 });
  console.log('Se creo .env. JWT_SECRET se genero de forma aleatoria.');
  console.log('IMPORTANTE: si ya existe una base de datos, estas contrasenas NO cambian las de PostgreSQL ni las del usuario administrador registrado.');
} catch (error) {
  if (error.code !== 'EEXIST') throw error;
  console.log('El archivo .env ya existe. No se modifico, no se regeneraron credenciales.');
}
