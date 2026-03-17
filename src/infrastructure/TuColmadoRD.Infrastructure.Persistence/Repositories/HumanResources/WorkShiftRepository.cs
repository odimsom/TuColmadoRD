using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TuColmadoRD.Core.Domain.Entities.HumanResources;
using TuColmadoRD.Infrastructure.Persistence.Contexts;
using TuColmadoRD.Infrastructure.Persistence.Repositories.Base;
using TuColmadoRD.Core.Domain.Interfaces.Repositories.Base;
using TuColmadoRD.Core.Domain.Interfaces.Repositories.HumanResources;

namespace TuColmadoRD.Infrastructure.Persistence.Repositories.HumanResources;

public class WorkShiftRepository : GenericRepository<WorkShift>, IWorkShiftRepository
{
    public WorkShiftRepository(TuColmadoDbContext dbContext) : base(dbContext)
    {
    }
}
