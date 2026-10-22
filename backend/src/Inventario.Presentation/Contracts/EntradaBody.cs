namespace Inventario.Presentation.Contracts;

public sealed record EntradaBody(
    long ProductoId,
    decimal Cantidad,
    decimal CostoUnitario,
    string? Proveedor,
    string? Motivo,
    string? Referencia,
    DateTime? Fecha
);