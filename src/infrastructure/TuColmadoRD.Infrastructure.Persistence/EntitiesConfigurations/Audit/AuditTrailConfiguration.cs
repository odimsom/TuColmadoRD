using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TuColmadoRD.Core.Domain.Entities.Audit;

namespace TuColmadoRD.Infrastructure.Persistence.EntitiesConfigurations.Audit;

public class AuditTrailConfiguration : IEntityTypeConfiguration<AuditTrail>
{
    public void Configure(EntityTypeBuilder<AuditTrail> builder)
    {
        builder.ToTable("AuditTrails");
        // No ID is defined in AuditTrail.cs in the provided code, assuming it has none or it's a stub right now.
        // It's empty. Let's configure it as no key or a basic table for now. 
        builder.HasNoKey(); 
    }
}
