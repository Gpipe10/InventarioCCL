using Inventario.Domain.Exceptions;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Inventario.Infrastructure.Persistence;

public sealed class UnitOfWork(InventarioDbContext db)
{
    public async Task SaveChangesAsync(
        CancellationToken cancellationToken)
    {
        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
            when (ex.InnerException is PostgresException
            {
                SqlState: PostgresErrorCodes.UniqueViolation,
                ConstraintName: "productos_sku_key"
            })
        {
            throw new SkuDuplicadoException(ex);
        }
    }
}