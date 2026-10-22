namespace Inventario.Presentation.Contracts;

public sealed record SalidaBody(
    long ProductoId,
    decimal Cantidad,
    string Motivo,
    string? Referencia,
    DateTime? Fecha
);