using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TuColmadoRD.Core.Domain.Entities.Sales;

namespace TuColmadoRD.Infrastructure.Persistence.EntitiesConfigurations.Sales;

public class SaleDetailConfiguration : IEntityTypeConfiguration<SaleDetail>
{
    public void Configure(EntityTypeBuilder<SaleDetail> builder)
    {
        builder.ToTable("SaleDetails");
        builder.HasKey(sd => sd.Id);

        builder.OwnsOne(sd => sd.Quantity, b => 
        {
            b.Property(q => q.Value).HasColumnName("Quantity").HasColumnType("decimal(18,4)").IsRequired();
            b.Property(q => q.UnitLabel).HasColumnName("UnitLabel").HasMaxLength(20).IsRequired();
            b.Property(q => q.Type).HasColumnName("UnitType").IsRequired();
        });

        builder.OwnsOne(sd => sd.UnitPrice, b => 
        {
            b.Property(m => m.Amount).HasColumnName("UnitPrice").HasColumnType("decimal(18,2)").IsRequired();
        });

        builder.OwnsOne(sd => sd.TaxAmount, b => 
        {
            b.Property(m => m.Amount).HasColumnName("TaxAmount").HasColumnType("decimal(18,2)").IsRequired();
        });

        builder.OwnsOne(sd => sd.SubTotal, b => 
        {
            b.Property(m => m.Amount).HasColumnName("SubTotal").HasColumnType("decimal(18,2)").IsRequired();
        });

        builder.HasOne<TuColmadoRD.Core.Domain.Entities.Inventory.Product>()
            .WithMany()
            .HasForeignKey(sd => sd.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
