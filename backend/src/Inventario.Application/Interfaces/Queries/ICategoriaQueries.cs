using Inventario.Application.DTOs.Responses;
using Inventario.Application.Common;
namespace Inventario.Application.Interfaces.Queries;

public interface ICategoriaQueries
{
    Task<bool> ExisteActivaAsync(int id, CancellationToken cancellationToken);

    Task<IReadOnlyList<CategoriaResponse>> ListarAsync(
        CancellationToken cancellationToken);
}
