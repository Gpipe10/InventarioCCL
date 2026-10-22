using Inventario.Application.DTOs.Requests;
using Inventario.Application.DTOs.Responses;
using Inventario.Application.Interfaces.Repositories;
using Inventario.Application.Interfaces.Security;
using FluentValidation;
using Inventario.Application.Common;
using Inventario.Domain.Entities;
using Inventario.Domain.Enums;

namespace Inventario.Application.Services;

public sealed class AuthCommands(
    IUsuarioRepository usuarios,
    ISesionRepository sesiones,
    IPasswordHasher passwords,
    ITokenService tokens,
    IValidator<LoginRequest> loginValidator,
    IValidator<RenovarSesionRequest> refreshValidator,
    AuthPolicy policy,
    TimeProvider clock)
{
    // --------------------------------------------------
    // ERRORES COMUNES
    // --------------------------------------------------

    private static readonly Error CredencialesInvalidas =
        new(
            "unauthorized",
            "Credenciales incorrectas"
        );

    private static readonly Error SesionInvalida =
        new(
            "unauthorized",
            "Sesi�n inv�lida"
        );

    // --------------------------------------------------
    // INICIAR SESI�N
    // --------------------------------------------------

    public async Task<Result<SesionResponse>> IniciarAsync(
        LoginRequest command,
        CancellationToken cancellationToken)
    {
        var validacion = await loginValidator.ValidateAsync(
            command,
            cancellationToken);

        if (!validacion.IsValid)
        {
            return Result<SesionResponse>.Failure(
                Error.FromValidation(validacion));
        }

        var usuario = await usuarios.ObtenerPorEmailAsync(
            command.Email.Trim().ToLowerInvariant(),
            cancellationToken);

        var correcto = passwords.Verificar(
            command.Password,
            usuario?.PasswordHash);

        var ahora = clock.GetUtcNow().UtcDateTime;

        if (usuario is null || !usuario.PuedeIniciarSesion(ahora))
        {
            return Result<SesionResponse>.Failure(
                CredencialesInvalidas);
        }

        if (!correcto)
        {
            await usuarios.RegistrarIntentoFallidoAsync(
                usuario.Id,
                policy.MaxIntentos,
                policy.DuracionBloqueo,
                ahora,
                cancellationToken);

            return Result<SesionResponse>.Failure(
                CredencialesInvalidas);
        }

        if (!await usuarios.RegistrarAccesoCorrectoAsync(
                usuario.Id,
                ahora,
                cancellationToken))
        {
            return Result<SesionResponse>.Failure(
                CredencialesInvalidas);
        }

        return Result<SesionResponse>.Success(
            await CrearSesionAsync(
                usuario,
                Guid.NewGuid(),
                ahora,
                cancellationToken));
    }

    // --------------------------------------------------
    // RENOVAR SESI�N
    // --------------------------------------------------

    public async Task<Result<SesionResponse>> RenovarAsync(
        RenovarSesionRequest command,
        CancellationToken cancellationToken)
    {
        var validacion = await refreshValidator.ValidateAsync(
            command,
            cancellationToken);

        if (!validacion.IsValid)
        {
            return Result<SesionResponse>.Failure(
                Error.FromValidation(validacion));
        }

        var hash = tokens.HashRefreshToken(
            command.RefreshToken);

        var anterior = await sesiones.ObtenerPorHashAsync(
            hash,
            cancellationToken);

        if (anterior is null)
        {
            return Result<SesionResponse>.Failure(
                SesionInvalida);
        }

        var ahora = clock.GetUtcNow().UtcDateTime;

        if (!anterior.EstaVigente(ahora))
        {
            await sesiones.RevocarFamiliaAsync(
                anterior.FamiliaId,
                ahora,
                cancellationToken);

            return Result<SesionResponse>.Failure(
                SesionInvalida);
        }

        var usuario = await usuarios.ObtenerPorIdAsync(
            anterior.UsuarioId,
            cancellationToken);

        if (usuario is null || !usuario.PuedeIniciarSesion(ahora))
        {
            return Result<SesionResponse>.Failure(
                SesionInvalida);
        }

        var nuevo = tokens.CrearRefreshToken();

        var reemplazo = new RefreshToken(
            usuario.Id,
            nuevo.Hash,
            anterior.FamiliaId,
            ahora,
            ahora.Add(policy.DuracionRefresh));

        if (!await sesiones.RotarAsync(
                hash,
                reemplazo,
                ahora,
                cancellationToken))
        {
            return Result<SesionResponse>.Failure(
                SesionInvalida);
        }

        var access = tokens.CrearAccessToken(
            usuario,
            ahora);

        return Result<SesionResponse>.Success(
            new SesionResponse(
                usuario.Id,
                usuario.Nombre,
                usuario.RolId,
                access.Valor,
                access.ExpiraEn,
                nuevo.Valor,
                reemplazo.ExpiraEn));
    }

    // --------------------------------------------------
    // CERRAR SESI�N
    // --------------------------------------------------

    public async Task<Result<bool>> CerrarAsync(
        CerrarSesionRequest command,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(command.RefreshToken))
        {
            return Result<bool>.Failure(
                new Error(
                    "validation",
                    "Token requerido"));
        }

        var actual = await sesiones.ObtenerPorHashAsync(
            tokens.HashRefreshToken(command.RefreshToken),
            cancellationToken);

        if (actual is not null)
        {
            await sesiones.RevocarFamiliaAsync(
                actual.FamiliaId,
                clock.GetUtcNow().UtcDateTime,
                cancellationToken);
        }

        return Result<bool>.Success(true);
    }

    // --------------------------------------------------
    // M�TODO PRIVADO: CREAR SESI�N
    // --------------------------------------------------

    private async Task<SesionResponse> CrearSesionAsync(
        Usuario usuario,
        Guid familiaId,
        DateTime ahora,
        CancellationToken cancellationToken)
    {
        var nuevo = tokens.CrearRefreshToken();

        var refresh = new RefreshToken(
            usuario.Id,
            nuevo.Hash,
            familiaId,
            ahora,
            ahora.Add(policy.DuracionRefresh));

        await sesiones.CrearAsync(
            refresh,
            cancellationToken);

        var access = tokens.CrearAccessToken(
            usuario,
            ahora);

        return new SesionResponse(
            usuario.Id,
            usuario.Nombre,
            usuario.RolId,
            access.Valor,
            access.ExpiraEn,
            nuevo.Valor,
            refresh.ExpiraEn);
    }
}
