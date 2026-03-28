using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TuColmadoRD.Core.Application.Interfaces.Infrastructure.CrossCutting.Network;
using TuColmadoRD.Infrastructure.CrossCutting.Configuration;
using TuColmadoRD.Infrastructure.CrossCutting.Network;

namespace TuColmadoRD.Infrastructure.CrossCutting;

public static class ServiceRegistration
{
    public static IServiceCollection AddCrossCuttingInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<ConnectionMonitorOptions>(
            opts => configuration.GetSection(ConnectionMonitorOptions.SectionName).Bind(opts));

        services.AddSingleton<IConnectionMonitor, ConnectionMonitor>();
        services.AddHostedService<ConnectionMonitorHostedService>();

        return services;
    }
}