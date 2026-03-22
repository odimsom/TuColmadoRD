using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TuColmadoRD.Core.Domain.Entities.Treasury;
using TuColmadoRD.Infrastructure.Persistence.Contexts;
using TuColmadoRD.Infrastructure.Persistence.Repositories.Base;
using TuColmadoRD.Core.Domain.Interfaces.Repositories.Base;
using TuColmadoRD.Core.Domain.Interfaces.Repositories.Treasury;

namespace TuColmadoRD.Infrastructure.Persistence.Repositories.Treasury;

public class PettyCashRepository : GenericRepository<PettyCash>, IPettyCashRepository
{
    public PettyCashRepository(TuColmadoDbContext dbContext) : base(dbContext)
    {
    }
}
