using FluentValidation;
using Inventario.Application.Common;
using Inventario.Domain.Enums;

namespace Inventario.Application.DTOs.Responses;

public sealed record KardexResponse(
    long MovimientoId,
    long ProductoId,
    string Sku,
    string Producto,
    TipoMovimiento Tipo,
    decimal Cantidad,
    decimal CostoUnitario,
    decimal Valor,
    decimal StockResultante,
    string? Proveedor,
    string? Motivo,
    string? Referencia,
    DateTime Fecha,
    DateTime RegistradoEn,
    string Usuario
);

// ======================================================
// DTOs DEL DASHBOARD
// ======================================================
