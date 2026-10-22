using Inventario.Domain.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Inventario.Presentation.Errors;

public sealed class ApiExceptionHandler(
    ILogger<ApiExceptionHandler> logger
) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext context,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var status = exception switch
        {
            ProductoNoEncontradoException
                => StatusCodes.Status404NotFound,

            StockInsuficienteException
                or ProductoInactivoException
                or SkuDuplicadoException
                => StatusCodes.Status409Conflict,

            UsuarioNoDisponibleException
                => StatusCodes.Status401Unauthorized,

            ReglaNegocioException
                => StatusCodes.Status400BadRequest,

            _ => StatusCodes.Status500InternalServerError
        };

        if (status == StatusCodes.Status500InternalServerError)
        {
            logger.LogError(
                exception,
                "Error no controlado en {Path}",
                context.Request.Path);
        }
        else
        {
            logger.LogWarning(
                exception,
                "Error de negocio en {Path}",
                context.Request.Path);
        }

        context.Response.StatusCode = status;
        context.Response.ContentType = "application/problem+json";

        var problem = new ProblemDetails
        {
            Status = status,
            Title = status == 500
                ? "Error interno del servidor"
                : exception.Message,

            Detail = status == 500
                ? "No se pudo completar la solicitud."
                : null
        };

        await context.Response.WriteAsJsonAsync(
            problem,
            cancellationToken);

        return true;
    }
}