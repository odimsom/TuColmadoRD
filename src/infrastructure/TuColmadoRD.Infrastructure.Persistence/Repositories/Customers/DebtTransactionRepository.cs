using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TuColmadoRD.Core.Domain.Entities.Customers;
using TuColmadoRD.Infrastructure.Persistence.Contexts;
using TuColmadoRD.Infrastructure.Persistence.Repositories.Base;
using TuColmadoRD.Core.Domain.Interfaces.Repositories.Base;
using TuColmadoRD.Core.Domain.Interfaces.Repositories.Customers;

namespace TuColmadoRD.Infrastructure.Persistence.Repositories.Customers;

public class DebtTransactionRepository : GenericRepository<DebtTransaction>, IDebtTransactionRepository
{
    public DebtTransactionRepository(TuColmadoDbContext dbContext) : base(dbContext)
    {
    }
}
