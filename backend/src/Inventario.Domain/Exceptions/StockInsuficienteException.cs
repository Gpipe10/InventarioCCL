namespace Inventario.Domain.Exceptions;

public sealed class StockInsuficienteException : DomainException
{
    public StockInsuficienteException(Exception? innerException = null)
        : base("No hay stock suficiente", innerException)
    {
    }
}