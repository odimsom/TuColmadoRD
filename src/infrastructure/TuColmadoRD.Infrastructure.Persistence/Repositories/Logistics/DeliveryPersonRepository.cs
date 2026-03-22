using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TuColmadoRD.Core.Domain.Entities.Logistics;
using TuColmadoRD.Infrastructure.Persistence.Contexts;
using TuColmadoRD.Infrastructure.Persistence.Repositories.Base;
using TuColmadoRD.Core.Domain.Interfaces.Repositories.Base;
using TuColmadoRD.Core.Domain.Interfaces.Repositories.Logistics;

namespace TuColmadoRD.Infrastructure.Persistence.Repositories.Logistics;

public class DeliveryPersonRepository : GenericRepository<DeliveryPerson>, IDeliveryPersonRepository
{
    public DeliveryPersonRepository(TuColmadoDbContext dbContext) : base(dbContext)
    {
    }
}
