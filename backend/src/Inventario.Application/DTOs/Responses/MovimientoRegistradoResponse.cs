using FluentValidation;
using Inventario.Application.Common;
namespace Inventario.Application.DTOs.Responses;

public sealed record MovimientoRegistradoResponse(
    long Id
);

// ======================================================
// VALIDADORES
// ======================================================
