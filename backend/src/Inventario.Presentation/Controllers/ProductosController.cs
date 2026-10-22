using Inventario.Application.DTOs.Requests;
using Inventario.Application.Services;
using Inventario.Domain.Enums;
using Inventario.Presentation.Contracts;
using Inventario.Presentation.Errors;
using Inventario.Presentation.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Inventario.Presentation.Controllers;

[ApiController]
[Authorize]
[Route("api/productos")]
public sealed class ProductosController(
    ProductoCommands commands,
    ProductoQueries queries
) : ControllerBase
{
    // ======================================================
    // LISTAR PRODUCTOS
    // ======================================================

    [HttpGet]
    public async Task<IActionResult> Listar(
        [FromQuery] FiltroProductosRequest filtro,
        CancellationToken cancellationToken)
    {
        var result = await queries.ListarAsync(
            filtro,
            cancellationToken);

        return result.IsSuccess
            ? Ok(result.Value)
            : ApiResults.FromError(result.Error!);
    }

    // ======================================================
    // LISTAR CATEGORÍAS
    // ======================================================

    [HttpGet("categorias")]
    public async Task<IActionResult> Categorias(
        CancellationToken cancellationToken)
    {
        var categorias = await queries.ListarCategoriasAsync(
            cancellationToken);

        return Ok(categorias);
    }

    // ======================================================
    // OBTENER PRODUCTO POR ID
    // ======================================================

    [HttpGet("{id:long}")]
    public async Task<IActionResult> Obtener(
        long id,
        CancellationToken cancellationToken)
    {
        var result = await queries.ObtenerAsync(
            id,
            cancellationToken);

        return result.IsSuccess
            ? Ok(result.Value)
            : ApiResults.FromError(result.Error!);
    }

    // ======================================================
    // CREAR PRODUCTO
    // ======================================================

    [Authorize(Roles = nameof(RolUsuario.Admin))]
    [HttpPost]
    public async Task<IActionResult> Crear(
        [FromBody] DatosProductoRequest body,
        CancellationToken cancellationToken)
    {
        var usuarioId = AuthenticatedUser.Id(User);

        if (usuarioId is null)
        {
            return Unauthorized();
        }

        var result = await commands.CrearAsync(
            new CrearProductoRequest(
                body,
                usuarioId.Value),
            cancellationToken);

        return result.IsSuccess
            ? CreatedAtAction(
                nameof(Obtener),
                new { id = result.Value!.Id },
                result.Value)
            : ApiResults.FromError(result.Error!);
    }

    // ======================================================
    // EDITAR PRODUCTO
    // ======================================================

    [Authorize(Roles = nameof(RolUsuario.Admin))]
    [HttpPut("{id:long}")]
    public async Task<IActionResult> Editar(
        long id,
        [FromBody] DatosProductoRequest body,
        CancellationToken cancellationToken)
    {
        var usuarioId = AuthenticatedUser.Id(User);

        if (usuarioId is null)
        {
            return Unauthorized();
        }

        var result = await commands.EditarAsync(
            new EditarProductoRequest(
                id,
                body,
                usuarioId.Value),
            cancellationToken);

        return result.IsSuccess
            ? Ok(result.Value)
            : ApiResults.FromError(result.Error!);
    }

    // ======================================================
    // ELIMINAR PRODUCTO (SOLO ADMIN)
    // ======================================================

    [Authorize(Roles = nameof(RolUsuario.Admin))]
    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Eliminar(
        long id,
        CancellationToken cancellationToken)
    {
        var usuarioId = AuthenticatedUser.Id(User);
        if (usuarioId is null)
        {
            return Unauthorized();
        }

        var result = await commands.EliminarAsync(
            id, usuarioId.Value, cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : ApiResults.FromError(result.Error!);
    }

    // ======================================================
    // ACTIVAR O DESACTIVAR PRODUCTO
    // ======================================================

    [Authorize(Roles = nameof(RolUsuario.Admin))]
    [HttpPatch("{id:long}/estado")]
    public async Task<IActionResult> CambiarEstado(
        long id,
        [FromBody] EstadoProductoBody body,
        CancellationToken cancellationToken)
    {
        var usuarioId = AuthenticatedUser.Id(User);
        var rol = AuthenticatedUser.Rol(User);

        if (usuarioId is null || rol is null)
        {
            return Unauthorized();
        }

        var result = await commands.CambiarEstadoAsync(
            new CambiarEstadoProductoRequest(
                id,
                body.Activo,
                usuarioId.Value,
                rol.Value),
            cancellationToken);

        return result.IsSuccess
            ? Ok(result.Value)
            : ApiResults.FromError(result.Error!);
    }
}