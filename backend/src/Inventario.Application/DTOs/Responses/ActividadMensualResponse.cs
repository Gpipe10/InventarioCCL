using FluentValidation;
using Inventario.Application.Common;
using Inventario.Domain.Enums;

namespace Inventario.Application.DTOs.Responses;

public sealed record ActividadMensualResponse(
    string Mes,
    long Entradas,
    long Salidas
);
