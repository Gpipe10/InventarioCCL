using FluentValidation;
using Inventario.Application.Common;
using Inventario.Domain.Enums;

namespace Inventario.Application.DTOs.Responses;

public sealed record InventarioResponse(
    long ProductoId,
    string Sku,
    string Nombre,
    string Categoria,
    string UnidadMedida,
    decimal Precio,
    decimal Stock,
    decimal StockMinimo,
    decimal CostoPromedio,
    decimal ValorInventario,
    bool BajoStock,
    bool Activo
);

// ======================================================
// FILTROS DE KARDEX
// ======================================================
