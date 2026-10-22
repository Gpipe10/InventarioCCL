
\set ON_ERROR_STOP on
\getenv seed_admin_password SEED_ADMIN_PASSWORD

BEGIN;
SET LOCAL TIME ZONE 'UTC';

INSERT INTO inventario.roles (id, nombre)
VALUES (1, 'Admin'), (2, 'Operador');

INSERT INTO inventario.usuarios (nombre, email, password_hash, rol_id)
VALUES ('Administrador', 'admin@inventario.local',
    public.crypt(:'seed_admin_password', public.gen_salt('bf', 12)), 1);

INSERT INTO inventario.categorias (nombre)
VALUES ('Lubricantes'), ('Ferretería'), ('Herramientas'), ('Electricidad');

DO $$
DECLARE
    r record;
    v_usuario bigint;
    v_producto bigint;
    v_movimiento bigint;
BEGIN
    SELECT id INTO v_usuario FROM inventario.usuarios WHERE email = 'admin@inventario.local';

    FOR r IN
        SELECT * FROM (VALUES
            ('ACE-10W40-1L', 'Aceite 10W40 1L', 'Lubricantes', 'unidad', 29.90, 12, 8, 18.00, 20.00),
            ('ACE-20W50-1L', 'Aceite 20W50 1L', 'Lubricantes', 'unidad', 24.90, 8, 24, 15.00, 16.50),
            ('GRA-LIT-400', 'Grasa de litio 400g', 'Lubricantes', 'unidad', 12.50, 6, 16, 7.00, 7.50),
            ('TOR-14X2', 'Tornillo 1/4 x 2', 'Ferretería', 'unidad', 0.45, 100, 200, 0.20, 0.22),
            ('TUE-14', 'Tuerca 1/4', 'Ferretería', 'unidad', 0.20, 100, 320, 0.08, 0.09),
            ('ARA-14', 'Arandela plana 1/4', 'Ferretería', 'unidad', 0.10, 100, 400, 0.04, 0.05),
            ('CIN-TEF-12', 'Cinta de teflón 12mm', 'Ferretería', 'unidad', 1.80, 10, 8, 0.80, 0.90),
            ('BRO-MET-6', 'Broca para metal 6mm', 'Herramientas', 'unidad', 4.90, 8, 24, 2.50, 2.75),
            ('LLA-COM-13', 'Llave combinada 13mm', 'Herramientas', 'unidad', 8.90, 5, 12, 4.80, 5.10),
            ('DES-PH2', 'Destornillador Phillips PH2', 'Herramientas', 'unidad', 6.50, 5, 20, 3.00, 3.20),
            ('MAR-500', 'Martillo de acero 500g', 'Herramientas', 'unidad', 14.90, 5, 4, 8.00, 8.50),
            ('CAB-25', 'Cable de cobre 2.5mm', 'Electricidad', 'metro', 1.20, 50, 160, 0.60, 0.65),
            ('CIN-AIS-20', 'Cinta aislante 20m', 'Electricidad', 'unidad', 2.50, 15, 40, 1.10, 1.20),
            ('TOM-DOB', 'Tomacorriente doble', 'Electricidad', 'unidad', 5.90, 8, 16, 2.80, 3.00),
            ('INT-10A', 'Interruptor simple 10A', 'Electricidad', 'unidad', 4.50, 8, 24, 2.20, 2.40)
        ) AS datos(sku, nombre, categoria, unidad, precio, minimo, cantidad, costo, costo_reposicion)
    LOOP
        INSERT INTO inventario.productos
            (sku, nombre, descripcion, categoria_id, unidad_medida, precio, stock_minimo, creado_por)
        SELECT r.sku, r.nombre, r.nombre, c.id, r.unidad, r.precio, r.minimo, v_usuario
        FROM inventario.categorias c WHERE c.nombre = r.categoria
        RETURNING id INTO v_producto;

        CALL inventario.sp_registrar_entrada(v_producto, r.cantidad::numeric, r.costo,
            v_usuario, 'Distribuidora Central', 'Compra inicial', 'FAC-2026-010',
            date_trunc('month', now()) - interval '2 months' + interval '5 days', v_movimiento);

        CALL inventario.sp_registrar_salida(v_producto, r.cantidad / 4.0, v_usuario,
            'Venta en mostrador', 'VEN-2026-021',
            date_trunc('month', now()) - interval '1 month' + interval '10 days', v_movimiento);

        CALL inventario.sp_registrar_entrada(v_producto, r.cantidad / 2.0, r.costo_reposicion,
            v_usuario, 'Suministros del Norte', 'Reposición', 'FAC-2026-032',
            now() - interval '1 day', v_movimiento);
    END LOOP;
END;
$$;

COMMIT;
