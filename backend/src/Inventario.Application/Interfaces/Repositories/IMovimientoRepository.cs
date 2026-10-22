using Inventario.Application.DTOs.Requests;
using Inventario.Application.Common;
namespace Inventario.Application.Interfaces.Repositories;

public interface IMovimientoRepository
{
    Task<long> RegistrarEntradaAsync(
        RegistrarEntradaRequest command,
        DateTime fecha,
        CancellationToken cancellationToken);

    Task<long> RegistrarSalidaAsync(
        RegistrarSalidaRequest command,
        DateTime fecha,
        CancellationToken cancellationToken);
}
