using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TuColmadoRD.Core.Domain.Entities.Inventory;

namespace TuColmadoRD.Infrastructure.Persistence.EntitiesConfigurations.Inventory;

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("Categories");
        builder.HasKey(c => c.Id);

        builder.OwnsOne(c => c.TenantId, b => 
        {
            b.Property(t => t.Value).HasColumnName("TenantId").IsRequired();
        });

        builder.Property(c => c.Name).IsRequired().HasMaxLength(100);
        builder.Property(c => c.Description).HasMaxLength(500);
        builder.Property(c => c.IconPath).HasMaxLength(250);
        builder.Property(c => c.ColorHex).HasMaxLength(10);
    }
}
