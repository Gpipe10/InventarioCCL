namespace Inventario.Domain.Exceptions;

public sealed class UsuarioNoDisponibleException : DomainException
{
    public UsuarioNoDisponibleException(Exception? innerException = null)
        : base("Usuario no disponible", innerException)
    {
    }
}