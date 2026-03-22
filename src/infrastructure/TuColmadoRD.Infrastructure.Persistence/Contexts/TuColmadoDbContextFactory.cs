using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace TuColmadoRD.Infrastructure.Persistence.Contexts;

public class TuColmadoDbContextFactory : IDesignTimeDbContextFactory<TuColmadoDbContext>
{
    public TuColmadoDbContext CreateDbContext(string[] args)
    {
        var builder = new DbContextOptionsBuilder<TuColmadoDbContext>();
        var connectionString = "Host=localhost;Database=TuColmadoDb;Username=devdb;Password=1011";

        builder.UseNpgsql(connectionString, b => b.MigrationsAssembly(typeof(TuColmadoDbContextFactory).Assembly.FullName));

        return new TuColmadoDbContext(builder.Options);
    }
}
