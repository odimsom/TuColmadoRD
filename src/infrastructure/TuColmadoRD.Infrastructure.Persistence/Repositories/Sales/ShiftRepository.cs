using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TuColmadoRD.Core.Domain.Entities.Sales;
using TuColmadoRD.Infrastructure.Persistence.Contexts;
using TuColmadoRD.Infrastructure.Persistence.Repositories.Base;
using TuColmadoRD.Core.Domain.Interfaces.Repositories.Base;
using TuColmadoRD.Core.Domain.Interfaces.Repositories.Sales;

namespace TuColmadoRD.Infrastructure.Persistence.Repositories.Sales;

public class ShiftRepository : GenericRepository<Shift>, IShiftRepository
{
    public ShiftRepository(TuColmadoDbContext dbContext) : base(dbContext)
    {
    }
}
