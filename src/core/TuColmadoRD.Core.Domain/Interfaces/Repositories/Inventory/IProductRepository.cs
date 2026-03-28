using System.Threading;
using System.Threading.Tasks;
using TuColmadoRD.Core.Domain.Entities.Inventory;
using TuColmadoRD.Core.Domain.Interfaces.Repositories.Base;

namespace TuColmadoRD.Core.Domain.Interfaces.Repositories.Inventory;

public interface IProductRepository : IGenericRepository<Product>
{
    Task<Product?> GetByBarcodeAsync(string barcode, CancellationToken cancellationToken = default);
}
