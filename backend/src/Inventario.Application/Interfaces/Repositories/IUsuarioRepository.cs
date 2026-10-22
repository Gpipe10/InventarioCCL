using Inventario.Application.Common;
using Inventario.Domain.Entities;

namespace Inventario.Application.Interfaces.Repositories;

public interface IUsuarioRepository
{
    Task<Usuario?> ObtenerPorEmailAsync(
        string email,
        CancellationToken cancellationToken);

    Task<Usuario?> ObtenerPorIdAsync(
        long id,
        CancellationToken cancellationToken);

    Task RegistrarIntentoFallidoAsync(
        long id,
        int maxIntentos,
        TimeSpan duracionBloqueo,
        DateTime ahora,
        CancellationToken cancellationToken);

    Task<bool> RegistrarAccesoCorrectoAsync(
        long id,
        DateTime ahora,
        CancellationToken cancellationToken);
}
