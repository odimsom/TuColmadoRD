using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TuColmadoRD.Core.Domain.Entities.Customers;
using TuColmadoRD.Infrastructure.Persistence.Contexts;
using TuColmadoRD.Infrastructure.Persistence.Repositories.Base;
using TuColmadoRD.Core.Domain.Interfaces.Repositories.Base;
using TuColmadoRD.Core.Domain.Interfaces.Repositories.Customers;

namespace TuColmadoRD.Infrastructure.Persistence.Repositories.Customers;

public class CustomerRepository : GenericRepository<Customer>, ICustomerRepository
{
    public CustomerRepository(TuColmadoDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<Customer?> GetByDocumentIdAsync(string documentId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Customers.FirstOrDefaultAsync(c => c.DocumentId.Value == documentId, cancellationToken);
    }
}
