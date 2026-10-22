using FluentValidation;
using Inventario.Application.Common;
using Inventario.Domain.Entities;
using Inventario.Domain.Enums;

namespace Inventario.Application.Common;

public sealed record AuthPolicy(
    int MaxIntentos,
    TimeSpan DuracionBloqueo,
    TimeSpan DuracionRefresh
)
{
    public static AuthPolicy Default =>
        new(
            5,
            TimeSpan.FromMinutes(15),
            TimeSpan.FromDays(7)
        );
}

// ======================================================
// VALIDADORES
// ======================================================
