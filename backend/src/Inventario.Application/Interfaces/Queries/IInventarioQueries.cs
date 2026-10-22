using Inventario.Application.DTOs.Requests;
using Inventario.Application.DTOs.Responses;
using Inventario.Application.Common;
namespace Inventario.Application.Interfaces.Queries;

public interface IInventarioQueries
{
    Task<Page<InventarioResponse>> InventarioAsync(
        FiltroInventarioRequest filtro,
        CancellationToken cancellationToken);

    Task<Page<KardexResponse>> KardexAsync(
        FiltroKardexRequest filtro,
        CancellationToken cancellationToken);

    Task<DashboardResponse> DashboardAsync(
        CancellationToken cancellationToken);
}
