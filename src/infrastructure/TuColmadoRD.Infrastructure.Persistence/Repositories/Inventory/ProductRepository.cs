using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TuColmadoRD.Core.Domain.Entities.Inventory;
using TuColmadoRD.Infrastructure.Persistence.Contexts;
using TuColmadoRD.Infrastructure.Persistence.Repositories.Base;
using TuColmadoRD.Core.Domain.Interfaces.Repositories.Base;
using TuColmadoRD.Core.Domain.Interfaces.Repositories.Inventory;

namespace TuColmadoRD.Infrastructure.Persistence.Repositories.Inventory;

public class ProductRepository : GenericRepository<Product>, IProductRepository
{
    public ProductRepository(TuColmadoDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<Product?> GetByBarcodeAsync(string barcode, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Products.FirstOrDefaultAsync(p => p.Barcode == barcode, cancellationToken);
    }
}
