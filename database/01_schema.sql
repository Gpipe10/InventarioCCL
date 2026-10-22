
\set ON_ERROR_STOP on
\getenv app_password APP_DB_PASSWORD

BEGIN;

CREATE EXTENSION IF NOT EXISTS pgcrypto;
REVOKE CREATE ON SCHEMA public FROM PUBLIC;
CREATE SCHEMA inventario;
CREATE ROLE inventario_app LOGIN PASSWORD :'app_password'
    NOSUPERUSER NOCREATEDB NOCREATEROLE NOREPLICATION;

CREATE TABLE inventario.roles (
    id smallint PRIMARY KEY,
    nombre varchar(20) NOT NULL UNIQUE
);

CREATE TABLE inventario.usuarios (
    id bigint GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    nombre varchar(100) NOT NULL CHECK (btrim(nombre) <> ''),
    email varchar(254) NOT NULL UNIQUE,
    password_hash text NOT NULL,
    rol_id smallint NOT NULL REFERENCES inventario.roles(id),
    activo boolean NOT NULL DEFAULT true,
    intentos_fallidos integer NOT NULL DEFAULT 0 CHECK (intentos_fallidos >= 0),
    bloqueado_hasta timestamptz,
    creado_en timestamptz NOT NULL DEFAULT now(),
    actualizado_en timestamptz NOT NULL DEFAULT now(),
    CHECK (email = lower(btrim(email)) AND email <> '')
);

CREATE TABLE inventario.refresh_tokens (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    usuario_id bigint NOT NULL REFERENCES inventario.usuarios(id),
    token_hash char(64) NOT NULL UNIQUE CHECK (token_hash ~ '^[0-9a-f]{64}$'),
    familia_id uuid NOT NULL,
    creado_en timestamptz NOT NULL DEFAULT now(),
    expira_en timestamptz NOT NULL,
    revocado_en timestamptz,
    reemplazado_por uuid REFERENCES inventario.refresh_tokens(id),
    CHECK (expira_en > creado_en),
    CHECK (revocado_en IS NULL OR revocado_en >= creado_en)
);

CREATE TABLE inventario.categorias (
    id integer GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    nombre varchar(80) NOT NULL CHECK (btrim(nombre) <> ''),
    activo boolean NOT NULL DEFAULT true
);

CREATE UNIQUE INDEX ux_categorias_nombre
    ON inventario.categorias (lower(nombre));

CREATE TABLE inventario.productos (
    id bigint GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    sku varchar(40) NOT NULL UNIQUE,
    nombre varchar(150) NOT NULL CHECK (btrim(nombre) <> ''),
    descripcion varchar(1000),
    categoria_id integer NOT NULL REFERENCES inventario.categorias(id),
    unidad_medida varchar(10) NOT NULL,
    precio numeric(18,2) NOT NULL CHECK (precio >= 0 AND precio < 1e16),
    stock_minimo numeric(18,3) NOT NULL DEFAULT 0
        CHECK (stock_minimo >= 0 AND stock_minimo < 1e15),
    stock numeric(18,3) NOT NULL DEFAULT 0 CHECK (stock >= 0 AND stock < 1e15),
    costo_promedio numeric(18,6) NOT NULL DEFAULT 0
        CHECK (costo_promedio >= 0 AND costo_promedio < 1e12),
    activo boolean NOT NULL DEFAULT true,
    creado_en timestamptz NOT NULL DEFAULT now(),
    actualizado_en timestamptz NOT NULL DEFAULT now(),
    creado_por bigint NOT NULL REFERENCES inventario.usuarios(id),
    actualizado_por bigint REFERENCES inventario.usuarios(id),
    CHECK (sku = upper(btrim(sku)) AND sku <> ''),
    CHECK (unidad_medida IN ('unidad', 'kg', 'litro', 'metro'))
);

CREATE TABLE inventario.movimientos_inventario (
    id bigint GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    producto_id bigint NOT NULL REFERENCES inventario.productos(id),
    usuario_id bigint NOT NULL REFERENCES inventario.usuarios(id),
    tipo smallint NOT NULL CHECK (tipo IN (1, 2)),
    cantidad numeric(18,3) NOT NULL CHECK (cantidad > 0 AND cantidad < 1e15),
    costo_unitario numeric(18,6) NOT NULL
        CHECK (costo_unitario >= 0 AND costo_unitario < 1e12),
    stock_resultante numeric(18,3) NOT NULL
        CHECK (stock_resultante >= 0 AND stock_resultante < 1e15),
    proveedor varchar(150),
    motivo varchar(250),
    referencia varchar(80),
    fecha timestamptz NOT NULL,
    creado_en timestamptz NOT NULL DEFAULT clock_timestamp(),
    CHECK (
        (tipo = 1 AND (nullif(btrim(proveedor), '') IS NOT NULL
                   OR nullif(btrim(motivo), '') IS NOT NULL))
        OR
        (tipo = 2 AND proveedor IS NULL AND nullif(btrim(motivo), '') IS NOT NULL)
    )
);

CREATE INDEX ix_usuarios_rol ON inventario.usuarios (rol_id);
CREATE INDEX ix_refresh_usuario ON inventario.refresh_tokens (usuario_id);
CREATE INDEX ix_refresh_familia ON inventario.refresh_tokens (familia_id);
CREATE INDEX ix_refresh_expira ON inventario.refresh_tokens (expira_en);
CREATE INDEX ix_refresh_reemplazo ON inventario.refresh_tokens (reemplazado_por);
CREATE INDEX ix_productos_categoria ON inventario.productos (categoria_id);
CREATE INDEX ix_productos_nombre ON inventario.productos (nombre, id);
CREATE INDEX ix_productos_creador ON inventario.productos (creado_por);
CREATE INDEX ix_productos_editor ON inventario.productos (actualizado_por);
CREATE INDEX ix_movimientos_producto ON inventario.movimientos_inventario (producto_id, id DESC);
CREATE INDEX ix_movimientos_fecha ON inventario.movimientos_inventario (fecha, id);
CREATE INDEX ix_movimientos_usuario ON inventario.movimientos_inventario (usuario_id);

GRANT CONNECT ON DATABASE inventario TO inventario_app;
GRANT USAGE ON SCHEMA inventario TO inventario_app;
GRANT SELECT ON ALL TABLES IN SCHEMA inventario TO inventario_app;
GRANT USAGE, SELECT ON ALL SEQUENCES IN SCHEMA inventario TO inventario_app;
GRANT INSERT, UPDATE ON inventario.usuarios, inventario.refresh_tokens,
    inventario.categorias TO inventario_app;
GRANT INSERT (sku, nombre, descripcion, categoria_id, unidad_medida, precio,
    stock_minimo, activo, creado_en, actualizado_en, creado_por, actualizado_por)
    ON inventario.productos TO inventario_app;
GRANT UPDATE (sku, nombre, descripcion, categoria_id, unidad_medida, precio,
    stock_minimo, activo, actualizado_en, actualizado_por)
    ON inventario.productos TO inventario_app;

COMMIT;
