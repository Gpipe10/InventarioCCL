using FluentValidation;
using Inventario.Application.Common;
using Inventario.Domain.Enums;

namespace Inventario.Application.DTOs.Requests;

public sealed record FiltroInventarioRequest(
    string? Busqueda = null,
    int? CategoriaId = null,
    bool? Activo = null,
    bool SoloBajoStock = false,
    string Orden = "nombre",
    bool Descendente = false,
    int Pagina = 1,
    int Tamano = 20
);

// ======================================================
// DTO DE INVENTARIO
// ======================================================
