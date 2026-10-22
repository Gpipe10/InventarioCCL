using Inventario.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Inventario.Infrastructure.Persistence;

public sealed class InventarioDbContext(
    DbContextOptions<InventarioDbContext> options
) : DbContext(options)
{
    public DbSet<Producto> Productos => Set<Producto>();

    public DbSet<Categoria> Categorias => Set<Categoria>();

    public DbSet<Usuario> Usuarios => Set<Usuario>();

    public DbSet<Rol> Roles => Set<Rol>();

    public DbSet<MovimientoInventario> Movimientos =>
        Set<MovimientoInventario>();

    public DbSet<RefreshToken> RefreshTokens =>
        Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.HasDefaultSchema("inventario");

        builder.ApplyConfigurationsFromAssembly(
            typeof(InventarioDbContext).Assembly
        );
    }
}