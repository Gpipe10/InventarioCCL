using System.Data;
using System.Globalization;
using Inventario.Application.DTOs.Requests;
using Inventario.Application.Interfaces.Repositories;
using Inventario.Domain.Exceptions;
using Inventario.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using NpgsqlTypes;

namespace Inventario.Infrastructure.Repositories;

public sealed class MovimientoRepository(
    InventarioDbContext db
) : IMovimientoRepository
{
    // ======================================================
    // REGISTRAR ENTRADA DE INVENTARIO
    // ======================================================

    public Task<long> RegistrarEntradaAsync(
        RegistrarEntradaRequest command,
        DateTime fecha,
        CancellationToken cancellationToken)
    {
        var parameters = new[]
        {
            CrearParametro(
                "p_producto_id",
                NpgsqlDbType.Bigint,
                command.ProductoId),

            CrearParametro(
                "p_cantidad",
                NpgsqlDbType.Numeric,
                command.Cantidad),

            CrearParametro(
                "p_costo_unitario",
                NpgsqlDbType.Numeric,
                command.CostoUnitario),

            CrearParametro(
                "p_usuario_id",
                NpgsqlDbType.Bigint,
                command.UsuarioId),

            CrearParametro(
                "p_proveedor",
                NpgsqlDbType.Text,
                command.Proveedor),

            CrearParametro(
                "p_motivo",
                NpgsqlDbType.Text,
                command.Motivo),

            CrearParametro(
                "p_referencia",
                NpgsqlDbType.Text,
                command.Referencia),

            CrearParametro(
                "p_fecha",
                NpgsqlDbType.TimestampTz,
                fecha)
        };

        return EjecutarProcedimientoAsync(
            "sp_registrar_entrada",
            parameters,
            cancellationToken);
    }

    // ======================================================
    // REGISTRAR SALIDA DE INVENTARIO
    // ======================================================

    public Task<long> RegistrarSalidaAsync(
        RegistrarSalidaRequest command,
        DateTime fecha,
        CancellationToken cancellationToken)
    {
        var parameters = new[]
        {
            CrearParametro(
                "p_producto_id",
                NpgsqlDbType.Bigint,
                command.ProductoId),

            CrearParametro(
                "p_cantidad",
                NpgsqlDbType.Numeric,
                command.Cantidad),

            CrearParametro(
                "p_usuario_id",
                NpgsqlDbType.Bigint,
                command.UsuarioId),

            CrearParametro(
                "p_motivo",
                NpgsqlDbType.Text,
                command.Motivo),

            CrearParametro(
                "p_referencia",
                NpgsqlDbType.Text,
                command.Referencia),

            CrearParametro(
                "p_fecha",
                NpgsqlDbType.TimestampTz,
                fecha)
        };

        return EjecutarProcedimientoAsync(
            "sp_registrar_salida",
            parameters,
            cancellationToken);
    }

    // ======================================================
    // EJECUTAR PROCEDIMIENTO ALMACENADO
    // ======================================================

    private async Task<long> EjecutarProcedimientoAsync(
        string nombreProcedimiento,
        NpgsqlParameter[] parametros,
        CancellationToken cancellationToken)
    {
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
            // Construimos los marcadores de parámetros.
            var placeholders = string.Join(
                ", ",
                parametros.Select(
                    parametro => "@" + parametro.ParameterName));

            // El último NULL corresponde al parámetro
            // de salida del procedimiento almacenado.
            var sql =
                $"CALL inventario.{nombreProcedimiento}({placeholders}, NULL)";

            await using var command = new NpgsqlCommand(
                sql,
                connection);

            command.Parameters.AddRange(parametros);

            try
            {
                var resultado = await command.ExecuteScalarAsync(
                    cancellationToken);

                return Convert.ToInt64(
                    resultado,
                    CultureInfo.InvariantCulture);
            }
            catch (PostgresException ex)
                when (ex.SqlState is
                    "P2001" or
                    "P2002" or
                    "P2003" or
                    "P2004" or
                    "P2005")
            {
                throw ex.SqlState switch
                {
                    "P2001" => new StockInsuficienteException(ex),

                    "P2002" => new ProductoNoEncontradoException(ex),

                    "P2003" => new ProductoInactivoException(ex),

                    "P2005" => new UsuarioNoDisponibleException(ex),

                    _ => new ReglaNegocioException(
                        "Datos de movimiento inválidos",
                        ex)
                };
            }
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
    // CREAR PARÁMETROS SQL
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