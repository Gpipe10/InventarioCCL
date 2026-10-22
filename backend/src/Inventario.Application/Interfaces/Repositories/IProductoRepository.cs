using Inventario.Application.DTOs.Requests;
using Inventario.Application.Common;
using Inventario.Domain.Entities;

namespace Inventario.Application.Interfaces.Repositories;

public interface IProductoRepository
{
    Task<Producto?> ObtenerPorIdAsync(long id, CancellationToken cancellationToken);

    Task<Page<Producto>> ListarAsync(
        FiltroProductosRequest filtro,
        CancellationToken cancellationToken);

    Task<bool> TieneMovimientosAsync(long id, CancellationToken cancellationToken);

    void Eliminar(Producto producto);

    void Agregar(Producto producto);

    Task GuardarCambiosAsync(CancellationToken cancellationToken);
}
