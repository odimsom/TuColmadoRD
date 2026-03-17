using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TuColmadoRD.Core.Domain.Entities.Inventory;

namespace TuColmadoRD.Infrastructure.Persistence.EntitiesConfigurations.Inventory;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Name).IsRequired().HasMaxLength(150);
        builder.Property(p => p.ShortName).IsRequired().HasMaxLength(50);
        builder.Property(p => p.Barcode).HasMaxLength(50);
        builder.Property(p => p.ImageUrl).HasMaxLength(500);

        builder.OwnsOne(p => p.TenantId, b => 
        {
            b.Property(t => t.Value).HasColumnName("TenantId").IsRequired();
        });

        builder.OwnsOne(p => p.StockQuantity, b => 
        {
            b.Property(q => q.Value).HasColumnName("StockQuantity").HasColumnType("decimal(18,4)").IsRequired();
            b.Property(q => q.UnitLabel).HasColumnName("StockUnitLabel").HasMaxLength(20).IsRequired();
            b.Property(q => q.Type).HasColumnName("StockUnitType").IsRequired();
        });

        builder.OwnsOne(p => p.MinStock, b => 
        {
            b.Property(q => q.Value).HasColumnName("MinStockQuantity").HasColumnType("decimal(18,4)").IsRequired();
            b.Property(q => q.UnitLabel).HasColumnName("MinStockUnitLabel").HasMaxLength(20).IsRequired();
            b.Property(q => q.Type).HasColumnName("MinStockUnitType").IsRequired();
        });

        builder.OwnsOne(p => p.CostPrice, b => 
        {
            b.Property(m => m.Amount).HasColumnName("CostPrice").HasColumnType("decimal(18,2)").IsRequired();
        });

        builder.OwnsOne(p => p.SalePrice, b => 
        {
            b.Property(m => m.Amount).HasColumnName("SalePrice").HasColumnType("decimal(18,2)").IsRequired();
        });

        builder.OwnsOne(p => p.ItbisRate, b => 
        {
            b.Property(t => t.Percentage).HasColumnName("ItbisPercentage").HasColumnType("decimal(5,2)").IsRequired();
            b.Property(t => t.Name).HasColumnName("ItbisName").HasMaxLength(20).IsRequired();
        });

        builder.OwnsOne(p => p.IscRate, b => 
        {
            b.Property(t => t.Percentage).HasColumnName("IscPercentage").HasColumnType("decimal(5,2)");
            b.Property(t => t.Name).HasColumnName("IscName").HasMaxLength(20);
        });
    }
}
