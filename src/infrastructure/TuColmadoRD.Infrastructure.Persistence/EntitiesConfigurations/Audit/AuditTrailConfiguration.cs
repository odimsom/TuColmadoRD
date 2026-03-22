using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TuColmadoRD.Core.Domain.Entities.Audit;

namespace TuColmadoRD.Infrastructure.Persistence.EntitiesConfigurations.Audit;

public class AuditTrailConfiguration : IEntityTypeConfiguration<AuditTrail>
{
    public void Configure(EntityTypeBuilder<AuditTrail> builder)
    {
        builder.ToTable("AuditTrails");
        builder.HasNoKey(); 
    }
}
