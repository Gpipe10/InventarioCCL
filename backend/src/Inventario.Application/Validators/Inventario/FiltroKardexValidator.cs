using Inventario.Application.DTOs.Requests;
using FluentValidation;
using Inventario.Application.Common;
using Inventario.Domain.Enums;

namespace Inventario.Application.Validators.Inventario;

public sealed class FiltroKardexValidator
    : AbstractValidator<FiltroKardexRequest>
{
    public FiltroKardexValidator()
    {
        RuleFor(x => x.ProductoId)
            .GreaterThan(0)
            .When(x => x.ProductoId.HasValue);

        RuleFor(x => x.Tipo)
            .IsInEnum()
            .When(x => x.Tipo.HasValue);

        RuleFor(x => x.Desde)
            .Must(x =>
                x is null
                || x.Value.Kind == DateTimeKind.Utc);

        RuleFor(x => x.Hasta)
            .Must(x =>
                x is null
                || x.Value.Kind == DateTimeKind.Utc);

        RuleFor(x => x)
            .Must(x =>
                x.Desde is null
                || x.Hasta is null
                || x.Desde < x.Hasta)
            .WithMessage("El rango de fechas no es válido");

        RuleFor(x => x.Pagina)
            .GreaterThan(0);

        RuleFor(x => x.Tamano)
            .InclusiveBetween(1, 100);
    }
}

// ======================================================
// CASOS DE USO: CONSULTAS
// ======================================================
