using Inventario.Application.DTOs.Requests;
using Inventario.Application.DTOs.Responses;
using Inventario.Application.Interfaces.Queries;
using FluentValidation;
using Inventario.Application.Common;
using Inventario.Domain.Enums;

namespace Inventario.Application.Services;

public sealed class InventarioQueries(
    IInventarioQueries queries,
    IValidator<FiltroInventarioRequest> inventarioValidator,
    IValidator<FiltroKardexRequest> kardexValidator)
{
    // --------------------------------------------------
    // CONSULTAR INVENTARIO ACTUAL
    // --------------------------------------------------

    public async Task<Result<Page<InventarioResponse>>> InventarioAsync(
        FiltroInventarioRequest filtro,
        CancellationToken cancellationToken)
    {
        var validacion = await inventarioValidator.ValidateAsync(
            filtro,
            cancellationToken);

        if (!validacion.IsValid)
        {
            return Result<Page<InventarioResponse>>.Failure(
                Error.FromValidation(validacion));
        }

        var resultado = await queries.InventarioAsync(
            filtro,
            cancellationToken);

        return Result<Page<InventarioResponse>>.Success(
            resultado);
    }

    // --------------------------------------------------
    // CONSULTAR KARDEX
    // --------------------------------------------------

    public async Task<Result<Page<KardexResponse>>> KardexAsync(
        FiltroKardexRequest filtro,
        CancellationToken cancellationToken)
    {
        var validacion = await kardexValidator.ValidateAsync(
            filtro,
            cancellationToken);

        if (!validacion.IsValid)
        {
            return Result<Page<KardexResponse>>.Failure(
                Error.FromValidation(validacion));
        }

        var resultado = await queries.KardexAsync(
            filtro,
            cancellationToken);

        return Result<Page<KardexResponse>>.Success(
            resultado);
    }

    // --------------------------------------------------
    // CONSULTAR DASHBOARD
    // --------------------------------------------------

    public Task<DashboardResponse> DashboardAsync(
        CancellationToken cancellationToken)
    {
        return queries.DashboardAsync(
            cancellationToken);
    }
}
