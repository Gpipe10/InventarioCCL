using Inventario.Application.DTOs.Requests;
using FluentValidation;
using Inventario.Application.Common;
using Inventario.Domain.Enums;

namespace Inventario.Application.Validators.Inventario;

public sealed class FiltroInventarioValidator
    : AbstractValidator<FiltroInventarioRequest>
{
    public FiltroInventarioValidator()
    {
        RuleFor(x => x.Busqueda)
            .MaximumLength(150);

        RuleFor(x => x.CategoriaId)
            .GreaterThan(0)
            .When(x => x.CategoriaId.HasValue);

        RuleFor(x => x.Orden)
            .Must(x =>
                x is "nombre"
                    or "sku"
                    or "stock"
                    or "valor")
            .WithMessage("Orden inválido");

        RuleFor(x => x.Pagina)
            .GreaterThan(0);

        RuleFor(x => x.Tamano)
            .InclusiveBetween(1, 100);
    }
}
