using Inventario.Application.Interfaces.Security;

namespace Inventario.Infrastructure.Security;

public sealed class PasswordHasher : IPasswordHasher
{
    // Hash auxiliar para ejecutar una verificación
    // incluso cuando el correo no existe.
    private static readonly string DummyHash =
        BCrypt.Net.BCrypt.HashPassword("dummy", 12);

    // ======================================================
    // GENERAR HASH DE CONTRASEÑA
    // ======================================================

    public string Hash(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(
            password,
            12);
    }

    // ======================================================
    // VERIFICAR CONTRASEÑA
    // ======================================================

    public bool Verificar(
        string password,
        string? hash)
    {
        var candidato = string.IsNullOrEmpty(hash)
            ? DummyHash
            : hash;

        try
        {
            return BCrypt.Net.BCrypt.Verify(
                password,
                candidato)
                && hash is not null;
        }
        catch (BCrypt.Net.SaltParseException)
        {
            return false;
        }
    }
}