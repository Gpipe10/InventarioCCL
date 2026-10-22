# InventarioCCL

**Sistema de Gestión de Inventario | Prueba Técnica Full Stack**

Aplicación web desarrollada con **C#, ASP.NET Core 10, Angular y PostgreSQL**, orientada a la administración de productos, el registro de entradas y salidas de mercancía y la consulta del inventario actual.

El proyecto fue realizado como prueba técnica para **Corporación Colombiana de Logística (CCL)**, implementando una API REST, procedimientos almacenados en PostgreSQL y una interfaz web responsiva.

Además de los requisitos solicitados, se incorporaron funcionalidades adicionales y una estructura basada en **Clean Architecture**, con el objetivo de mantener una separación clara entre las responsabilidades de cada componente.

---

## 1. Funcionalidades

### Requisitos de la prueba técnica

- **Consulta de inventario:** visualización de productos, existencias actuales y cantidades disponibles.
- **Entradas de mercancía:** registro de ingresos con cantidades y costos unitarios.
- **Salidas de mercancía:** registro de retiros con validación de stock disponible.
- **Historial de movimientos:** consulta de las operaciones de entrada y salida.
- **API REST:** servicios desarrollados con ASP.NET Core para la comunicación entre el frontend y el backend.
- **Persistencia:** almacenamiento de productos y movimientos en PostgreSQL mediante Entity Framework Core.
- **Procedimientos almacenados:** operaciones de entrada, salida y consulta del inventario implementadas en PL/pgSQL.
- **Autenticación:** control de acceso mediante JWT.
- **Validaciones y manejo de errores:** verificación de datos y tratamiento de excepciones.
- **Interfaz responsiva:** aplicación Angular adaptable a diferentes tamaños de pantalla.

### Funcionalidades adicionales

Como complemento a los requisitos principales, se implementaron:

- Administración de productos: creación, edición, consulta, activación y desactivación.
- Clasificación de productos mediante categorías y unidades de medida.
- Dashboard con indicadores generales y actividad del inventario.
- Identificación de productos con stock bajo.
- Kardex con filtros por producto, fechas y tipo de movimiento.
- Cálculo del costo promedio ponderado en las entradas de mercancía.
- Renovación de sesiones mediante refresh tokens.
- Búsquedas, filtros, ordenamiento y paginación.
- Contenerización de los servicios mediante Docker Compose.
- Integración continua con GitHub Actions.

---

## 2. Tecnologías utilizadas

| Componente | Tecnología |
|---|---|
| Lenguaje backend | C# |
| Framework backend | ASP.NET Core 10 |
| Frontend | Angular / TypeScript |
| Base de datos | PostgreSQL 17 |
| Acceso a datos | Entity Framework Core |
| Procedimientos almacenados | PL/pgSQL |
| Autenticación | JWT y refresh tokens |
| Arquitectura | Clean Architecture |
| Contenedores | Docker / Docker Compose |
| Servidor frontend | Nginx |
| Control de versiones | Git / GitHub |
| Integración continua | GitHub Actions |

---

## 3. Arquitectura del proyecto

El backend fue desarrollado utilizando **Clean Architecture**, con una organización en cuatro proyectos que permite separar la lógica del dominio, los casos de uso, la persistencia y la exposición de servicios HTTP.

Esta estructura facilita el mantenimiento del código, reduce el acoplamiento entre componentes y permite modificar detalles de infraestructura sin concentrar toda la lógica en los controladores.

### 3.1. Organización general

```text
InventarioCCL/
│
├── backend/
│   ├── src/
│   │   ├── Inventario.Domain/
│   │   ├── Inventario.Application/
│   │   ├── Inventario.Infrastructure/
│   │   └── Inventario.Presentation/
│   └── Dockerfile
│
├── frontend/
│   ├── src/
│   ├── Dockerfile
│   └── nginx.conf
│
├── database/
│   ├── 01_schema.sql
│   ├── 02_stored_procedures.sql
│   ├── 03_seed.sql
│   └── 04_verificar.sql
│
├── scripts/
│   └── create-env.cjs
│
├── .github/
│   └── workflows/
│       └── ci.yml
│
├── compose.yaml
├── .env.example
├── Inventario.slnx
└── README.md
```

### 3.2. Inventario.Domain

Representa el núcleo del negocio y contiene los elementos fundamentales del dominio.

Su responsabilidad es definir las entidades y los conceptos del sistema sin depender de tecnologías de presentación o persistencia.

Esta capa permite mantener las definiciones del negocio separadas de ASP.NET Core, Entity Framework Core y PostgreSQL.

### 3.3. Inventario.Application

Contiene los casos de uso y la coordinación de las operaciones del sistema.

Está organizada en componentes como:

- **Services:** implementación de operaciones relacionadas con productos, autenticación, movimientos y consultas.
- **DTOs:** contratos para transportar datos de entrada y salida.
- **Interfaces:** contratos de repositorios, consultas y servicios de seguridad.
- **Validators:** validación de las solicitudes antes de ejecutar operaciones.
- **Common:** componentes compartidos para resultados, políticas y operaciones de seguridad.

Esta capa define qué necesita realizar la aplicación, mientras que las implementaciones de acceso a datos se mantienen en Infrastructure.

### 3.4. Inventario.Infrastructure

Contiene las implementaciones técnicas requeridas por la aplicación.

Entre sus responsabilidades se encuentran:

- Configuración de Entity Framework Core.
- Acceso a PostgreSQL.
- Implementación de repositorios.
- Ejecución de procedimientos almacenados y funciones SQL.
- Persistencia de productos, usuarios y movimientos.
- Implementación de los servicios técnicos de autenticación.

De esta manera, el acceso a datos permanece desacoplado de los casos de uso definidos en Application.

### 3.5. Inventario.Presentation

Es el punto de entrada del backend y expone las funcionalidades mediante una API REST desarrollada con ASP.NET Core.

Se encarga de recibir solicitudes HTTP, utilizar los servicios de aplicación y devolver las respuestas correspondientes.

También concentra la configuración de autenticación, autorización, inyección de dependencias y tratamiento de errores de la API.

### 3.6. Comunicación entre componentes

El flujo general de una operación es:

```text
Usuario
   |
   v
Frontend Angular
   |
   | Solicitud HTTP / JSON
   v
API REST - Presentation
   |
   v
Application
   |
   | Interfaces
   v
Infrastructure
   |
   | EF Core / SQL
   v
PostgreSQL
```

Las dependencias se organizan para mantener el dominio y los casos de uso separados de los detalles tecnológicos.

---

## 4. Persistencia y lógica de inventario

La base de datos utiliza PostgreSQL y está organizada dentro del esquema `inventario`.

### 4.1. Modelo de datos

Las principales tablas son:

| Tabla | Responsabilidad |
|---|---|
| `roles` | Roles de acceso disponibles. |
| `usuarios` | Información de usuarios y autenticación. |
| `refresh_tokens` | Tokens para renovación de sesiones. |
| `categorias` | Clasificación de productos. |
| `productos` | Información de productos y existencias. |
| `movimientos_inventario` | Historial de entradas y salidas. |

Se utilizan relaciones, restricciones e índices para respaldar la integridad de la información.

### 4.2. Procedimientos almacenados

Para las operaciones principales del inventario se implementaron procedimientos almacenados en PL/pgSQL.

**`sp_registrar_entrada`**

- Valida la información del movimiento.
- Comprueba la existencia y el estado del producto.
- Registra el ingreso de mercancía.
- Actualiza el stock.
- Recalcula el costo promedio ponderado.

**`sp_registrar_salida`**

- Valida la cantidad solicitada.
- Comprueba la disponibilidad del producto.
- Impide salidas superiores al stock existente.
- Actualiza las existencias.
- Registra el movimiento realizado.

Ambos procedimientos utilizan bloqueos de filas para evitar inconsistencias cuando se realizan operaciones concurrentes sobre un mismo producto.

### 4.3. Funciones de consulta

Se implementaron funciones para recuperar información del inventario:

| Función | Propósito |
|---|---|
| `fn_inventario_actual` | Consultar existencias con búsqueda, filtros y paginación. |
| `fn_kardex_producto` | Recuperar el historial de movimientos. |
| `fn_resumen_dashboard` | Obtener indicadores generales y actividad del inventario. |

La lógica de actualización de existencias se concentra en PostgreSQL, mientras que los servicios de aplicación coordinan las operaciones y sus validaciones.

---

## 5. Seguridad y validaciones

El sistema incorpora mecanismos de seguridad tanto en la API como en la base de datos.

### Autenticación

Se utiliza autenticación basada en **JWT**, con refresh tokens para la renovación de sesiones.

Las contraseñas de los usuarios se almacenan mediante hashes y las operaciones protegidas requieren autenticación.

### Validación de operaciones

Se implementaron validaciones para controlar situaciones como:

- Cantidades inválidas.
- Productos inexistentes o inactivos.
- Salidas con stock insuficiente.
- Información obligatoria incompleta.
- Solicitudes con parámetros de consulta incorrectos.

### Control de permisos

La API utiliza un usuario específico de PostgreSQL (`inventario_app`) con permisos limitados.

Las operaciones de inventario se realizan mediante los procedimientos correspondientes, evitando que el usuario de aplicación pueda modificar directamente las existencias.

Las credenciales y el secreto JWT se configuran mediante variables de entorno.

---

## 6. Frontend Angular

La interfaz fue desarrollada con **Angular y TypeScript**, consumiendo los servicios de la API REST.

La aplicación organiza las funcionalidades en módulos o áreas de trabajo relacionadas con autenticación, productos, inventario, movimientos y dashboard.

Entre los aspectos implementados se encuentran:

- Formularios con validaciones.
- Comunicación con servicios HTTP.
- Manejo de errores y mensajes al usuario.
- Búsqueda, filtros y paginación.
- Navegación entre las secciones.
- Diseño responsivo.
- Protección de rutas y gestión de sesiones.

El frontend se compila para producción y se sirve mediante **Nginx** dentro de un contenedor Docker.

---

## 7. Ejecución con Docker

La solución está preparada para ejecutarse mediante Docker Compose, utilizando tres servicios:

| Servicio | Descripción |
|---|---|
| `postgres` | Base de datos PostgreSQL. |
| `api` | Backend ASP.NET Core. |
| `frontend` | Aplicación Angular servida con Nginx. |

### Requisitos

- Git.
- Docker Desktop con Docker Compose.
- Node.js para generar automáticamente el archivo `.env`.

Los siguientes comandos están preparados para ejecutarse desde **CMD de Windows**.

### 7.1. Clonar el repositorio

```cmd
git clone https://github.com/Gpipe10/InventarioCCL.git
cd InventarioCCL
```

### 7.2. Generar las variables de entorno

```cmd
node scripts/create-env.cjs
```

Este comando crea el archivo `.env` con las variables necesarias para PostgreSQL y JWT.

El archivo no se sobrescribe si ya existe.

La configuración generada está destinada exclusivamente a desarrollo y evaluación local.

### 7.3. Iniciar PostgreSQL

```cmd
docker compose up -d postgres
```

### 7.4. Inicializar la base de datos

En una instalación nueva, ejecutar los scripts SQL en el siguiente orden:

```cmd
docker compose exec -T postgres psql -U postgres -d inventario < database\01_schema.sql
docker compose exec -T postgres psql -U postgres -d inventario < database\02_stored_procedures.sql
docker compose exec -T postgres psql -U postgres -d inventario < database\03_seed.sql
```

Estos scripts crean la estructura de la base de datos, los procedimientos almacenados, las categorías, los productos iniciales y el usuario administrador.

La inicialización debe realizarse una sola vez por base de datos nueva.

Para ejecutar las verificaciones de las operaciones de inventario:

```cmd
docker compose exec -T postgres psql -U postgres -d inventario < database\04_verificar.sql
```

El script comprueba el registro de entradas y salidas, la validación de stock insuficiente y las restricciones de permisos. Las operaciones de prueba se revierten al finalizar.

### 7.5. Construir e iniciar la aplicación

```cmd
docker compose up -d --build
```

Una vez iniciados los servicios:

| Componente | Dirección |
|---|---|
| Aplicación Angular | http://localhost:4200 |
| API REST | http://localhost:5000 |
| Verificación de API | http://localhost:5000/api/health |
| PostgreSQL | localhost:5433 |

Para verificar los contenedores:

```cmd
docker compose ps
```

Para detener los servicios sin eliminar los datos:

```cmd
docker compose down
```

PostgreSQL utiliza un volumen Docker para conservar la información entre reinicios.

---

## 8. Acceso inicial

El script `03_seed.sql` crea un usuario administrador para las pruebas locales.

| Campo | Valor |
|---|---|
| Correo | `admin@inventario.local` |
| Contraseña | `Inventario2026!` |
| Rol | Administrador |

La contraseña indicada corresponde al valor predeterminado generado por `scripts/create-env.cjs`.

Estas credenciales son exclusivamente de demostración y deben reemplazarse para cualquier despliegue público.

---

## 9. Integración continua

El proyecto incluye un flujo de integración continua en:

```text
.github/workflows/ci.yml
```

GitHub Actions realiza verificaciones independientes de compilación:

- **Backend:** restauración de dependencias y compilación de la solución .NET.
- **Frontend:** instalación de dependencias y compilación de Angular.

Este proceso permite detectar errores de compilación al integrar cambios en el repositorio.

---

## 10. Consideraciones del desarrollo

Durante la implementación se priorizaron los siguientes aspectos:

- **Separación de responsabilidades:** organización del backend mediante Clean Architecture.
- **Integridad del inventario:** validaciones y operaciones transaccionales para entradas y salidas.
- **Trazabilidad:** registro de movimientos con información del producto, usuario y existencias resultantes.
- **Seguridad:** autenticación JWT, validaciones y permisos restringidos en PostgreSQL.
- **Experiencia de usuario:** interfaz responsiva, formularios y mensajes de error.
- **Reproducibilidad:** configuración de los servicios mediante Docker Compose.
- **Mantenimiento:** interfaces, servicios y repositorios organizados por responsabilidades.

---

## Autor

**Daniel Felipe Guzmán Bernal**  
Ingeniero Mecatrónico | Desarrollador Full Stack .NET y Angular

**Proyecto:** Prueba técnica de desarrollo Full Stack – Corporación Colombiana de Logística (CCL).