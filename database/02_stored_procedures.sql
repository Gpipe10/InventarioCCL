
\set ON_ERROR_STOP on

BEGIN;

CREATE OR REPLACE PROCEDURE inventario.sp_registrar_entrada(
    p_producto_id bigint,
    p_cantidad numeric,
    p_costo_unitario numeric,
    p_usuario_id bigint,
    p_proveedor text,
    p_motivo text,
    p_referencia text,
    p_fecha timestamptz,
    INOUT p_movimiento_id bigint DEFAULT NULL
)
LANGUAGE plpgsql
SECURITY DEFINER
SET search_path = pg_catalog, inventario, pg_temp
AS $$
DECLARE
    v_producto inventario.productos%ROWTYPE;
    v_stock numeric;
    v_costo numeric;
BEGIN
    IF p_cantidad IS NULL OR NOT (p_cantidad > 0 AND p_cantidad < 1e15)
        OR p_cantidad <> round(p_cantidad, 3)
        OR p_costo_unitario IS NULL
        OR NOT (p_costo_unitario >= 0 AND p_costo_unitario < 1e12)
        OR p_costo_unitario <> round(p_costo_unitario, 6)
        OR (nullif(btrim(p_proveedor), '') IS NULL
            AND nullif(btrim(p_motivo), '') IS NULL)
        OR p_fecha > statement_timestamp() THEN
        RAISE EXCEPTION USING ERRCODE = 'P2004', MESSAGE = 'Datos de entrada inválidos';
    END IF;

    IF NOT EXISTS (SELECT 1 FROM inventario.usuarios WHERE id = p_usuario_id AND activo) THEN
        RAISE EXCEPTION USING ERRCODE = 'P2005', MESSAGE = 'Usuario no disponible';
    END IF;

    SELECT * INTO v_producto
    FROM inventario.productos WHERE id = p_producto_id FOR UPDATE;

    IF NOT FOUND THEN
        RAISE EXCEPTION USING ERRCODE = 'P2002', MESSAGE = 'Producto no encontrado';
    END IF;
    IF NOT v_producto.activo THEN
        RAISE EXCEPTION USING ERRCODE = 'P2003', MESSAGE = 'El producto está inactivo';
    END IF;

    v_stock := v_producto.stock + p_cantidad;
    v_costo := round((v_producto.stock * v_producto.costo_promedio
        + p_cantidad * p_costo_unitario) / v_stock, 6);

    UPDATE inventario.productos
    SET stock = v_stock, costo_promedio = v_costo,
        actualizado_en = clock_timestamp(), actualizado_por = p_usuario_id
    WHERE id = p_producto_id;

    INSERT INTO inventario.movimientos_inventario
        (producto_id, usuario_id, tipo, cantidad, costo_unitario, stock_resultante,
         proveedor, motivo, referencia, fecha)
    VALUES
        (p_producto_id, p_usuario_id, 1, p_cantidad, p_costo_unitario, v_stock,
         nullif(btrim(p_proveedor), ''), nullif(btrim(p_motivo), ''),
         nullif(btrim(p_referencia), ''), coalesce(p_fecha, statement_timestamp()))
    RETURNING id INTO p_movimiento_id;
END;
$$;

CREATE OR REPLACE PROCEDURE inventario.sp_registrar_salida(
    p_producto_id bigint,
    p_cantidad numeric,
    p_usuario_id bigint,
    p_motivo text,
    p_referencia text,
    p_fecha timestamptz,
    INOUT p_movimiento_id bigint DEFAULT NULL
)
LANGUAGE plpgsql
SECURITY DEFINER
SET search_path = pg_catalog, inventario, pg_temp
AS $$
DECLARE
    v_producto inventario.productos%ROWTYPE;
    v_stock numeric;
BEGIN
    IF p_cantidad IS NULL OR NOT (p_cantidad > 0 AND p_cantidad < 1e15)
        OR p_cantidad <> round(p_cantidad, 3)
        OR nullif(btrim(p_motivo), '') IS NULL
        OR p_fecha > statement_timestamp() THEN
        RAISE EXCEPTION USING ERRCODE = 'P2004', MESSAGE = 'Datos de salida inválidos';
    END IF;

    IF NOT EXISTS (SELECT 1 FROM inventario.usuarios WHERE id = p_usuario_id AND activo) THEN
        RAISE EXCEPTION USING ERRCODE = 'P2005', MESSAGE = 'Usuario no disponible';
    END IF;

    -- El bloqueo debe ocurrir antes de leer y validar el stock disponible.
    SELECT * INTO v_producto
    FROM inventario.productos WHERE id = p_producto_id FOR UPDATE;

    IF NOT FOUND THEN
        RAISE EXCEPTION USING ERRCODE = 'P2002', MESSAGE = 'Producto no encontrado';
    END IF;
    IF NOT v_producto.activo THEN
        RAISE EXCEPTION USING ERRCODE = 'P2003', MESSAGE = 'El producto está inactivo';
    END IF;
    IF v_producto.stock < p_cantidad THEN
        RAISE EXCEPTION USING ERRCODE = 'P2001', MESSAGE = 'No hay stock suficiente';
    END IF;

    v_stock := v_producto.stock - p_cantidad;

    UPDATE inventario.productos
    SET stock = v_stock, actualizado_en = clock_timestamp(), actualizado_por = p_usuario_id
    WHERE id = p_producto_id;

    INSERT INTO inventario.movimientos_inventario
        (producto_id, usuario_id, tipo, cantidad, costo_unitario,
         stock_resultante, motivo, referencia, fecha)
    VALUES
        (p_producto_id, p_usuario_id, 2, p_cantidad, v_producto.costo_promedio,
         v_stock, btrim(p_motivo), nullif(btrim(p_referencia), ''),
         coalesce(p_fecha, statement_timestamp()))
    RETURNING id INTO p_movimiento_id;
END;
$$;

CREATE OR REPLACE FUNCTION inventario.fn_inventario_actual(
    p_busqueda text DEFAULT NULL,
    p_categoria_id integer DEFAULT NULL,
    p_activo boolean DEFAULT NULL,
    p_solo_bajo_stock boolean DEFAULT false,
    p_orden text DEFAULT 'nombre',
    p_descendente boolean DEFAULT false,
    p_pagina integer DEFAULT 1,
    p_tamano integer DEFAULT 20
)
RETURNS TABLE (
    producto_id bigint, sku varchar, nombre varchar, categoria varchar,
    unidad_medida varchar, precio numeric, stock numeric, stock_minimo numeric,
    costo_promedio numeric, valor_inventario numeric, bajo_stock boolean, activo boolean
)
LANGUAGE plpgsql STABLE
SET search_path = pg_catalog, inventario, pg_temp
AS $$
BEGIN
    IF p_pagina IS NULL OR p_pagina < 1
        OR (p_tamano IS NOT NULL AND p_tamano NOT BETWEEN 1 AND 100)
        OR p_orden IS NULL OR p_orden NOT IN ('nombre', 'sku', 'stock', 'valor')
        OR p_descendente IS NULL OR p_solo_bajo_stock IS NULL THEN
        RAISE EXCEPTION USING ERRCODE = 'P2004', MESSAGE = 'Filtros inválidos';
    END IF;

    RETURN QUERY
    SELECT p.id, p.sku, p.nombre, c.nombre, p.unidad_medida, p.precio,
        p.stock, p.stock_minimo, p.costo_promedio,
        round(p.stock * p.costo_promedio, 2), p.activo AND p.stock <= p.stock_minimo, p.activo
    FROM inventario.productos p
    JOIN inventario.categorias c ON c.id = p.categoria_id
    WHERE (nullif(btrim(p_busqueda), '') IS NULL
           OR strpos(lower(p.nombre), lower(btrim(p_busqueda))) > 0
           OR strpos(lower(p.sku), lower(btrim(p_busqueda))) > 0)
      AND (p_categoria_id IS NULL OR p.categoria_id = p_categoria_id)
      AND (p_activo IS NULL OR p.activo = p_activo)
      AND (NOT p_solo_bajo_stock OR (p.activo AND p.stock <= p.stock_minimo))
    ORDER BY
        CASE WHEN NOT p_descendente AND p_orden = 'nombre' THEN p.nombre END ASC,
        CASE WHEN p_descendente AND p_orden = 'nombre' THEN p.nombre END DESC,
        CASE WHEN NOT p_descendente AND p_orden = 'sku' THEN p.sku END ASC,
        CASE WHEN p_descendente AND p_orden = 'sku' THEN p.sku END DESC,
        CASE WHEN NOT p_descendente AND p_orden = 'stock' THEN p.stock END ASC,
        CASE WHEN p_descendente AND p_orden = 'stock' THEN p.stock END DESC,
        CASE WHEN NOT p_descendente AND p_orden = 'valor' THEN p.stock * p.costo_promedio END ASC,
        CASE WHEN p_descendente AND p_orden = 'valor' THEN p.stock * p.costo_promedio END DESC,
        p.id ASC
    LIMIT p_tamano OFFSET (p_pagina::bigint - 1) * coalesce(p_tamano, 0);
END;
$$;

CREATE OR REPLACE FUNCTION inventario.fn_kardex_producto(
    p_producto_id bigint DEFAULT NULL,
    p_desde timestamptz DEFAULT NULL,
    p_hasta timestamptz DEFAULT NULL,
    p_tipo integer DEFAULT NULL,
    p_pagina integer DEFAULT 1,
    p_tamano integer DEFAULT 20
)
RETURNS TABLE (
    movimiento_id bigint, producto_id bigint, sku varchar, producto varchar,
    tipo smallint, cantidad numeric, costo_unitario numeric, valor numeric,
    stock_resultante numeric, proveedor varchar, motivo varchar, referencia varchar,
    fecha timestamptz, registrado_en timestamptz, usuario varchar
)
LANGUAGE plpgsql STABLE
SET search_path = pg_catalog, inventario, pg_temp
AS $$
BEGIN
    IF p_pagina IS NULL OR p_pagina < 1
        OR (p_tamano IS NOT NULL AND p_tamano NOT BETWEEN 1 AND 100)
        OR (p_tipo IS NOT NULL AND p_tipo NOT IN (1, 2))
        OR (p_desde IS NOT NULL AND p_hasta IS NOT NULL AND p_desde >= p_hasta) THEN
        RAISE EXCEPTION USING ERRCODE = 'P2004', MESSAGE = 'Filtros inválidos';
    END IF;

    RETURN QUERY
    SELECT m.id, m.producto_id, p.sku, p.nombre, m.tipo, m.cantidad,
        m.costo_unitario, round(m.cantidad * m.costo_unitario, 2),
        m.stock_resultante, m.proveedor, m.motivo, m.referencia,
        m.fecha, m.creado_en, u.nombre
    FROM inventario.movimientos_inventario m
    JOIN inventario.productos p ON p.id = m.producto_id
    JOIN inventario.usuarios u ON u.id = m.usuario_id
    WHERE (p_producto_id IS NULL OR m.producto_id = p_producto_id)
      AND (p_desde IS NULL OR m.fecha >= p_desde)
      AND (p_hasta IS NULL OR m.fecha < p_hasta)
      AND (p_tipo IS NULL OR m.tipo = p_tipo)
    ORDER BY m.id DESC
    LIMIT p_tamano OFFSET (p_pagina::bigint - 1) * coalesce(p_tamano, 0);
END;
$$;

CREATE OR REPLACE FUNCTION inventario.fn_resumen_dashboard()
RETURNS TABLE (
    productos_activos bigint, stock_total numeric, valor_inventario numeric,
    productos_bajo_stock bigint, stock_por_unidad jsonb, movimientos_por_mes jsonb
)
LANGUAGE sql STABLE
SET search_path = pg_catalog, inventario, pg_temp
AS $$
    WITH existencias AS (
        SELECT * FROM inventario.productos
    ), unidades AS (
        SELECT unidad_medida, sum(stock) AS cantidad
        FROM existencias GROUP BY unidad_medida
    ), meses AS (
        SELECT generate_series(
            date_trunc('month', now() AT TIME ZONE 'UTC') - interval '5 months',
            date_trunc('month', now() AT TIME ZONE 'UTC'), interval '1 month'
        ) AS mes
    ), actividad AS (
        SELECT t.mes,
            count(m.id) FILTER (WHERE m.tipo = 1) AS entradas,
            count(m.id) FILTER (WHERE m.tipo = 2) AS salidas
        FROM meses t
        LEFT JOIN inventario.movimientos_inventario m
            ON m.fecha >= t.mes AT TIME ZONE 'UTC'
           AND m.fecha < (t.mes + interval '1 month') AT TIME ZONE 'UTC'
        GROUP BY t.mes
    )
    SELECT count(*) FILTER (WHERE activo), coalesce(sum(stock), 0),
        coalesce(sum(round(stock * costo_promedio, 2)), 0),
        count(*) FILTER (WHERE activo AND stock <= stock_minimo),
        (SELECT coalesce(jsonb_object_agg(unidad_medida, cantidad), '{}'::jsonb) FROM unidades),
        (SELECT jsonb_agg(jsonb_build_object('mes', to_char(mes, 'YYYY-MM'),
             'entradas', entradas, 'salidas', salidas) ORDER BY mes) FROM actividad)
    FROM existencias;
$$;

REVOKE EXECUTE ON ALL ROUTINES IN SCHEMA inventario FROM PUBLIC;
GRANT EXECUTE ON ALL ROUTINES IN SCHEMA inventario TO inventario_app;

COMMIT;
