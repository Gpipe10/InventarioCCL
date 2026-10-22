using Inventario.Application.Common;
using Microsoft.AspNetCore.Mvc;

namespace Inventario.Presentation.Errors;

public static class ApiResults
{
    public static IActionResult FromError(Error error)
    {
        var status = error.Code switch
        {
            "validation" or "categoria" or "usuario"
                => StatusCodes.Status400BadRequest,

            "unauthorized"
                => StatusCodes.Status401Unauthorized,

            "forbidden"
                => StatusCodes.Status403Forbidden,

            "not_found"
                => StatusCodes.Status404NotFound,

            "unidad"
                => StatusCodes.Status409Conflict,

            _ => StatusCodes.Status400BadRequest
        };

        ProblemDetails problem = error.Details is null
            ? new ProblemDetails()
            : new ValidationProblemDetails(
                error.Details.ToDictionary(
                    x => x.Key,
                    x => x.Value));

        problem.Status = status;
        problem.Title = error.Message;
        problem.Extensions["code"] = error.Code;

        return new ObjectResult(problem)
        {
            StatusCode = status
        };
    }
}