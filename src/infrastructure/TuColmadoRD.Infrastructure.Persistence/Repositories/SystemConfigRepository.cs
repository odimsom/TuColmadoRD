using Microsoft.EntityFrameworkCore;
using TuColmadoRD.Core.Domain.Interfaces.Repositories.Security;
using TuColmadoRD.Core.Domain.Entities.System;
using TuColmadoRD.Infrastructure.Persistence.Contexts;

namespace TuColmadoRD.Infrastructure.Persistence.Repositories;

public class SystemConfigRepository : ISystemConfigRepository
{
    private const string LktKey = "security.last_known_time";
    private readonly TuColmadoDbContext _dbContext;

    public SystemConfigRepository(TuColmadoDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<string?> GetLastKnownTimeAsync()
    {
        var config = await _dbContext.SystemConfigs
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Key == LktKey);

        return config?.Value;
    }

    public async Task UpdateLastKnownTimeAsync(string newTime)
    {
        using var transaction = await _dbContext.Database.BeginTransactionAsync();
        try
        {
            var config = await _dbContext.SystemConfigs.FirstOrDefaultAsync(c => c.Key == LktKey);
            if (config == null)
            {
                config = new SystemConfig(LktKey, newTime);
                _dbContext.SystemConfigs.Add(config);
            }
            else
            {
                config.UpdateValue(newTime);
            }

            await _dbContext.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}
