using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TuColmadoRD.Infrastructure.Persistence.Contexts;
using TuColmadoRD.Core.Domain.Interfaces.Repositories.Base;

namespace TuColmadoRD.Infrastructure.Persistence.Repositories.Base;

public class GenericRepository<T> : IGenericRepository<T> where T : class
{
    protected readonly TuColmadoDbContext _dbContext;

    public GenericRepository(TuColmadoDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public virtual async Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Set<T>().FindAsync(new object[] { id }, cancellationToken);
    }

    public virtual async Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Set<T>().ToListAsync(cancellationToken);
    }

    public virtual async Task<T> AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        await _dbContext.Set<T>().AddAsync(entity, cancellationToken);
        return entity;
    }

    public virtual void Update(T entity)
    {
        _dbContext.Set<T>().Update(entity);
    }

    public virtual void Delete(T entity)
    {
        _dbContext.Set<T>().Remove(entity);
    }
}
