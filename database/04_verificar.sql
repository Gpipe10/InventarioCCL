\set ON_ERROR_STOP on

BEGIN;

DO $$
DECLARE
    v_producto_id bigint;
    v_usuario_id bigint;
    v_stock_inicial numeric;
    v_stock_actual numeric;
    v_movimiento_id bigint;
    v_error_detectado boolean;
BEGIN
    SELECT id, stock
    INTO v_producto_id, v_stock_inicial
    FROM inventario.productos
    WHERE sku = 'ACE-10W40-1L';

    SELECT id
    INTO v_usuario_id
    FROM inventario.usuarios
    WHERE email = 'admin@inventario.local';

    IF v_producto_id IS NULL OR v_usuario_id IS NULL THEN
        RAISE EXCEPTION 'No se encontraron los datos iniciales';
    END IF;

    -- Prueba 1: registrar una entrada.
    CALL inventario.sp_registrar_entrada(
        v_producto_id,
        5,
        20,
        v_usuario_id,
        'Proveedor de prueba',
        'Verificación de entrada',
        'TEST-001',
        now(),
        v_movimiento_id
    );

    SELECT stock INTO v_stock_actual
    FROM inventario.productos
    WHERE id = v_producto_id;

    IF v_stock_actual <> v_stock_inicial + 5 THEN
        RAISE EXCEPTION 'Falló la prueba de entrada';
    END IF;

    -- Prueba 2: registrar una salida.
    CALL inventario.sp_registrar_salida(
        v_producto_id,
        3,
        v_usuario_id,
        'Verificación de salida',
        'TEST-002',
        now(),
        v_movimiento_id
    );

    SELECT stock INTO v_stock_actual
    FROM inventario.productos
    WHERE id = v_producto_id;

    IF v_stock_actual <> v_stock_inicial + 2 THEN
        RAISE EXCEPTION 'Falló la prueba de salida';
    END IF;

    -- Prueba 3: rechazar una salida sin stock suficiente.
    v_error_detectado := false;

    BEGIN
        CALL inventario.sp_registrar_salida(
            v_producto_id,
            999999,
            v_usuario_id,
            'Salida que debe rechazarse',
            'TEST-003',
            now(),
            v_movimiento_id
        );
    EXCEPTION WHEN SQLSTATE 'P2001' THEN
        v_error_detectado := true;
    END;

    IF NOT v_error_detectado THEN
        RAISE EXCEPTION 'Falló la validación de stock insuficiente';
    END IF;

    -- Prueba 4: comprobar que el usuario de aplicación
    -- no puede modificar directamente el stock.
    v_error_detectado := false;

    BEGIN
        EXECUTE 'SET LOCAL ROLE inventario_app';

        UPDATE inventario.productos
        SET stock = 999
        WHERE id = v_producto_id;

    EXCEPTION WHEN insufficient_privilege THEN
        v_error_detectado := true;
    END;

    RESET ROLE;

    IF NOT v_error_detectado THEN
        RAISE EXCEPTION 'Falló la prueba de permisos';
    END IF;

    RAISE NOTICE 'Entrada, salida, stock insuficiente y permisos: OK';
END;
$$;

ROLLBACK;