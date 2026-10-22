using Inventario.Application.DTOs.Requests;
using FluentValidation;
using Inventario.Application.Common;
using Inventario.Domain.Entities;
using Inventario.Domain.Enums;

namespace Inventario.Application.Validators.Productos;

public sealed class FiltroProductosValidator
    : AbstractValidator<FiltroProductosRequest>
{
    public FiltroProductosValidator()
    {
        RuleFor(x => x.Busqueda)
            .MaximumLength(150);

        RuleFor(x => x.CategoriaId)
            .GreaterThan(0)
            .When(x => x.CategoriaId.HasValue);

        RuleFor(x => x.Pagina)
            .GreaterThan(0);

        RuleFor(x => x.Tamano)
            .InclusiveBetween(1, 100);
    }
}

// ======================================================
// CASOS DE USO: COMANDOS
// ======================================================
