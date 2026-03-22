using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TuColmadoRD.Core.Domain.Entities.Purchasing;

namespace TuColmadoRD.Infrastructure.Persistence.EntitiesConfigurations.Purchasing;

public class SupplierConfiguration : IEntityTypeConfiguration<Supplier>
{
    public void Configure(EntityTypeBuilder<Supplier> builder)
    {
        builder.ToTable("Suppliers");
        builder.HasKey(s => s.Id);

        builder.OwnsOne(s => s.TenantId, b => 
        {
            b.Property(t => t.Value).HasColumnName("TenantId").IsRequired();
        });

        builder.Property(s => s.BusinessName).IsRequired().HasMaxLength(150);

        builder.OwnsOne(s => s.TaxId, b => 
        {
            b.Property(r => r.Value).HasColumnName("TaxId").HasMaxLength(15);
        });

        builder.Property(s => s.Type).IsRequired().HasConversion<string>();

        builder.OwnsOne(s => s.ContactPhone, b => 
        {
            b.Property(p => p.Value).HasColumnName("ContactPhone").HasMaxLength(20);
        });
    }
}
