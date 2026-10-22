# InventarioCCL

**Sistema de Gestión de Inventario | Prueba Técnica Full Stack**

Aplicación web desarrollada con **C#, ASP.NET Core 10, Angular y PostgreSQL** para administrar productos, registrar entradas y salidas de mercancía y consultar las existencias actuales.

La solución implementa una API REST, procedimientos almacenados para las operaciones de inventario y una interfaz web responsiva. El backend está organizado mediante **Clean Architecture** para separar la lógica de negocio, el acceso a datos y la presentación.

## 1. Funcionalidades

### Requisitos de la prueba técnica

- **Gestión de inventario:** consulta de productos, cantidades disponibles y estado actual de las existencias.
- **Entradas de mercancía:** registro de productos ingresados, cantidades y costos unitarios.
- **Salidas de mercancía:** registro de retiros con validación de stock disponible.
- **Historial de movimientos:** consulta de entradas y salidas registradas.
- **API REST:** servicios desarrollados con ASP.NET Core para comunicar el frontend con el backend.
- **Base de datos PostgreSQL:** persistencia de productos, usuarios y movimientos.
- **Procedimientos almacenados:** implementación de operaciones de entrada, salida y consultas de inventario.
- **Autenticación y validaciones:** control de acceso mediante JWT y manejo de errores de las operaciones.
- **Interfaz responsiva:** aplicación desarrollada con Angular para su uso desde el navegador.

### Funcionalidades adicionales

Además de los requisitos principales, se incorporaron las siguientes mejoras:

- **Administración de productos:** creación, edición, búsqueda, activación y desactivación.
- **Categorías y unidades de medida:** organización de productos según sus características.
- **Dashboard:** indicadores de productos activos, existencias, valoración del inventario y actividad de movimientos.
- **Alertas de stock bajo:** identificación de productos que alcanzan su nivel mínimo de existencias.
- **Kardex:** historial de movimientos con filtros por producto, tipo y fechas.
- **Costos promedio:** actualización del costo promedio ponderado al registrar entradas.
- **Refresh tokens:** renovación de sesiones de autenticación.
- **Paginación y filtros:** consultas de inventario y movimientos con criterios de búsqueda.
- **Docker Compose:** ejecución de frontend, API y base de datos mediante contenedores.
- **Integración continua:** compilación automática del backend y frontend con GitHub Actions.

## 2. Tecnologías utilizadas

| Componente | Tecnología |
|---|---|
| Backend | C# y ASP.NET Core 10 |
| Frontend | Angular y TypeScript |
| Base de datos | PostgreSQL 17 |
| Acceso a datos | Entity Framework Core |
| Procedimientos almacenados | PL/pgSQL |
| Autenticación | JWT y refresh tokens |
| Arquitectura | Clean Architecture |
| Contenedores | Docker y Docker Compose |
| Servidor web | Nginx |
| Integración continua | GitHub Actions |

## 3. Arquitectura

El backend está organizado en cuatro proyectos siguiendo **Clean Architecture**:

```text
InventarioCCL/
├── backend/
│   └── src/
│       ├── Inventario.Domain/
│       ├── Inventario.Application/
│       ├── Inventario.Infrastructure/
│       └── Inventario.Presentation/
├── frontend/
├── database/
├── scripts/
├── compose.yaml
└── README.md
```

**Responsabilidades de cada capa:**

- **Domain:** entidades y reglas del dominio.
- **Application:** casos de uso, servicios, DTOs, validadores e interfaces.
- **Infrastructure:** implementación de repositorios, persistencia con Entity Framework Core y servicios técnicos.
- **Presentation:** controladores y configuración de la API REST.

El frontend Angular consume los servicios de la API. El backend utiliza PostgreSQL para almacenar la información y ejecutar los procedimientos almacenados asociados con los movimientos de inventario.

Las entradas y salidas actualizan las existencias mediante operaciones transaccionales, con validaciones para evitar cantidades inválidas o salidas sin stock suficiente.

## 4. Ejecución con Docker

La aplicación puede ejecutarse localmente utilizando **Docker Desktop y Docker Compose**.

Para generar el archivo de configuración automáticamente también se requiere **Node.js**.

### Paso 1. Clonar el repositorio

```cmd
git clone https://github.com/Gpipe10/InventarioCCL.git
cd InventarioCCL
```

El repositorio debe estar disponible para la cuenta que realice la clonación.

### Paso 2. Generar las variables de entorno

Ejecutar:

```cmd
node scripts/create-env.cjs
```

El script genera un archivo `.env` con las variables necesarias para PostgreSQL, el usuario inicial y la autenticación JWT.

La configuración generada está destinada únicamente a **desarrollo y evaluación local**. El archivo `.env` no se publica en el repositorio.

### Paso 3. Iniciar PostgreSQL

```cmd
docker compose up -d postgres
```

### Paso 4. Inicializar la base de datos

En una instalación nueva, ejecutar los siguientes comandos desde **CMD de Windows**, en la raíz del proyecto:

```cmd
docker compose exec -T postgres psql -U postgres -d inventario < database\01_schema.sql

docker compose exec -T postgres psql -U postgres -d inventario < database\02_stored_procedures.sql

docker compose exec -T postgres psql -U postgres -d inventario < database\03_seed.sql
```

Los scripts crean la estructura de la base de datos, los procedimientos almacenados y los datos iniciales, incluidos el usuario administrador y productos de ejemplo.

La inicialización debe realizarse **una sola vez por cada base de datos nueva**.

Opcionalmente, se pueden verificar las operaciones de entrada, salida, control de stock y permisos mediante:

```cmd
docker compose exec -T postgres psql -U postgres -d inventario < database\04_verificar.sql
```

Este último script ejecuta verificaciones dentro de una transacción y revierte sus cambios al finalizar.

### Paso 5. Iniciar la aplicación

```cmd
docker compose up -d --build
```

Una vez iniciados los contenedores, los servicios estarán disponibles en:

| Servicio | Dirección |
|---|---|
| Aplicación web | http://localhost:4200 |
| API REST | http://localhost:5000 |
| Verificación de la API | http://localhost:5000/api/health |
| PostgreSQL | localhost:5433 |

Para consultar el estado de los contenedores:

```cmd
docker compose ps
```

Para detener los servicios sin eliminar los datos:

```cmd
docker compose down
```

## 5. Credenciales de demostración

Al ejecutar `03_seed.sql`, se crea el usuario administrador inicial.

| Campo | Valor |
|---|---|
| Correo | `admin@inventario.local` |
| Contraseña | `Inventario2026!` |
| Rol | Administrador |

Estas credenciales corresponden a la configuración generada por `scripts/create-env.cjs` y son exclusivamente para evaluación local.

Si se modifica `SEED_ADMIN_PASSWORD` antes de inicializar la base de datos, debe utilizarse la contraseña configurada en ese momento.

## 6. Consideraciones técnicas

- **PostgreSQL:** los movimientos de inventario se registran mediante procedimientos almacenados que actualizan las existencias y validan las reglas de negocio.
- **Integridad:** se utilizan transacciones, restricciones de base de datos y bloqueos de filas para proteger la consistencia del stock.
- **Seguridad:** la API incorpora autenticación JWT, validaciones y manejo de excepciones. El usuario de aplicación tiene permisos limitados en PostgreSQL.
- **Persistencia:** Docker utiliza un volumen para conservar la información de la base de datos entre reinicios.
- **Integración continua:** GitHub Actions verifica la compilación de los proyectos .NET y Angular.

## Autor

**Daniel Felipe Guzmán Bernal**  
Ingeniero Mecatrónico | Desarrollador Full Stack .NET y Angular

Proyecto desarrollado como prueba técnica para **Corporación Colombiana de Logística (CCL)**.