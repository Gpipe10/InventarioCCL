using FluentValidation;
using Inventario.Application.Common;
using Inventario.Domain.Entities;
using Inventario.Domain.Enums;

namespace Inventario.Application.DTOs.Responses;

public sealed record CategoriaResponse(
    int Id,
    string Nombre
);

// ======================================================
// DATOS DE ENTRADA
// ======================================================
