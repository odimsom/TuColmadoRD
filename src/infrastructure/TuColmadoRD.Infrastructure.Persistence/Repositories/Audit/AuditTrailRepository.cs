using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TuColmadoRD.Core.Domain.Entities.Audit;
using TuColmadoRD.Infrastructure.Persistence.Contexts;
using TuColmadoRD.Infrastructure.Persistence.Repositories.Base;
using TuColmadoRD.Core.Domain.Interfaces.Repositories.Base;
using TuColmadoRD.Core.Domain.Interfaces.Repositories.Audit;

namespace TuColmadoRD.Infrastructure.Persistence.Repositories.Audit;

public class AuditTrailRepository : GenericRepository<AuditTrail>, IAuditTrailRepository
{
    public AuditTrailRepository(TuColmadoDbContext dbContext) : base(dbContext)
    {
    }
}
