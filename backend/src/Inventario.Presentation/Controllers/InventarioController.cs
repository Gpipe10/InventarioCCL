using Inventario.Application.DTOs.Requests;
using Inventario.Application.Services;
using Inventario.Presentation.Errors;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Inventario.Presentation.Controllers;

[ApiController]
[Authorize]
[Route("api/inventario")]
public sealed class InventarioController(
    InventarioQueries queries
) : ControllerBase
{
    // ======================================================
    // CONSULTAR INVENTARIO ACTUAL
    // ======================================================

    [HttpGet]
    public async Task<IActionResult> Actual(
        [FromQuery] FiltroInventarioRequest filtro,
        CancellationToken cancellationToken)
    {
        var result = await queries.InventarioAsync(
            filtro,
            cancellationToken);

        return result.IsSuccess
            ? Ok(result.Value)
            : ApiResults.FromError(result.Error!);
    }

    // ======================================================
    // CONSULTAR KARDEX
    // ======================================================

    [HttpGet("kardex")]
    public async Task<IActionResult> Kardex(
        [FromQuery] FiltroKardexRequest filtro,
        CancellationToken cancellationToken)
    {
        var result = await queries.KardexAsync(
            filtro,
            cancellationToken);

        return result.IsSuccess
            ? Ok(result.Value)
            : ApiResults.FromError(result.Error!);
    }

    // ======================================================
    // CONSULTAR DASHBOARD
    // ======================================================

    [HttpGet("dashboard")]
    public async Task<IActionResult> Dashboard(
        CancellationToken cancellationToken)
    {
        var resultado = await queries.DashboardAsync(
            cancellationToken);

        return Ok(resultado);
    }
}