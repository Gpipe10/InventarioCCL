using Inventario.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Inventario.Infrastructure.Persistence.Configurations;

public sealed class CategoriaConfiguration : IEntityTypeConfiguration<Categoria>
{
    public void Configure(EntityTypeBuilder<Categoria> entity)
    {
        entity.ToTable("categorias");

        entity.HasKey(x => x.Id);

        entity.Property(x => x.Id)
            .HasColumnName("id")
            .UseIdentityAlwaysColumn();

        entity.Property(x => x.Nombre)
            .HasColumnName("nombre")
            .HasMaxLength(80)
            .IsRequired();

        entity.Property(x => x.Activo)
            .HasColumnName("activo");
    }
}