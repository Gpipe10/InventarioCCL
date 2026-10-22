namespace Inventario.Domain.Exceptions;

public sealed class ReglaNegocioException : DomainException
{
    public ReglaNegocioException(string message, Exception? innerException = null)
        : base(message, innerException)
    {
    }
}