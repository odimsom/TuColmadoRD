using Microsoft.EntityFrameworkCore;
using TuColmadoRD.Core.Domain.Entities.Inventory;
using TuColmadoRD.Core.Domain.Interfaces.Repositories.Inventory;
using TuColmadoRD.Infrastructure.Persistence.Contexts;
using TuColmadoRD.Infrastructure.Persistence.Repositories.Base;

namespace TuColmadoRD.Infrastructure.Persistence.Repositories.Inventory;

public class ProductRepository(TuColmadoDbContext dbContext) : GenericRepository<Product>(dbContext), IProductRepository
{
    public async Task<Product?> GetByBarcodeAsync(string barcode, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Set<Product>().FirstOrDefaultAsync(p => p.Barcode == barcode, cancellationToken);
    }
}
