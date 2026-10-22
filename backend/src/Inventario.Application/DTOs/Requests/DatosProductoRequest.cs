using FluentValidation;
using Inventario.Application.Common;
using Inventario.Domain.Entities;
using Inventario.Domain.Enums;

namespace Inventario.Application.DTOs.Requests;

public sealed record DatosProductoRequest(
    string Sku,
    string Nombre,
    string? Descripcion,
    int CategoriaId,
    UnidadMedida UnidadMedida,
    decimal Precio,
    decimal StockMinimo
);

// ======================================================
// COMANDOS
// ======================================================
