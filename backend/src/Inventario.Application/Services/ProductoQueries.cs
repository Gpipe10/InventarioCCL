using Inventario.Application.DTOs.Requests;
using Inventario.Application.DTOs.Responses;
using Inventario.Application.Interfaces.Queries;
using Inventario.Application.Interfaces.Repositories;
using FluentValidation;
using Inventario.Application.Common;
using Inventario.Domain.Entities;
using Inventario.Domain.Enums;

namespace Inventario.Application.Services;

public sealed class ProductoQueries(
    IProductoRepository productos,
    ICategoriaQueries categorias,
    IValidator<FiltroProductosRequest> validator)
{
    // --------------------------------------------------
    // OBTENER PRODUCTO POR ID
    // --------------------------------------------------

    public async Task<Result<ProductoResponse>> ObtenerAsync(
        long id,
        CancellationToken cancellationToken)
    {
        if (id <= 0)
        {
            return Result<ProductoResponse>.Failure(
                new Error(
                    "validation",
                    "Identificador inválido"));
        }

        var producto = await productos.ObtenerPorIdAsync(
            id,
            cancellationToken);

        return producto is null
            ? Result<ProductoResponse>.Failure(
                new Error(
                    "not_found",
                    "Producto no encontrado"))
            : Result<ProductoResponse>.Success(
                ProductoResponse.Desde(producto));
    }

    // --------------------------------------------------
    // LISTAR PRODUCTOS
    // --------------------------------------------------

    public async Task<Result<Page<ProductoResponse>>> ListarAsync(
        FiltroProductosRequest filtro,
        CancellationToken cancellationToken)
    {
        var validacion = await validator.ValidateAsync(
            filtro,
            cancellationToken);

        if (!validacion.IsValid)
        {
            return Result<Page<ProductoResponse>>.Failure(
                Error.FromValidation(validacion));
        }

        var page = await productos.ListarAsync(
            filtro,
            cancellationToken);

        return Result<Page<ProductoResponse>>.Success(
            new Page<ProductoResponse>(
                page.Items
                    .Select(ProductoResponse.Desde)
                    .ToArray(),
                page.PageNumber,
                page.PageSize,
                page.Total
            ));
    }

    // --------------------------------------------------
    // LISTAR CATEGORÍAS
    // --------------------------------------------------

    public Task<IReadOnlyList<CategoriaResponse>> ListarCategoriasAsync(
        CancellationToken cancellationToken)
    {
        return categorias.ListarAsync(cancellationToken);
    }
}
