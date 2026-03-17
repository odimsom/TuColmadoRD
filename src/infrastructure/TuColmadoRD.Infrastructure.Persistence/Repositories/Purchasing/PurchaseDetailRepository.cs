using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TuColmadoRD.Core.Domain.Entities.Purchasing;
using TuColmadoRD.Infrastructure.Persistence.Contexts;
using TuColmadoRD.Infrastructure.Persistence.Repositories.Base;
using TuColmadoRD.Core.Domain.Interfaces.Repositories.Base;
using TuColmadoRD.Core.Domain.Interfaces.Repositories.Purchasing;

namespace TuColmadoRD.Infrastructure.Persistence.Repositories.Purchasing;

public class PurchaseDetailRepository : GenericRepository<PurchaseDetail>, IPurchaseDetailRepository
{
    public PurchaseDetailRepository(TuColmadoDbContext dbContext) : base(dbContext)
    {
    }
}
