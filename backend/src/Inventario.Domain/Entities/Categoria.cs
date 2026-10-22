namespace Inventario.Domain.Entities;

public sealed class Categoria
{
    public int Id { get; private set; }
    public string Nombre { get; private set; } = string.Empty;
    public bool Activo { get; private set; }

    private Categoria()
    {
    }
}