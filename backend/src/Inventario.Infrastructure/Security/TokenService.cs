using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Inventario.Application.Common.Security;
using Inventario.Application.Interfaces.Security;
using Inventario.Domain.Entities;
using Microsoft.IdentityModel.Tokens;

namespace Inventario.Infrastructure.Security;

// ======================================================
// CONFIGURACIÓN DE JWT
// ======================================================

public sealed record JwtOptions(
    string Issuer,
    string Audience,
    string Secret,
    int AccessMinutes
);

// ======================================================
// SERVICIO DE TOKENS
// ======================================================

public sealed class TokenService(
    JwtOptions options
) : ITokenService
{
    // ======================================================
    // CREAR ACCESS TOKEN
    // ======================================================

    public NuevoAccessToken CrearAccessToken(
        Usuario usuario,
        DateTime ahora)
    {
        if (Encoding.UTF8.GetByteCount(options.Secret) < 32)
        {
            throw new InvalidOperationException(
                "La clave JWT debe tener al menos 32 bytes.");
        }

        if (options.AccessMinutes <= 0)
        {
            throw new InvalidOperationException(
                "La duración del Access Token debe ser positiva.");
        }

        var expira = ahora.AddMinutes(
            options.AccessMinutes);

        // Información que incluiremos en el JWT.
        var claims = new[]
        {
            new Claim(
                JwtRegisteredClaimNames.Sub,
                usuario.Id.ToString(
                    System.Globalization.CultureInfo.InvariantCulture)),

            new Claim(
                JwtRegisteredClaimNames.Jti,
                Guid.NewGuid().ToString("N")),

            new Claim(
                ClaimTypes.Name,
                usuario.Nombre),

            new Claim(
                ClaimTypes.Role,
                usuario.RolId.ToString())
        };

        // Clave utilizada para firmar el token.
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(options.Secret));

        var credentials = new SigningCredentials(
            key,
            SecurityAlgorithms.HmacSha256);

        var jwt = new JwtSecurityToken(
            issuer: options.Issuer,
            audience: options.Audience,
            claims: claims,
            notBefore: ahora,
            expires: expira,
            signingCredentials: credentials);

        var token = new JwtSecurityTokenHandler()
            .WriteToken(jwt);

        return new NuevoAccessToken(
            token,
            expira);
    }

    // ======================================================
    // CREAR REFRESH TOKEN
    // ======================================================

    public NuevoRefreshToken CrearRefreshToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(48);

        var valor = Convert.ToBase64String(bytes);

        return new NuevoRefreshToken(
            valor,
            HashRefreshToken(valor));
    }

    // ======================================================
    // CALCULAR HASH DEL REFRESH TOKEN
    // ======================================================

    public string HashRefreshToken(string valor)
    {
        var bytes = Encoding.UTF8.GetBytes(valor);

        var hash = SHA256.HashData(bytes);

        return Convert
            .ToHexString(hash)
            .ToLowerInvariant();
    }
}