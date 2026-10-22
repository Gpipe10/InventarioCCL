using Inventario.Application.DTOs.Requests;
using FluentValidation;
using Inventario.Application.Common;
namespace Inventario.Application.Validators.Movimientos;

public sealed class RegistrarEntradaValidator
    : AbstractValidator<RegistrarEntradaRequest>
{
    public RegistrarEntradaValidator()
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

        RuleFor(x => x.CostoUnitario)
            .GreaterThanOrEqualTo(0)
            .LessThan(1e12m)
            .Must(x => x == decimal.Round(x, 6))
            .WithMessage("Usa hasta 6 decimales");

        RuleFor(x => x)
            .Must(x =>
                !string.IsNullOrWhiteSpace(x.Proveedor)
                || !string.IsNullOrWhiteSpace(x.Motivo))
            .WithMessage("Indica proveedor o motivo");

        RuleFor(x => x.Proveedor)
            .MaximumLength(150);

        RuleFor(x => x.Motivo)
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
