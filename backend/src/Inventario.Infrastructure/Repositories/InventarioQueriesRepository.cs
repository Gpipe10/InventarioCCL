using System.Data;
using System.Text.Json;
using Inventario.Application.Common;
using Inventario.Application.DTOs.Requests;
using Inventario.Application.DTOs.Responses;
using Inventario.Application.Interfaces.Queries;
using Inventario.Domain.Enums;
using Inventario.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using NpgsqlTypes;

namespace Inventario.Infrastructure.Repositories;

public sealed class InventarioQueriesRepository(
    InventarioDbContext db
) : IInventarioQueries
{
    // ======================================================
    // CONSULTAR INVENTARIO ACTUAL
    // ======================================================

    public async Task<Page<InventarioResponse>> InventarioAsync(
        FiltroInventarioRequest filtro,
        CancellationToken cancellationToken)
    {
        // Construimos la consulta para contar los registros.
        IQueryable<Inventario.Domain.Entities.Producto> productos =
            db.Productos.AsNoTracking();

        // --------------------------------------------------
        // FILTRO DE BÚSQUEDA
        // --------------------------------------------------

        if (!string.IsNullOrWhiteSpace(filtro.Busqueda))
        {
            var busqueda = filtro.Busqueda
                .Trim()
                .ToLowerInvariant();

            productos = productos.Where(x =>
                x.Nombre.ToLower().Contains(busqueda) ||
                x.Sku.ToLower().Contains(busqueda));
        }

        // --------------------------------------------------
        // FILTRO POR CATEGORÍA
        // --------------------------------------------------

        if (filtro.CategoriaId is int categoriaId)
        {
            productos = productos.Where(x =>
                x.CategoriaId == categoriaId);
        }

        // --------------------------------------------------
        // FILTRO POR ESTADO
        // --------------------------------------------------

        if (filtro.Activo is bool activo)
        {
            productos = productos.Where(x =>
                x.Activo == activo);
        }

        // --------------------------------------------------
        // FILTRO DE STOCK MÍNIMO
        // --------------------------------------------------

        if (filtro.SoloBajoStock)
        {
            productos = productos.Where(x =>
                x.Activo &&
                x.Stock <= x.StockMinimo);
        }

        // --------------------------------------------------
        // TOTAL DE REGISTROS
        // --------------------------------------------------

        var total = await productos.LongCountAsync(
            cancellationToken);

        // --------------------------------------------------
        // OBTENER CONEXIÓN A POSTGRESQL
        // --------------------------------------------------

        var connection = (NpgsqlConnection)
            db.Database.GetDbConnection();

        var cerrarConexion =
            connection.State != ConnectionState.Open;

        if (cerrarConexion)
        {
            await connection.OpenAsync(cancellationToken);
        }

        try
        {
            // --------------------------------------------------
            // CONSULTAR FUNCIÓN DE INVENTARIO
            // --------------------------------------------------

            const string sql = """
                SELECT *
                FROM inventario.fn_inventario_actual(
                    @busqueda,
                    @categoria,
                    @activo,
                    @bajo,
                    @orden,
                    @desc,
                    @pagina,
                    @tamano
                )
                """;

            await using var command = new NpgsqlCommand(
                sql,
                connection);

            // CORRECCIÓN:
            // Declaramos explícitamente el arreglo NpgsqlParameter[].

            command.Parameters.AddRange(
                new NpgsqlParameter[]
                {
                    CrearParametro(
                        "busqueda",
                        NpgsqlDbType.Text,
                        filtro.Busqueda),

                    CrearParametro(
                        "categoria",
                        NpgsqlDbType.Integer,
                        filtro.CategoriaId),

                    CrearParametro(
                        "activo",
                        NpgsqlDbType.Boolean,
                        filtro.Activo),

                    CrearParametro(
                        "bajo",
                        NpgsqlDbType.Boolean,
                        filtro.SoloBajoStock),

                    CrearParametro(
                        "orden",
                        NpgsqlDbType.Text,
                        filtro.Orden),

                    CrearParametro(
                        "desc",
                        NpgsqlDbType.Boolean,
                        filtro.Descendente),

                    CrearParametro(
                        "pagina",
                        NpgsqlDbType.Integer,
                        filtro.Pagina),

                    CrearParametro(
                        "tamano",
                        NpgsqlDbType.Integer,
                        filtro.Tamano)
                });

            // --------------------------------------------------
            // EJECUTAR CONSULTA
            // --------------------------------------------------

            var items = new List<InventarioResponse>();

            await using var reader =
                await command.ExecuteReaderAsync(
                    cancellationToken);

            while (await reader.ReadAsync(cancellationToken))
            {
                items.Add(
                    new InventarioResponse(
                        reader.GetInt64(0),
                        reader.GetString(1),
                        reader.GetString(2),
                        reader.GetString(3),
                        reader.GetString(4),
                        reader.GetDecimal(5),
                        reader.GetDecimal(6),
                        reader.GetDecimal(7),
                        reader.GetDecimal(8),
                        reader.GetDecimal(9),
                        reader.GetBoolean(10),
                        reader.GetBoolean(11)
                    ));
            }

            // --------------------------------------------------
            // DEVOLVER RESULTADO PAGINADO
            // --------------------------------------------------

            return new Page<InventarioResponse>(
                items,
                filtro.Pagina,
                filtro.Tamano,
                total);
        }
        finally
        {
            if (cerrarConexion)
            {
                await connection.CloseAsync();
            }
        }
    }

    // ======================================================
    // CONSULTAR KARDEX
    // ======================================================

    public async Task<Page<KardexResponse>> KardexAsync(
        FiltroKardexRequest filtro,
        CancellationToken cancellationToken)
    {
        // --------------------------------------------------
        // CONSTRUIR CONSULTA PARA CONTAR MOVIMIENTOS
        // --------------------------------------------------

        IQueryable<Inventario.Domain.Entities.MovimientoInventario>
            movimientos = db.Movimientos.AsNoTracking();

        // --------------------------------------------------
        // FILTRO POR PRODUCTO
        // --------------------------------------------------

        if (filtro.ProductoId is long productoId)
        {
            movimientos = movimientos.Where(x =>
                x.ProductoId == productoId);
        }

        // --------------------------------------------------
        // FILTRO FECHA DESDE
        // --------------------------------------------------

        if (filtro.Desde is DateTime desde)
        {
            movimientos = movimientos.Where(x =>
                x.Fecha >= desde);
        }

        // --------------------------------------------------
        // FILTRO FECHA HASTA
        // --------------------------------------------------

        if (filtro.Hasta is DateTime hasta)
        {
            movimientos = movimientos.Where(x =>
                x.Fecha < hasta);
        }

        // --------------------------------------------------
        // FILTRO POR TIPO DE MOVIMIENTO
        // --------------------------------------------------

        if (filtro.Tipo is TipoMovimiento tipo)
        {
            movimientos = movimientos.Where(x =>
                x.Tipo == tipo);
        }

        // --------------------------------------------------
        // TOTAL DE MOVIMIENTOS
        // --------------------------------------------------

        var total = await movimientos.LongCountAsync(
            cancellationToken);

        // --------------------------------------------------
        // OBTENER CONEXIÓN
        // --------------------------------------------------

        var connection = (NpgsqlConnection)
            db.Database.GetDbConnection();

        var cerrarConexion =
            connection.State != ConnectionState.Open;

        if (cerrarConexion)
        {
            await connection.OpenAsync(cancellationToken);
        }

        try
        {
            // --------------------------------------------------
            // CONSULTAR FUNCIÓN KARDEX
            // --------------------------------------------------

            const string sql = """
                SELECT *
                FROM inventario.fn_kardex_producto(
                    @producto,
                    @desde,
                    @hasta,
                    @tipo,
                    @pagina,
                    @tamano
                )
                """;

            await using var command = new NpgsqlCommand(
                sql,
                connection);

            // CORRECCIÓN:
            // Utilizamos un arreglo explícito NpgsqlParameter[].

            command.Parameters.AddRange(
                new NpgsqlParameter[]
                {
                    CrearParametro(
                        "producto",
                        NpgsqlDbType.Bigint,
                        filtro.ProductoId),

                    CrearParametro(
                        "desde",
                        NpgsqlDbType.TimestampTz,
                        filtro.Desde),

                    CrearParametro(
                        "hasta",
                        NpgsqlDbType.TimestampTz,
                        filtro.Hasta),

                    CrearParametro(
                        "tipo",
                        NpgsqlDbType.Integer,
                        filtro.Tipo is null
                            ? null
                            : (int)filtro.Tipo.Value),

                    CrearParametro(
                        "pagina",
                        NpgsqlDbType.Integer,
                        filtro.Pagina),

                    CrearParametro(
                        "tamano",
                        NpgsqlDbType.Integer,
                        filtro.Tamano)
                });

            // --------------------------------------------------
            // EJECUTAR CONSULTA
            // --------------------------------------------------

            var items = new List<KardexResponse>();

            await using var reader =
                await command.ExecuteReaderAsync(
                    cancellationToken);

            while (await reader.ReadAsync(cancellationToken))
            {
                items.Add(
                    new KardexResponse(
                        reader.GetInt64(0),
                        reader.GetInt64(1),
                        reader.GetString(2),
                        reader.GetString(3),
                        (TipoMovimiento)reader.GetInt16(4),
                        reader.GetDecimal(5),
                        reader.GetDecimal(6),
                        reader.GetDecimal(7),
                        reader.GetDecimal(8),
                        ObtenerTextoOpcional(reader, 9),
                        ObtenerTextoOpcional(reader, 10),
                        ObtenerTextoOpcional(reader, 11),
                        reader.GetDateTime(12),
                        reader.GetDateTime(13),
                        reader.GetString(14)
                    ));
            }

            // --------------------------------------------------
            // DEVOLVER RESULTADO PAGINADO
            // --------------------------------------------------

            return new Page<KardexResponse>(
                items,
                filtro.Pagina,
                filtro.Tamano,
                total);
        }
        finally
        {
            if (cerrarConexion)
            {
                await connection.CloseAsync();
            }
        }
    }

    // ======================================================
    // CONSULTAR DASHBOARD
    // ======================================================

    public async Task<DashboardResponse> DashboardAsync(
        CancellationToken cancellationToken)
    {
        // --------------------------------------------------
        // OBTENER CONEXIÓN
        // --------------------------------------------------

        var connection = (NpgsqlConnection)
            db.Database.GetDbConnection();

        var cerrarConexion =
            connection.State != ConnectionState.Open;

        if (cerrarConexion)
        {
            await connection.OpenAsync(cancellationToken);
        }

        try
        {
            // --------------------------------------------------
            // CONSULTAR FUNCIÓN DEL DASHBOARD
            // --------------------------------------------------

            const string sql = """
                SELECT *
                FROM inventario.fn_resumen_dashboard()
                """;

            await using var command = new NpgsqlCommand(
                sql,
                connection);

            await using var reader =
                await command.ExecuteReaderAsync(
                    cancellationToken);

            // --------------------------------------------------
            // VALIDAR RESULTADO
            // --------------------------------------------------

            if (!await reader.ReadAsync(cancellationToken))
            {
                throw new InvalidOperationException(
                    "El dashboard no devolvió datos.");
            }

            // --------------------------------------------------
            // DESERIALIZAR STOCK POR UNIDAD
            // --------------------------------------------------

            var stockPorUnidad =
                JsonSerializer.Deserialize<
                    Dictionary<string, decimal>>(
                        reader.GetString(4))
                ?? new Dictionary<string, decimal>();

            // --------------------------------------------------
            // DESERIALIZAR ACTIVIDAD MENSUAL
            // --------------------------------------------------

            var actividadMensual =
                JsonSerializer.Deserialize<
                    List<ActividadMensualResponse>>(
                        reader.GetString(5),
                        new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        })
                ?? new List<ActividadMensualResponse>();

            // --------------------------------------------------
            // DEVOLVER DASHBOARD
            // --------------------------------------------------

            return new DashboardResponse(
                reader.GetInt64(0),
                reader.GetDecimal(1),
                reader.GetDecimal(2),
                reader.GetInt64(3),
                stockPorUnidad,
                actividadMensual
            );
        }
        finally
        {
            if (cerrarConexion)
            {
                await connection.CloseAsync();
            }
        }
    }

    // ======================================================
    // OBTENER TEXTO OPCIONAL
    // ======================================================

    private static string? ObtenerTextoOpcional(
        NpgsqlDataReader reader,
        int indice)
    {
        return reader.IsDBNull(indice)
            ? null
            : reader.GetString(indice);
    }

    // ======================================================
    // CREAR PARÁMETRO POSTGRESQL
    // ======================================================

    private static NpgsqlParameter CrearParametro(
        string nombre,
        NpgsqlDbType tipo,
        object? valor)
    {
        return new NpgsqlParameter(nombre, tipo)
        {
            Value = valor ?? DBNull.Value
        };
    }
}