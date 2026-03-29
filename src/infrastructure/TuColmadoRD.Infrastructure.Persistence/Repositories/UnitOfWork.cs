using TuColmadoRD.Core.Application.Inventory.Abstractions;
using TuColmadoRD.Infrastructure.Persistence.Contexts;

namespace TuColmadoRD.Infrastructure.Persistence.Repositories;

/// <summary>
/// EF Core unit of work implementation.
/// </summary>
public sealed class UnitOfWork : IUnitOfWork
{
    private readonly TuColmadoDbContext _dbContext;

    public UnitOfWork(TuColmadoDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task CommitAsync(CancellationToken ct)
    {
        await _dbContext.SaveChangesAsync(ct);
    }
}
