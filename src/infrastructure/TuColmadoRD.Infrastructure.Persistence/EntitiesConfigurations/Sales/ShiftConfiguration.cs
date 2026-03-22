using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TuColmadoRD.Core.Domain.Entities.Sales;

namespace TuColmadoRD.Infrastructure.Persistence.EntitiesConfigurations.Sales;

public class ShiftConfiguration : IEntityTypeConfiguration<Shift>
{
    public void Configure(EntityTypeBuilder<Shift> builder)
    {
        builder.ToTable("Shifts");
        builder.HasKey(s => s.Id);

        builder.OwnsOne(s => s.TenantId, b => 
        {
            b.Property(t => t.Value).HasColumnName("TenantId").IsRequired();
        });

        builder.OwnsOne(s => s.InitialCash, b => 
        {
            b.Property(m => m.Amount).HasColumnName("InitialCash").HasColumnType("decimal(18,2)").IsRequired();
        });

        builder.OwnsOne(s => s.ActualCashAtClose, b => 
        {
            b.Property(m => m.Amount).HasColumnName("ActualCashAtClose").HasColumnType("decimal(18,2)");
        });

        builder.OwnsOne(s => s.TotalCashSales, b => 
        {
            b.Property(m => m.Amount).HasColumnName("TotalCashSales").HasColumnType("decimal(18,2)").IsRequired();
        });

        builder.OwnsOne(s => s.TotalCreditSales, b => 
        {
            b.Property(m => m.Amount).HasColumnName("TotalCreditSales").HasColumnType("decimal(18,2)").IsRequired();
        });

        builder.OwnsOne(s => s.TotalCardSales, b => 
        {
            b.Property(m => m.Amount).HasColumnName("TotalCardSales").HasColumnType("decimal(18,2)").IsRequired();
        });

        builder.OwnsOne(s => s.TotalTransferSales, b => 
        {
            b.Property(m => m.Amount).HasColumnName("TotalTransferSales").HasColumnType("decimal(18,2)").IsRequired();
        });

        builder.Property(s => s.Status).IsRequired().HasConversion<string>();

        builder.HasOne<TuColmadoRD.Core.Domain.Entities.HumanResources.Employee>()
            .WithMany()
            .HasForeignKey(s => s.CashierId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
