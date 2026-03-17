using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace TuColmadoRD.Infrastructure.Persistence.Contexts;

public class TuColmadoDbContextFactory : IDesignTimeDbContextFactory<TuColmadoDbContext>
{
    public TuColmadoDbContext CreateDbContext(string[] args)
    {
        var builder = new DbContextOptionsBuilder<TuColmadoDbContext>();
        // Hardcode a local design-time connection string.
        // This is only used when running "dotnet ef migrations add" or "database update"
        // At runtime, the application will use the connection string from appsettings.json
        var connectionString = "Host=localhost;Database=TuColmadoDb;Username=devdb;Password=1011";

        builder.UseNpgsql(connectionString, b => b.MigrationsAssembly(typeof(TuColmadoDbContextFactory).Assembly.FullName));

        return new TuColmadoDbContext(builder.Options);
    }
}
