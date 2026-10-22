namespace Inventario.Domain.Exceptions;

public sealed class ProductoNoEncontradoException : DomainException
{
    public ProductoNoEncontradoException(Exception? innerException = null)
        : base("Producto no encontrado", innerException)
    {
    }
}