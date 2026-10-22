using Inventario.Domain.Enums;

namespace Inventario.Domain.Entities;

public sealed class Rol
{
    public RolUsuario Id { get; private set; }
    public string Nombre { get; private set; } = string.Empty;

    private Rol()
    {
    }
}