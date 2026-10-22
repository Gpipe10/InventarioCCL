using Inventario.Application.Interfaces.Repositories;
using Inventario.Domain.Entities;
using Inventario.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Npgsql;
using NpgsqlTypes;
using System.Data;

namespace Inventario.Infrastructure.Repositories;

public sealed class SesionRepository(
    InventarioDbContext db,
    UnitOfWork unitOfWork
) : ISesionRepository
{
    // ======================================================
    // CREAR SESIÓN
    // ======================================================

    public async Task CrearAsync(
        RefreshToken token,
        CancellationToken cancellationToken)
    {
        db.RefreshTokens.Add(token);

        await unitOfWork.SaveChangesAsync(
            cancellationToken);
    }

    // ======================================================
    // OBTENER REFRESH TOKEN POR HASH
    // ======================================================

    public Task<RefreshToken?> ObtenerPorHashAsync(
        string hash,
        CancellationToken cancellationToken)
    {
        return db.RefreshTokens
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.TokenHash == hash,
                cancellationToken);
    }

    // ======================================================
    // ROTAR REFRESH TOKEN
    // ======================================================

    public async Task<bool> RotarAsync(
        string hash,
        RefreshToken reemplazo,
        DateTime ahora,
        CancellationToken cancellationToken)
    {
        await using var transaction =
            await db.Database.BeginTransactionAsync(
                cancellationToken);

        await BloquearFamiliaAsync(
            reemplazo.FamiliaId,
            cancellationToken);

        var anterior = await db.RefreshTokens
            .FirstOrDefaultAsync(
                x => x.TokenHash == hash,
                cancellationToken);

        if (anterior is null ||
            anterior.FamiliaId != reemplazo.FamiliaId ||
            anterior.UsuarioId != reemplazo.UsuarioId)
        {
            return false;
        }

        // Si el token ya no es válido,
        // revocamos toda la familia.
        if (!anterior.EstaVigente(ahora))
        {
            await RevocarFamiliaSinBloqueoAsync(
                anterior.FamiliaId,
                ahora,
                cancellationToken);

            await transaction.CommitAsync(
                cancellationToken);

            return false;
        }

        // Insertamos el token nuevo.
        db.RefreshTokens.Add(reemplazo);

        await unitOfWork.SaveChangesAsync(
            cancellationToken);

        // Revocamos el anterior y registramos
        // qué token lo reemplazó.
        anterior.Rotar(reemplazo, ahora);

        await unitOfWork.SaveChangesAsync(
            cancellationToken);

        await transaction.CommitAsync(
            cancellationToken);

        return true;
    }

    // ======================================================
    // REVOCAR FAMILIA DE TOKENS
    // ======================================================

    public async Task RevocarFamiliaAsync(
        Guid familiaId,
        DateTime ahora,
        CancellationToken cancellationToken)
    {
        await using var transaction =
            await db.Database.BeginTransactionAsync(
                cancellationToken);

        await BloquearFamiliaAsync(
            familiaId,
            cancellationToken);

        await RevocarFamiliaSinBloqueoAsync(
            familiaId,
            ahora,
            cancellationToken);

        await transaction.CommitAsync(
            cancellationToken);
    }

    // ======================================================
    // BLOQUEAR FAMILIA DURANTE LA TRANSACCIÓN
    // ======================================================

    private async Task BloquearFamiliaAsync(
        Guid familiaId,
        CancellationToken cancellationToken)
    {
        var connection = (NpgsqlConnection)
            db.Database.GetDbConnection();

        var transaction = db.Database.CurrentTransaction
            ?? throw new InvalidOperationException(
                "Se requiere una transacción activa.");

        await using var command = new NpgsqlCommand(
            """
            SELECT pg_advisory_xact_lock(
                hashtextextended(@familia::text, 0)
            )
            """,
            connection,
            (NpgsqlTransaction)transaction.GetDbTransaction());

        command.Parameters.Add(
            new NpgsqlParameter(
                "familia",
                NpgsqlDbType.Text)
            {
                Value = familiaId.ToString()
            });

        await command.ExecuteScalarAsync(
            cancellationToken);
    }

    // ======================================================
    // REVOCAR TOKENS ACTIVOS
    // ======================================================

    private Task<int> RevocarFamiliaSinBloqueoAsync(
        Guid familiaId,
        DateTime ahora,
        CancellationToken cancellationToken)
    {
        return db.RefreshTokens
            .Where(x =>
                x.FamiliaId == familiaId &&
                x.RevocadoEn == null &&
                x.CreadoEn <= ahora)
            .ExecuteUpdateAsync(
                actualizacion => actualizacion
                    .SetProperty(
                        x => x.RevocadoEn,
                        ahora),
                cancellationToken);
    }
}