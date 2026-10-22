using Inventario.Application.DTOs.Requests;
using FluentValidation;
using Inventario.Application.Common;
namespace Inventario.Application.Validators.Movimientos;

public sealed class RegistrarSalidaValidator
    : AbstractValidator<RegistrarSalidaRequest>
{
    public RegistrarSalidaValidator()
    {
        RuleFor(x => x.ProductoId)
            .GreaterThan(0);

        RuleFor(x => x.UsuarioId)
            .GreaterThan(0);

        RuleFor(x => x.Cantidad)
            .GreaterThan(0)
            .LessThan(1e15m)
            .Must(x => x == decimal.Round(x, 3))
            .WithMessage("Usa hasta 3 decimales");

        RuleFor(x => x.Motivo)
            .NotEmpty()
            .MaximumLength(250);

        RuleFor(x => x.Referencia)
            .MaximumLength(80);

        RuleFor(x => x.Fecha)
            .Must(x =>
                x is null
                || x.Value.Kind == DateTimeKind.Utc)
            .WithMessage("La fecha debe estar en UTC");
    }
}

// ======================================================
// CASOS DE USO
// ======================================================
