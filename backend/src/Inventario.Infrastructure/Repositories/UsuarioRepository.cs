using Inventario.Application.Interfaces.Repositories;
using Inventario.Domain.Entities;
using Inventario.Domain.Exceptions;
using Inventario.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Inventario.Infrastructure.Repositories;

public sealed class UsuarioRepository(
    InventarioDbContext db,
    UnitOfWork unitOfWork
) : IUsuarioRepository
{
    // ======================================================
    // OBTENER USUARIO POR CORREO
    // ======================================================

    public Task<Usuario?> ObtenerPorEmailAsync(
        string email,
        CancellationToken cancellationToken)
    {
        return db.Usuarios
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.Email == email,
                cancellationToken);
    }

    // ======================================================
    // OBTENER USUARIO POR ID
    // ======================================================

    public Task<Usuario?> ObtenerPorIdAsync(
        long id,
        CancellationToken cancellationToken)
    {
        return db.Usuarios
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.Id == id,
                cancellationToken);
    }

    // ======================================================
    // REGISTRAR INTENTO FALLIDO
    // ======================================================

    public async Task RegistrarIntentoFallidoAsync(
        long id,
        int maxIntentos,
        TimeSpan duracionBloqueo,
        DateTime ahora,
        CancellationToken cancellationToken)
    {
        await using var transaction =
            await db.Database.BeginTransactionAsync(
                cancellationToken);

        // Bloquea la fila mientras actualizamos
        // el contador de intentos.
        var usuario = await db.Usuarios
            .FromSqlInterpolated(
                $"SELECT * FROM inventario.usuarios WHERE id = {id} FOR UPDATE")
            .SingleOrDefaultAsync(cancellationToken);

        if (usuario is not null)
        {
            try
            {
                usuario.RegistrarIntentoFallido(
                    maxIntentos,
                    duracionBloqueo,
                    ahora);

                await unitOfWork.SaveChangesAsync(
                    cancellationToken);
            }
            catch (UsuarioNoDisponibleException)
            {
                // La entidad puede rechazar el intento
                // cuando el usuario ya no está disponible.
            }
        }

        await transaction.CommitAsync(cancellationToken);
    }

    // ======================================================
    // REGISTRAR ACCESO CORRECTO
    // ======================================================

    public async Task<bool> RegistrarAccesoCorrectoAsync(
        long id,
        DateTime ahora,
        CancellationToken cancellationToken)
    {
        await using var transaction =
            await db.Database.BeginTransactionAsync(
                cancellationToken);

        var usuario = await db.Usuarios
            .FromSqlInterpolated(
                $"SELECT * FROM inventario.usuarios WHERE id = {id} FOR UPDATE")
            .SingleOrDefaultAsync(cancellationToken);

        if (usuario is null ||
            !usuario.PuedeIniciarSesion(ahora))
        {
            return false;
        }

        usuario.RegistrarAccesoCorrecto(ahora);

        await unitOfWork.SaveChangesAsync(
            cancellationToken);

        await transaction.CommitAsync(
            cancellationToken);

        return true;
    }
}