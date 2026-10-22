using Inventario.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Inventario.Infrastructure.Persistence.Configurations;

public sealed class UsuarioConfiguration
    : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> entity)
    {
        entity.ToTable("usuarios");

        entity.HasKey(x => x.Id);

        entity.Property(x => x.Id)
            .HasColumnName("id")
            .UseIdentityAlwaysColumn();

        entity.Property(x => x.Nombre)
            .HasColumnName("nombre")
            .HasMaxLength(100)
            .IsRequired();

        entity.Property(x => x.Email)
            .HasColumnName("email")
            .HasMaxLength(254)
            .IsRequired();

        entity.Property(x => x.PasswordHash)
            .HasColumnName("password_hash")
            .IsRequired();

        entity.Property(x => x.RolId)
            .HasColumnName("rol_id")
            .HasConversion<short>();

        entity.Property(x => x.Activo)
            .HasColumnName("activo");

        entity.Property(x => x.IntentosFallidos)
            .HasColumnName("intentos_fallidos");

        entity.Property(x => x.BloqueadoHasta)
            .HasColumnName("bloqueado_hasta");

        entity.Property(x => x.CreadoEn)
            .HasColumnName("creado_en");

        entity.Property(x => x.ActualizadoEn)
            .HasColumnName("actualizado_en");

        entity.HasOne<Rol>()
            .WithMany()
            .HasForeignKey(x => x.RolId);
    }
}