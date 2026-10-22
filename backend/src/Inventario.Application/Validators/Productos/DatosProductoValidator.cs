using Inventario.Application.DTOs.Requests;
using FluentValidation;
using Inventario.Application.Common;
using Inventario.Domain.Entities;
using Inventario.Domain.Enums;

namespace Inventario.Application.Validators.Productos;

public sealed class DatosProductoValidator
    : AbstractValidator<DatosProductoRequest>
{
    public DatosProductoValidator()
    {
        RuleFor(x => x.Sku)
            .NotEmpty()
            .MaximumLength(40)
            .Matches("^[a-zA-Z0-9][a-zA-Z0-9._-]*$")
            .WithMessage("SKU inválido");

        RuleFor(x => x.Nombre)
            .NotEmpty()
            .MaximumLength(150);

        RuleFor(x => x.Descripcion)
            .MaximumLength(1000);

        RuleFor(x => x.CategoriaId)
            .GreaterThan(0);

        RuleFor(x => x.UnidadMedida)
            .IsInEnum();

        RuleFor(x => x.Precio)
            .GreaterThanOrEqualTo(0)
            .LessThan(1e16m)
            .Must(x => x == decimal.Round(x, 2))
            .WithMessage("Usa hasta 2 decimales");

        RuleFor(x => x.StockMinimo)
            .GreaterThanOrEqualTo(0)
            .LessThan(1e15m)
            .Must(x => x == decimal.Round(x, 3))
            .WithMessage("Usa hasta 3 decimales");
    }
}
