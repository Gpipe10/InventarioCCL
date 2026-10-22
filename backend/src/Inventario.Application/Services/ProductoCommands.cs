using Inventario.Application.DTOs.Requests;
using Inventario.Application.DTOs.Responses;
using Inventario.Application.Interfaces.Queries;
using Inventario.Application.Interfaces.Repositories;
using FluentValidation;
using Inventario.Application.Common;
using Inventario.Domain.Entities;
using Inventario.Domain.Enums;

namespace Inventario.Application.Services;

public sealed class ProductoCommands(
    IProductoRepository productos,
    ICategoriaQueries categorias,
    IValidator<DatosProductoRequest> validator,
    TimeProvider clock)
{
    // --------------------------------------------------
    // CREAR PRODUCTO
    // --------------------------------------------------

    public async Task<Result<ProductoResponse>> CrearAsync(
        CrearProductoRequest command,
        CancellationToken cancellationToken)
    {
        var validacion = await validator.ValidateAsync(
            command.Datos,
            cancellationToken);

        if (!validacion.IsValid)
        {
            return Result<ProductoResponse>.Failure(
                Error.FromValidation(validacion));
        }

        if (command.UsuarioId <= 0)
        {
            return Result<ProductoResponse>.Failure(
                new Error("usuario", "Usuario inválido"));
        }

        if (!await categorias.ExisteActivaAsync(
                command.Datos.CategoriaId,
                cancellationToken))
        {
            return Result<ProductoResponse>.Failure(
                new Error(
                    "categoria",
                    "Categoría no disponible"));
        }

        if (command.Datos.Precio < 50m)
        {
            return Result<ProductoResponse>.Failure(
                new Error("validation", "El precio mínimo es $50 COP"));
        }

        var d = command.Datos;

        var producto = new Producto(
            d.Sku,
            d.Nombre,
            d.Descripcion,
            d.CategoriaId,
            d.UnidadMedida,
            d.Precio,
            d.StockMinimo,
            command.UsuarioId,
            clock.GetUtcNow().UtcDateTime
        );

        productos.Agregar(producto);

        await productos.GuardarCambiosAsync(
            cancellationToken);

        return Result<ProductoResponse>.Success(
            ProductoResponse.Desde(producto));
    }

    // --------------------------------------------------
    // EDITAR PRODUCTO
    // --------------------------------------------------

    public async Task<Result<ProductoResponse>> EditarAsync(
        EditarProductoRequest command,
        CancellationToken cancellationToken)
    {
        var validacion = await validator.ValidateAsync(
            command.Datos,
            cancellationToken);

        if (!validacion.IsValid)
        {
            return Result<ProductoResponse>.Failure(
                Error.FromValidation(validacion));
        }

        if (command.Id <= 0 || command.UsuarioId <= 0)
        {
            return Result<ProductoResponse>.Failure(
                new Error(
                    "validation",
                    "Identificador inválido"));
        }

        var producto = await productos.ObtenerPorIdAsync(
            command.Id,
            cancellationToken);

        if (producto is null)
        {
            return Result<ProductoResponse>.Failure(
                new Error(
                    "not_found",
                    "Producto no encontrado"));
        }

        if (producto.UnidadMedida != command.Datos.UnidadMedida)
        {
            return Result<ProductoResponse>.Failure(
                new Error(
                    "unidad",
                    "La unidad de medida no se puede cambiar"));
        }

        if (!await categorias.ExisteActivaAsync(
                command.Datos.CategoriaId,
                cancellationToken))
        {
            return Result<ProductoResponse>.Failure(
                new Error(
                    "categoria",
                    "Categoría no disponible"));
        }

        if (command.Datos.Precio < 50m)
        {
            return Result<ProductoResponse>.Failure(
                new Error("validation", "El precio mínimo es $50 COP"));
        }

        var d = command.Datos;

        producto.Actualizar(
            d.Sku,
            d.Nombre,
            d.Descripcion,
            d.CategoriaId,
            d.Precio,
            d.StockMinimo,
            command.UsuarioId,
            clock.GetUtcNow().UtcDateTime
        );

        await productos.GuardarCambiosAsync(
            cancellationToken);

        return Result<ProductoResponse>.Success(
            ProductoResponse.Desde(producto));
    }

    // --------------------------------------------------
    // ELIMINAR PRODUCTO SIN EXISTENCIAS NI MOVIMIENTOS
    // --------------------------------------------------

    public async Task<Result<bool>> EliminarAsync(
        long id,
        long usuarioId,
        CancellationToken cancellationToken)
    {
        if (id <= 0 || usuarioId <= 0)
        {
            return Result<bool>.Failure(
                new Error("validation", "Identificador inválido"));
        }

        var producto = await productos.ObtenerPorIdAsync(
            id, cancellationToken);

        if (producto is null)
        {
            return Result<bool>.Failure(
                new Error("not_found", "Producto no encontrado"));
        }

        if (producto.Stock != 0m)
        {
            return Result<bool>.Failure(
                new Error("validation", "No se puede eliminar un producto con existencias. Ajusta el inventario primero."));
        }

        if (await productos.TieneMovimientosAsync(id, cancellationToken))
        {
            return Result<bool>.Failure(
                new Error("validation", "El producto tiene movimientos en Kardex. Desactívalo en lugar de eliminarlo."));
        }

        productos.Eliminar(producto);
        await productos.GuardarCambiosAsync(cancellationToken);

        return Result<bool>.Success(true);
    }

    // --------------------------------------------------
    // ACTIVAR O DESACTIVAR PRODUCTO
    // --------------------------------------------------

    public async Task<Result<ProductoResponse>> CambiarEstadoAsync(
        CambiarEstadoProductoRequest command,
        CancellationToken cancellationToken)
    {
        if (command.Rol != RolUsuario.Admin)
        {
            return Result<ProductoResponse>.Failure(
                new Error(
                    "forbidden",
                    "No tienes permiso"));
        }

        if (command.Id <= 0 || command.UsuarioId <= 0)
        {
            return Result<ProductoResponse>.Failure(
                new Error(
                    "validation",
                    "Identificador inválido"));
        }

        var producto = await productos.ObtenerPorIdAsync(
            command.Id,
            cancellationToken);

        if (producto is null)
        {
            return Result<ProductoResponse>.Failure(
                new Error(
                    "not_found",
                    "Producto no encontrado"));
        }

        producto.CambiarEstado(
            command.Activo,
            command.UsuarioId,
            clock.GetUtcNow().UtcDateTime
        );

        await productos.GuardarCambiosAsync(
            cancellationToken);

        return Result<ProductoResponse>.Success(
            ProductoResponse.Desde(producto));
    }
}

// ======================================================
// CASOS DE USO: CONSULTAS
// ======================================================
