using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TuColmadoRD.Core.Domain.Entities.Purchasing;

namespace TuColmadoRD.Infrastructure.Persistence.EntitiesConfigurations.Purchasing;

public class PurchaseDetailConfiguration : IEntityTypeConfiguration<PurchaseDetail>
{
    public void Configure(EntityTypeBuilder<PurchaseDetail> builder)
    {
        builder.ToTable("PurchaseDetails");
        builder.HasKey(pd => pd.Id);

        builder.OwnsOne(pd => pd.Quantity, b => 
        {
            b.Property(q => q.Value).HasColumnName("Quantity").HasColumnType("decimal(18,4)").IsRequired();
            b.Property(q => q.UnitLabel).HasColumnName("UnitLabel").HasMaxLength(20).IsRequired();
            b.Property(q => q.Type).HasColumnName("UnitType").IsRequired();
        });

        builder.OwnsOne(pd => pd.UnitCost, b => 
        {
            b.Property(m => m.Amount).HasColumnName("UnitCost").HasColumnType("decimal(18,2)").IsRequired();
        });

        builder.OwnsOne(pd => pd.SubTotal, b => 
        {
            b.Property(m => m.Amount).HasColumnName("SubTotal").HasColumnType("decimal(18,2)").IsRequired();
        });

        builder.HasOne<TuColmadoRD.Core.Domain.Entities.Inventory.Product>()
            .WithMany()
            .HasForeignKey(pd => pd.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
