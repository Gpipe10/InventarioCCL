namespace Inventario.Domain.Exceptions;

public sealed class SkuDuplicadoException : DomainException
{
    public SkuDuplicadoException(Exception? innerException = null)
        : base("El SKU ya está registrado", innerException)
    {
    }
}