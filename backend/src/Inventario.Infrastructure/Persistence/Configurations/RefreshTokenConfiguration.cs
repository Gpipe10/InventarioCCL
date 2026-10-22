using Inventario.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Inventario.Infrastructure.Persistence.Configurations;

public sealed class RefreshTokenConfiguration
    : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> entity)
    {
        entity.ToTable("refresh_tokens");

        entity.HasKey(x => x.Id);

        entity.Property(x => x.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        entity.Property(x => x.UsuarioId)
            .HasColumnName("usuario_id");

        entity.Property(x => x.TokenHash)
            .HasColumnName("token_hash")
            .HasColumnType("char(64)")
            .IsRequired();

        entity.HasIndex(x => x.TokenHash)
            .IsUnique();

        entity.Property(x => x.FamiliaId)
            .HasColumnName("familia_id");

        entity.Property(x => x.CreadoEn)
            .HasColumnName("creado_en");

        entity.Property(x => x.ExpiraEn)
            .HasColumnName("expira_en");

        entity.Property(x => x.RevocadoEn)
            .HasColumnName("revocado_en");

        entity.Property(x => x.ReemplazadoPor)
            .HasColumnName("reemplazado_por");

        entity.HasOne<Usuario>()
            .WithMany()
            .HasForeignKey(x => x.UsuarioId);
    }
}