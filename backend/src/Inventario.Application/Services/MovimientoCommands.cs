using Inventario.Application.DTOs.Requests;
using Inventario.Application.DTOs.Responses;
using Inventario.Application.Interfaces.Repositories;
using FluentValidation;
using Inventario.Application.Common;
namespace Inventario.Application.Services;

public sealed class MovimientoCommands(
    IMovimientoRepository movimientos,
    IValidator<RegistrarEntradaRequest> entradaValidator,
    IValidator<RegistrarSalidaRequest> salidaValidator,
    TimeProvider clock)
{
    // --------------------------------------------------
    // REGISTRAR ENTRADA
    // --------------------------------------------------

    public async Task<Result<MovimientoRegistradoResponse>> EntradaAsync(
        RegistrarEntradaRequest command,
        CancellationToken cancellationToken)
    {
        var validacion = await entradaValidator.ValidateAsync(
            command,
            cancellationToken);

        if (!validacion.IsValid)
        {
            return Result<MovimientoRegistradoResponse>.Failure(
                Error.FromValidation(validacion));
        }

        var fecha = command.Fecha
            ?? clock.GetUtcNow().UtcDateTime;

        if (fecha > clock.GetUtcNow().UtcDateTime)
        {
            return Result<MovimientoRegistradoResponse>.Failure(
                new Error(
                    "validation",
                    "Fecha futura no permitida"));
        }

        var id = await movimientos.RegistrarEntradaAsync(
            command,
            fecha,
            cancellationToken);

        return Result<MovimientoRegistradoResponse>.Success(
            new MovimientoRegistradoResponse(id));
    }

    // --------------------------------------------------
    // REGISTRAR SALIDA
    // --------------------------------------------------

    public async Task<Result<MovimientoRegistradoResponse>> SalidaAsync(
        RegistrarSalidaRequest command,
        CancellationToken cancellationToken)
    {
        var validacion = await salidaValidator.ValidateAsync(
            command,
            cancellationToken);

        if (!validacion.IsValid)
        {
            return Result<MovimientoRegistradoResponse>.Failure(
                Error.FromValidation(validacion));
        }

        var fecha = command.Fecha
            ?? clock.GetUtcNow().UtcDateTime;

        if (fecha > clock.GetUtcNow().UtcDateTime)
        {
            return Result<MovimientoRegistradoResponse>.Failure(
                new Error(
                    "validation",
                    "Fecha futura no permitida"));
        }

        var id = await movimientos.RegistrarSalidaAsync(
            command,
            fecha,
            cancellationToken);

        return Result<MovimientoRegistradoResponse>.Success(
            new MovimientoRegistradoResponse(id));
    }
}
