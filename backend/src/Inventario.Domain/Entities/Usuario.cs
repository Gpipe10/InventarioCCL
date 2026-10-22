using Inventario.Domain.Enums;
using Inventario.Domain.Exceptions;

namespace Inventario.Domain.Entities;

public sealed class Usuario
{
    public long Id { get; private set; }
    public string Nombre { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public RolUsuario RolId { get; private set; }
    public bool Activo { get; private set; } = true;
    public int IntentosFallidos { get; private set; }
    public DateTime? BloqueadoHasta { get; private set; }
    public DateTime CreadoEn { get; private set; }
    public DateTime ActualizadoEn { get; private set; }

    private Usuario()
    {
    }

    public Usuario(string nombre, string email, string passwordHash,
        RolUsuario rolId, DateTime ahora)
    {
        Nombre = nombre.Trim();
        Email = email.Trim().ToLowerInvariant();
        PasswordHash = passwordHash;
        RolId = rolId;
        CreadoEn = ahora;
        ActualizadoEn = ahora;
    }

    public bool PuedeIniciarSesion(DateTime ahora)
    {
        return Activo && (BloqueadoHasta is null || BloqueadoHasta <= ahora);
    }

    public void RegistrarIntentoFallido(int maxIntentos, TimeSpan duracionBloqueo, DateTime ahora)
    {
        if (maxIntentos < 1)
            throw new ArgumentOutOfRangeException(nameof(maxIntentos));

        if (duracionBloqueo <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(duracionBloqueo));

        if (!PuedeIniciarSesion(ahora))
            throw new UsuarioNoDisponibleException();

        if (BloqueadoHasta is not null)
        {
            IntentosFallidos = 0;
            BloqueadoHasta = null;
        }

        IntentosFallidos++;

        if (IntentosFallidos >= maxIntentos)
            BloqueadoHasta = ahora.Add(duracionBloqueo);

        ActualizadoEn = ahora;
    }

    public void RegistrarAccesoCorrecto(DateTime ahora)
    {
        if (!PuedeIniciarSesion(ahora))
            throw new UsuarioNoDisponibleException();

        IntentosFallidos = 0;
        BloqueadoHasta = null;
        ActualizadoEn = ahora;
    }
}