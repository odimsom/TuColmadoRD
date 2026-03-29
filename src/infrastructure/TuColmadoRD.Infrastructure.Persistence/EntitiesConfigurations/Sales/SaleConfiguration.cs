using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TuColmadoRD.Core.Domain.Entities.Sales;
using TuColmadoRD.Core.Domain.ValueObjects;

namespace TuColmadoRD.Infrastructure.Persistence.EntitiesConfigurations.Sales;

public class SaleConfiguration : IEntityTypeConfiguration<Sale>
{
    public void Configure(EntityTypeBuilder<Sale> builder)
    {
        builder.ToTable("Sales");
        builder.HasKey(s => s.Id);

        builder.Property(s => s.ShiftId).IsRequired();
        builder.Property(s => s.TerminalId).IsRequired();
        builder.Property(s => s.CreatedAt).IsRequired();

        builder.OwnsOne(s => s.TenantId, b => 
        {
            b.Property(t => t.Value).HasColumnName("TenantId").IsRequired();
        });

        builder.Property(s => s.Total)
            .HasConversion(v => v.Amount, v => Money.FromDecimal(v).Result)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(s => s.Status).IsRequired().HasConversion<int>();

    }
}
