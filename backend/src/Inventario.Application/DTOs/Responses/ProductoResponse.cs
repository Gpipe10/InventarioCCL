using FluentValidation;
using Inventario.Application.Common;
using Inventario.Domain.Entities;
using Inventario.Domain.Enums;

namespace Inventario.Application.DTOs.Responses;

public sealed record ProductoResponse(
    long Id,
    string Sku,
    string Nombre,
    string? Descripcion,
    int CategoriaId,
    UnidadMedida UnidadMedida,
    decimal Precio,
    decimal StockMinimo,
    decimal Stock,
    decimal CostoPromedio,
    bool Activo,
    DateTime CreadoEn,
    DateTime ActualizadoEn)
{
    public static ProductoResponse Desde(Producto p) =>
        new(
            p.Id,
            p.Sku,
            p.Nombre,
            p.Descripcion,
            p.CategoriaId,
            p.UnidadMedida,
            p.Precio,
            p.StockMinimo,
            p.Stock,
            p.CostoPromedio,
            p.Activo,
            p.CreadoEn,
            p.ActualizadoEn
        );
}
