using Inventario.Domain.Enums;

namespace Inventario.Domain.Entities;

public sealed class MovimientoInventario
{
    public long Id { get; private set; }
    public long ProductoId { get; private set; }
    public long UsuarioId { get; private set; }
    public TipoMovimiento Tipo { get; private set; }
    public decimal Cantidad { get; private set; }
    public decimal CostoUnitario { get; private set; }
    public decimal StockResultante { get; private set; }
    public string? Proveedor { get; private set; }
    public string? Motivo { get; private set; }
    public string? Referencia { get; private set; }
    public DateTime Fecha { get; private set; }
    public DateTime CreadoEn { get; private set; }

    private MovimientoInventario()
    {
    }
}