using Inventario.Domain.Enums;

namespace Inventario.Domain.Entities;

public sealed class Producto
{
    public long Id { get; private set; }
    public string Sku { get; private set; } = string.Empty;
    public string Nombre { get; private set; } = string.Empty;
    public string? Descripcion { get; private set; }
    public int CategoriaId { get; private set; }
    public UnidadMedida UnidadMedida { get; private set; }
    public decimal Precio { get; private set; }
    public decimal StockMinimo { get; private set; }
    public decimal Stock { get; private set; }
    public decimal CostoPromedio { get; private set; }
    public bool Activo { get; private set; } = true;
    public DateTime CreadoEn { get; private set; }
    public DateTime ActualizadoEn { get; private set; }
    public long CreadoPor { get; private set; }
    public long? ActualizadoPor { get; private set; }

    private Producto()
    {
    }

    public Producto(string sku, string nombre, string? descripcion, int categoriaId,
        UnidadMedida unidadMedida, decimal precio, decimal stockMinimo,
        long usuarioId, DateTime ahora)
    {
        UnidadMedida = unidadMedida;
        CreadoPor = usuarioId;
        CreadoEn = ahora;
        Actualizar(sku, nombre, descripcion, categoriaId, precio, stockMinimo, usuarioId, ahora);
    }

    public void Actualizar(string sku, string nombre, string? descripcion,
        int categoriaId, decimal precio, decimal stockMinimo, long usuarioId, DateTime ahora)
    {
        Sku = sku.Trim().ToUpperInvariant();
        Nombre = nombre.Trim();
        Descripcion = string.IsNullOrWhiteSpace(descripcion) ? null : descripcion.Trim();
        CategoriaId = categoriaId;
        Precio = precio;
        StockMinimo = stockMinimo;
        ActualizadoPor = usuarioId;
        ActualizadoEn = ahora;
    }

    public void CambiarEstado(bool activo, long usuarioId, DateTime ahora)
    {
        Activo = activo;
        ActualizadoPor = usuarioId;
        ActualizadoEn = ahora;
    }
}