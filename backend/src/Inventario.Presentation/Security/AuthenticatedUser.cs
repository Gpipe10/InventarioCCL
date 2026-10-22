using System.Security.Claims;
using Inventario.Domain.Enums;

namespace Inventario.Presentation.Security;

public static class AuthenticatedUser
{
    public static long? Id(ClaimsPrincipal principal)
    {
        var claim = principal.FindFirstValue("sub");

        return long.TryParse(claim, out var id) && id > 0
            ? id
            : null;
    }

    public static RolUsuario? Rol(ClaimsPrincipal principal)
    {
        var claim = principal.FindFirstValue(ClaimTypes.Role);

        return Enum.TryParse<RolUsuario>(claim, out var rol)
            && Enum.IsDefined(rol)
            ? rol
            : null;
    }
}