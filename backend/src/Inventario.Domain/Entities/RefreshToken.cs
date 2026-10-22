using Inventario.Domain.Exceptions;

namespace Inventario.Domain.Entities;

public sealed class RefreshToken
{
    public Guid Id { get; private set; }
    public long UsuarioId { get; private set; }
    public string TokenHash { get; private set; } = string.Empty;
    public Guid FamiliaId { get; private set; }
    public DateTime CreadoEn { get; private set; }
    public DateTime ExpiraEn { get; private set; }
    public DateTime? RevocadoEn { get; private set; }
    public Guid? ReemplazadoPor { get; private set; }

    private RefreshToken()
    {
    }

    public RefreshToken(long usuarioId, string tokenHash, Guid familiaId,
        DateTime ahora, DateTime expiraEn)
    {
        if (usuarioId <= 0 || familiaId == Guid.Empty || expiraEn <= ahora)
            throw new ReglaNegocioException("Datos de sesión inválidos");

        Id = Guid.NewGuid();
        UsuarioId = usuarioId;
        TokenHash = tokenHash;
        FamiliaId = familiaId;
        CreadoEn = ahora;
        ExpiraEn = expiraEn;
    }

    public bool EstaVigente(DateTime ahora)
    {
        return RevocadoEn is null && CreadoEn <= ahora && ahora < ExpiraEn;
    }

    public void Revocar(DateTime ahora)
    {
        if (RevocadoEn is not null)
            return;

        if (ahora < CreadoEn)
            throw new ReglaNegocioException("La fecha de revocación no es válida");

        RevocadoEn = ahora;
    }

    public void Rotar(RefreshToken reemplazo, DateTime ahora)
    {
        ArgumentNullException.ThrowIfNull(reemplazo);

        if (!EstaVigente(ahora) || !reemplazo.EstaVigente(ahora)
            || reemplazo.Id == Id || reemplazo.UsuarioId != UsuarioId
            || reemplazo.FamiliaId != FamiliaId)
            throw new ReglaNegocioException("La sesión no se puede renovar");

        Revocar(ahora);
        ReemplazadoPor = reemplazo.Id;
    }
}