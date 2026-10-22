using Inventario.Application.Common.Security;
using Inventario.Application.Common;
using Inventario.Domain.Entities;

namespace Inventario.Application.Interfaces.Security;

public interface ITokenService
{
    NuevoAccessToken CrearAccessToken(Usuario usuario, DateTime ahora);

    NuevoRefreshToken CrearRefreshToken();

    string HashRefreshToken(string valor);
}
