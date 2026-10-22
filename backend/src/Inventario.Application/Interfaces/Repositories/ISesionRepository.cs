using Inventario.Application.Common;
using Inventario.Domain.Entities;

namespace Inventario.Application.Interfaces.Repositories;

public interface ISesionRepository
{
    Task CrearAsync(
        RefreshToken token,
        CancellationToken cancellationToken);

    Task<RefreshToken?> ObtenerPorHashAsync(
        string hash,
        CancellationToken cancellationToken);

    Task<bool> RotarAsync(
        string hash,
        RefreshToken reemplazo,
        DateTime ahora,
        CancellationToken cancellationToken);

    Task RevocarFamiliaAsync(
        Guid familiaId,
        DateTime ahora,
        CancellationToken cancellationToken);
}
