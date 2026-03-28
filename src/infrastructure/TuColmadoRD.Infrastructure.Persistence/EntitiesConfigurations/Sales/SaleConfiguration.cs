using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TuColmadoRD.Core.Domain.Entities.Sales;

namespace TuColmadoRD.Infrastructure.Persistence.EntitiesConfigurations.Sales;

public class SaleConfiguration : IEntityTypeConfiguration<Sale>
{
    public void Configure(EntityTypeBuilder<Sale> builder)
    {
        builder.ToTable("Sales");
        builder.HasKey(s => s.Id);

        builder.OwnsOne(s => s.TenantId, b => 
        {
            b.Property(t => t.Value).HasColumnName("TenantId").IsRequired();
        });

        builder.OwnsOne(s => s.SubTotal, b => 
        {
            b.Property(m => m.Amount).HasColumnName("SubTotal").HasColumnType("decimal(18,2)").IsRequired();
        });

        builder.OwnsOne(s => s.TaxTotal, b => 
        {
            b.Property(m => m.Amount).HasColumnName("TaxTotal").HasColumnType("decimal(18,2)").IsRequired();
        });

        builder.OwnsOne(s => s.Total, b => 
        {
            b.Property(m => m.Amount).HasColumnName("Total").HasColumnType("decimal(18,2)").IsRequired();
        });

        builder.Property(s => s.Status).IsRequired().HasConversion<string>();
        builder.Property(s => s.PaymentMethod).IsRequired().HasConversion<string>();

        builder.HasOne<TuColmadoRD.Core.Domain.Entities.Customers.Customer>()
            .WithMany()
            .HasForeignKey(s => s.CustomerId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(s => s.Details)
            .WithOne()
            .HasForeignKey(d => d.SaleId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
