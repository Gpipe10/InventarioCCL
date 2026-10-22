using Inventario.Application.DTOs.Requests;
using Inventario.Application.Services;
using Inventario.Presentation.Contracts;
using Inventario.Presentation.Errors;
using Inventario.Presentation.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Inventario.Presentation.Controllers;

[ApiController]
[Authorize]
[Route("api/movimientos")]
public sealed class MovimientosController(
    MovimientoCommands commands
) : ControllerBase
{
    // ======================================================
    // REGISTRAR ENTRADA DE INVENTARIO
    // ======================================================

    [HttpPost("entradas")]
    public async Task<IActionResult> Entrada(
        [FromBody] EntradaBody body,
        CancellationToken cancellationToken)
    {
        var usuarioId = AuthenticatedUser.Id(User);

        if (usuarioId is null)
        {
            return Unauthorized();
        }

        var request = new RegistrarEntradaRequest(
            body.ProductoId,
            body.Cantidad,
            body.CostoUnitario,
            body.Proveedor,
            body.Motivo,
            body.Referencia,
            body.Fecha,
            usuarioId.Value);

        var result = await commands.EntradaAsync(
            request,
            cancellationToken);

        return result.IsSuccess
            ? StatusCode(
                StatusCodes.Status201Created,
                result.Value)
            : ApiResults.FromError(result.Error!);
    }

    // ======================================================
    // REGISTRAR SALIDA DE INVENTARIO
    // ======================================================

    [HttpPost("salidas")]
    public async Task<IActionResult> Salida(
        [FromBody] SalidaBody body,
        CancellationToken cancellationToken)
    {
        var usuarioId = AuthenticatedUser.Id(User);

        if (usuarioId is null)
        {
            return Unauthorized();
        }

        var request = new RegistrarSalidaRequest(
            body.ProductoId,
            body.Cantidad,
            body.Motivo,
            body.Referencia,
            body.Fecha,
            usuarioId.Value);

        var result = await commands.SalidaAsync(
            request,
            cancellationToken);

        return result.IsSuccess
            ? StatusCode(
                StatusCodes.Status201Created,
                result.Value)
            : ApiResults.FromError(result.Error!);
    }
}