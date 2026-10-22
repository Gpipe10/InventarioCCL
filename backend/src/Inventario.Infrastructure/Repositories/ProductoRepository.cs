using Inventario.Application.Common;
using Inventario.Application.DTOs.Requests;
using Inventario.Application.Interfaces.Repositories;
using Inventario.Domain.Entities;
using Inventario.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Inventario.Infrastructure.Repositories;

public sealed class ProductoRepository(
    InventarioDbContext db,
    UnitOfWork unitOfWork
) : IProductoRepository
{
    // ======================================================
    // OBTENER PRODUCTO POR ID
    // ======================================================

    public Task<Producto?> ObtenerPorIdAsync(
        long id,
        CancellationToken cancellationToken)
    {
        return db.Productos
            .FirstOrDefaultAsync(
                x => x.Id == id,
                cancellationToken);
    }

    // ======================================================
    // LISTAR PRODUCTOS CON FILTROS Y PAGINACIÓN
    // ======================================================

    public async Task<Page<Producto>> ListarAsync(
        FiltroProductosRequest filtro,
        CancellationToken cancellationToken)
    {
        IQueryable<Producto> query = db.Productos
            .AsNoTracking();

        // --------------------------------------------------
        // FILTRO DE BÚSQUEDA
        // --------------------------------------------------

        if (!string.IsNullOrWhiteSpace(filtro.Busqueda))
        {
            var busqueda = filtro.Busqueda.Trim().ToLowerInvariant();

            query = query.Where(x =>
                x.Nombre.ToLower().Contains(busqueda) ||
                x.Sku.ToLower().Contains(busqueda));
        }

        // --------------------------------------------------
        // FILTRO POR CATEGORÍA
        // --------------------------------------------------

        if (filtro.CategoriaId is int categoriaId)
        {
            query = query.Where(x =>
                x.CategoriaId == categoriaId);
        }

        // --------------------------------------------------
        // FILTRO POR ESTADO
        // --------------------------------------------------

        if (filtro.Activo is bool activo)
        {
            query = query.Where(x =>
                x.Activo == activo);
        }

        // --------------------------------------------------
        // TOTAL DE REGISTROS
        // --------------------------------------------------

        var total = await query.LongCountAsync(
            cancellationToken);

        // --------------------------------------------------
        // ORDENAMIENTO Y PAGINACIÓN
        // --------------------------------------------------

        var productos = await query
            .OrderBy(x => x.Nombre)
            .ThenBy(x => x.Id)
            .Skip((filtro.Pagina - 1) * filtro.Tamano)
            .Take(filtro.Tamano)
            .ToListAsync(cancellationToken);

        // --------------------------------------------------
        // RESULTADO
        // --------------------------------------------------

        return new Page<Producto>(
            productos,
            filtro.Pagina,
            filtro.Tamano,
            total);
    }

    // ======================================================
    // AGREGAR PRODUCTO
    // ======================================================

    public void Agregar(Producto producto)
    {
        db.Productos.Add(producto);
    }

    // ======================================================
    // VALIDAR HISTORIAL Y ELIMINAR PRODUCTO
    // ======================================================

    public Task<bool> TieneMovimientosAsync(
        long id,
        CancellationToken cancellationToken)
    {
        return db.Movimientos.AnyAsync(
            m => m.ProductoId == id,
            cancellationToken);
    }

    public void Eliminar(Producto producto)
    {
        db.Productos.Remove(producto);
    }

    // ======================================================
    // GUARDAR CAMBIOS
    // ======================================================

    public Task GuardarCambiosAsync(
        CancellationToken cancellationToken)
    {
        return unitOfWork.SaveChangesAsync(
            cancellationToken);
    }
}