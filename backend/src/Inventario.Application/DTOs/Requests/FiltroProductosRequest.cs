using FluentValidation;
using Inventario.Application.Common;
using Inventario.Domain.Entities;
using Inventario.Domain.Enums;

namespace Inventario.Application.DTOs.Requests;

public sealed record FiltroProductosRequest(
    string? Busqueda = null,
    int? CategoriaId = null,
    bool? Activo = null,
    int Pagina = 1,
    int Tamano = 20
);

// ======================================================
// VALIDADORES
// ======================================================
