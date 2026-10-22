using Inventario.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Inventario.Infrastructure.Persistence.Configurations;

public sealed class MovimientoInventarioConfiguration
    : IEntityTypeConfiguration<MovimientoInventario>
{
    public void Configure(EntityTypeBuilder<MovimientoInventario> entity)
    {
        entity.ToTable("movimientos_inventario");

        entity.HasKey(x => x.Id);

        entity.Property(x => x.Id)
            .HasColumnName("id")
            .UseIdentityAlwaysColumn();

        entity.Property(x => x.ProductoId)
            .HasColumnName("producto_id");

        entity.Property(x => x.UsuarioId)
            .HasColumnName("usuario_id");

        entity.Property(x => x.Tipo)
            .HasColumnName("tipo")
            .HasConversion<short>();

        entity.Property(x => x.Cantidad)
            .HasColumnName("cantidad")
            .HasPrecision(18, 3);

        entity.Property(x => x.CostoUnitario)
            .HasColumnName("costo_unitario")
            .HasPrecision(18, 6);

        entity.Property(x => x.StockResultante)
            .HasColumnName("stock_resultante")
            .HasPrecision(18, 3);

        entity.Property(x => x.Proveedor)
            .HasColumnName("proveedor")
            .HasMaxLength(150);

        entity.Property(x => x.Motivo)
            .HasColumnName("motivo")
            .HasMaxLength(250);

        entity.Property(x => x.Referencia)
            .HasColumnName("referencia")
            .HasMaxLength(80);

        entity.Property(x => x.Fecha)
            .HasColumnName("fecha");

        entity.Property(x => x.CreadoEn)
            .HasColumnName("creado_en");

        entity.HasOne<Producto>()
            .WithMany()
            .HasForeignKey(x => x.ProductoId);

        entity.HasOne<Usuario>()
            .WithMany()
            .HasForeignKey(x => x.UsuarioId);
    }
}