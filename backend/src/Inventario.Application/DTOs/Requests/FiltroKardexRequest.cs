using FluentValidation;
using Inventario.Application.Common;
using Inventario.Domain.Enums;

namespace Inventario.Application.DTOs.Requests;

public sealed record FiltroKardexRequest(
    long? ProductoId = null,
    DateTime? Desde = null,
    DateTime? Hasta = null,
    TipoMovimiento? Tipo = null,
    int Pagina = 1,
    int Tamano = 20
);

// ======================================================
// DTO DE KARDEX
// ======================================================
