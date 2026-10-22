using Inventario.Application.DTOs.Responses;
using Inventario.Application.Interfaces.Queries;
using Inventario.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Inventario.Infrastructure.Repositories;

public sealed class CategoriaQueries(
    InventarioDbContext db
) : ICategoriaQueries
{
    // ======================================================
    // VERIFICAR CATEGORÍA ACTIVA
    // ======================================================

    public Task<bool> ExisteActivaAsync(
        int id,
        CancellationToken cancellationToken)
    {
        return db.Categorias.AnyAsync(
            x => x.Id == id && x.Activo,
            cancellationToken);
    }

    // ======================================================
    // LISTAR CATEGORÍAS ACTIVAS
    // ======================================================

    public async Task<IReadOnlyList<CategoriaResponse>> ListarAsync(
        CancellationToken cancellationToken)
    {
        return await db.Categorias
            .AsNoTracking()
            .Where(x => x.Activo)
            .OrderBy(x => x.Nombre)
            .Select(x => new CategoriaResponse(
                x.Id,
                x.Nombre))
            .ToListAsync(cancellationToken);
    }
}