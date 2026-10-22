using Inventario.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Inventario.Infrastructure.Persistence.Configurations;

public sealed class RolConfiguration : IEntityTypeConfiguration<Rol>
{
    public void Configure(EntityTypeBuilder<Rol> entity)
    {
        entity.ToTable("roles");

        entity.HasKey(x => x.Id);

        entity.Property(x => x.Id)
            .HasColumnName("id")
            .HasConversion<short>()
            .ValueGeneratedNever();

        entity.Property(x => x.Nombre)
            .HasColumnName("nombre")
            .HasMaxLength(20)
            .IsRequired();
    }
}