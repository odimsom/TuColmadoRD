using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TuColmadoRD.Core.Domain.Entities.Fiscal;

namespace TuColmadoRD.Infrastructure.Persistence.EntitiesConfigurations.Fiscal;

public class TaxConfiguration : IEntityTypeConfiguration<Tax>
{
    public void Configure(EntityTypeBuilder<Tax> builder)
    {
        builder.ToTable("Taxes");
        builder.HasKey(t => t.Id);

        builder.OwnsOne(t => t.TenantId, b => 
        {
            b.Property(t => t.Value).HasColumnName("TenantId").IsRequired();
        });

        builder.Property(t => t.Name).IsRequired().HasMaxLength(50);
        builder.Property(t => t.Type).IsRequired().HasConversion<string>();

        builder.OwnsOne(t => t.Rate, b => 
        {
            b.Property(r => r.Percentage).HasColumnName("RatePercentage").HasColumnType("decimal(5,2)").IsRequired();
            b.Property(r => r.Name).HasColumnName("RateName").HasMaxLength(20).IsRequired();
        });
    }
}
