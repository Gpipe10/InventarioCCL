using Inventario.Application.DTOs.Responses;
using Inventario.Domain.Enums;

namespace Inventario.Presentation.Contracts;

public sealed record AccesoResponse(
    long UsuarioId,
    string Nombre,
    RolUsuario Rol,
    string AccessToken,
    DateTime AccessExpiraEn)
{
    public static AccesoResponse Desde(SesionResponse sesion)
    {
        return new AccesoResponse(
            sesion.UsuarioId,
            sesion.Nombre,
            sesion.Rol,
            sesion.AccessToken,
            sesion.AccessExpiraEn);
    }
}