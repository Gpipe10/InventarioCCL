using Inventario.Domain.Entities;
using Inventario.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Inventario.Infrastructure.Persistence.Configurations;

public sealed class ProductoConfiguration : IEntityTypeConfiguration<Producto>
{
    public void Configure(EntityTypeBuilder<Producto> entity)
    {
        entity.ToTable("productos");

        entity.HasKey(x => x.Id);

        entity.Property(x => x.Id)
            .HasColumnName("id")
            .UseIdentityAlwaysColumn();

        entity.Property(x => x.Sku)
            .HasColumnName("sku")
            .HasMaxLength(40)
            .IsRequired();

        entity.HasIndex(x => x.Sku)
            .IsUnique()
            .HasDatabaseName("productos_sku_key");

        entity.Property(x => x.Nombre)
            .HasColumnName("nombre")
            .HasMaxLength(150)
            .IsRequired();

        entity.Property(x => x.Descripcion)
            .HasColumnName("descripcion")
            .HasMaxLength(1000);

        entity.Property(x => x.CategoriaId)
            .HasColumnName("categoria_id");

        entity.Property(x => x.UnidadMedida)
            .HasColumnName("unidad_medida")
            .HasMaxLength(10)
            .HasConversion(
                unidad => ATexto(unidad),
                texto => DesdeTexto(texto));

        entity.Property(x => x.Precio)
            .HasColumnName("precio")
            .HasPrecision(18, 2);

        entity.Property(x => x.StockMinimo)
            .HasColumnName("stock_minimo")
            .HasPrecision(18, 3);

        entity.Property(x => x.Stock)
            .HasColumnName("stock")
            .HasPrecision(18, 3)
            .HasDefaultValue(0m)
            .ValueGeneratedOnAdd();

        entity.Property(x => x.CostoPromedio)
            .HasColumnName("costo_promedio")
            .HasPrecision(18, 6)
            .HasDefaultValue(0m)
            .ValueGeneratedOnAdd();

        entity.Property(x => x.Activo)
            .HasColumnName("activo");

        entity.Property(x => x.CreadoEn)
            .HasColumnName("creado_en");

        entity.Property(x => x.ActualizadoEn)
            .HasColumnName("actualizado_en");

        entity.Property(x => x.CreadoPor)
            .HasColumnName("creado_por");

        entity.Property(x => x.ActualizadoPor)
            .HasColumnName("actualizado_por");

        entity.HasOne<Categoria>()
            .WithMany()
            .HasForeignKey(x => x.CategoriaId);

        // El stock y el costo promedio se modifican
        // mediante los procedimientos almacenados de PostgreSQL.
        entity.Property(x => x.Stock)
            .Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);

        entity.Property(x => x.CostoPromedio)
            .Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);
    }

    private static string ATexto(UnidadMedida unidad) =>
        unidad switch
        {
            UnidadMedida.Unidad => "unidad",
            UnidadMedida.Kg => "kg",
            UnidadMedida.Litro => "litro",
            UnidadMedida.Metro => "metro",
            _ => throw new ArgumentOutOfRangeException(nameof(unidad))
        };

    private static UnidadMedida DesdeTexto(string unidad) =>
        unidad switch
        {
            "unidad" => UnidadMedida.Unidad,
            "kg" => UnidadMedida.Kg,
            "litro" => UnidadMedida.Litro,
            "metro" => UnidadMedida.Metro,
            _ => throw new InvalidOperationException(
                $"Unidad no reconocida: {unidad}")
        };
}