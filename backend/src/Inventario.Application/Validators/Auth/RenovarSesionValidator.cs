using Inventario.Application.DTOs.Requests;
using FluentValidation;
using Inventario.Application.Common;
using Inventario.Domain.Entities;
using Inventario.Domain.Enums;

namespace Inventario.Application.Validators.Auth;

public sealed class RenovarSesionValidator
    : AbstractValidator<RenovarSesionRequest>
{
    public RenovarSesionValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty();
    }
}

// ======================================================
// CASOS DE USO DE AUTENTICACI�N
// ======================================================
