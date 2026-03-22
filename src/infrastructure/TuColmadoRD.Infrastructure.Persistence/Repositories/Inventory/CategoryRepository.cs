using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TuColmadoRD.Core.Domain.Entities.Inventory;
using TuColmadoRD.Infrastructure.Persistence.Contexts;
using TuColmadoRD.Infrastructure.Persistence.Repositories.Base;
using TuColmadoRD.Core.Domain.Interfaces.Repositories.Base;
using TuColmadoRD.Core.Domain.Interfaces.Repositories.Inventory;

namespace TuColmadoRD.Infrastructure.Persistence.Repositories.Inventory;

public class CategoryRepository : GenericRepository<Category>, ICategoryRepository
{
    public CategoryRepository(TuColmadoDbContext dbContext) : base(dbContext)
    {
    }
}
