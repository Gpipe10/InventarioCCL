using Inventario.Application.Common;
using Inventario.Application.DTOs.Requests;
using Inventario.Application.DTOs.Responses;
using Inventario.Application.Services;
using Inventario.Presentation.Contracts;
using Inventario.Presentation.Errors;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Inventario.Presentation.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(
    AuthCommands commands,
    IConfiguration configuration,
    IWebHostEnvironment environment
) : ControllerBase
{
    private const string CookieName = "inventario_refresh";

    // ======================================================
    // INICIAR SESIÓN
    // ======================================================

    [AllowAnonymous]
    [EnableRateLimiting("auth")]
    [HttpPost("login")]
    public async Task<IActionResult> Login(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken)
    {
        if (!OrigenPermitido())
        {
            return ApiResults.FromError(
                new Error(
                    "forbidden",
                    "Origen no permitido"));
        }

        var result = await commands.IniciarAsync(
            request,
            cancellationToken);

        if (!result.IsSuccess)
        {
            return ApiResults.FromError(result.Error!);
        }

        GuardarCookie(result.Value!);

        return Ok(AccesoResponse.Desde(result.Value!));
    }

    // ======================================================
    // RENOVAR SESIÓN
    // ======================================================

    [AllowAnonymous]
    [EnableRateLimiting("auth")]
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh(
        CancellationToken cancellationToken)
    {
        if (!OrigenPermitido())
        {
            return ApiResults.FromError(
                new Error(
                    "forbidden",
                    "Origen no permitido"));
        }

        if (!Request.Cookies.TryGetValue(
                CookieName,
                out var refreshToken))
        {
            return ApiResults.FromError(
                new Error(
                    "unauthorized",
                    "Sesión inválida"));
        }

        var result = await commands.RenovarAsync(
            new RenovarSesionRequest(refreshToken),
            cancellationToken);

        if (!result.IsSuccess)
        {
            BorrarCookie();

            return ApiResults.FromError(result.Error!);
        }

        GuardarCookie(result.Value!);

        return Ok(AccesoResponse.Desde(result.Value!));
    }

    // ======================================================
    // CERRAR SESIÓN
    // ======================================================

    [AllowAnonymous]
    [EnableRateLimiting("auth")]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout(
        CancellationToken cancellationToken)
    {
        if (!OrigenPermitido())
        {
            return ApiResults.FromError(
                new Error(
                    "forbidden",
                    "Origen no permitido"));
        }

        if (Request.Cookies.TryGetValue(
                CookieName,
                out var refreshToken))
        {
            await commands.CerrarAsync(
                new CerrarSesionRequest(refreshToken),
                cancellationToken);
        }

        BorrarCookie();

        return NoContent();
    }

    // ======================================================
    // COOKIE DEL REFRESH TOKEN
    // ======================================================

    private void GuardarCookie(SesionResponse sesion)
    {
        Response.Cookies.Append(
            CookieName,
            sesion.RefreshToken,
            new CookieOptions
            {
                HttpOnly = true,

                Secure =
                    !environment.IsDevelopment()
                    || Request.IsHttps,

                SameSite = SameSiteMode.Strict,

                Path = "/api/auth",

                Expires = new DateTimeOffset(
                    sesion.RefreshExpiraEn,
                    TimeSpan.Zero),

                IsEssential = true
            });
    }

    private void BorrarCookie()
    {
        Response.Cookies.Delete(
            CookieName,
            new CookieOptions
            {
                Path = "/api/auth",

                Secure =
                    !environment.IsDevelopment()
                    || Request.IsHttps,

                SameSite = SameSiteMode.Strict
            });
    }

    // ======================================================
    // VERIFICAR ORIGEN DE LA PETICIÓN
    // ======================================================

    private bool OrigenPermitido()
    {
        if (!Request.Headers.TryGetValue(
                "Origin",
                out var values))
        {
            return true;
        }

        var origin = values.ToString();

        var sameOrigin =
            $"{Request.Scheme}://{Request.Host}";

        var allowed = configuration
            .GetSection("Cors:Origins")
            .Get<string[]>() ?? Array.Empty<string>();

        return string.Equals(
                origin,
                sameOrigin,
                StringComparison.OrdinalIgnoreCase)
            || allowed.Contains(
                origin,
                StringComparer.OrdinalIgnoreCase);
    }
}