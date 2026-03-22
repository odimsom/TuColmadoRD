using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TuColmadoRD.Core.Domain.Entities.Fiscal;
using TuColmadoRD.Infrastructure.Persistence.Contexts;
using TuColmadoRD.Infrastructure.Persistence.Repositories.Base;
using TuColmadoRD.Core.Domain.Interfaces.Repositories.Base;
using TuColmadoRD.Core.Domain.Interfaces.Repositories.Fiscal;

namespace TuColmadoRD.Infrastructure.Persistence.Repositories.Fiscal;

public class TaxRepository : GenericRepository<Tax>, ITaxRepository
{
    public TaxRepository(TuColmadoDbContext dbContext) : base(dbContext)
    {
    }
}
