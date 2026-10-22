namespace Inventario.Domain.Exceptions;

public sealed class ProductoInactivoException : DomainException
{
    public ProductoInactivoException(Exception? innerException = null)
        : base("El producto está inactivo", innerException)
    {
    }
}