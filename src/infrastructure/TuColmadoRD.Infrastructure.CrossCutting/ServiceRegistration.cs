using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TuColmadoRD.Core.Application.Interfaces.Infrastructure.CrossCutting.Network;
using TuColmadoRD.Core.Application.Interfaces.Tenancy;
using TuColmadoRD.Infrastructure.CrossCutting.Configuration;
using TuColmadoRD.Infrastructure.CrossCutting.Network;
using TuColmadoRD.Infrastructure.CrossCutting.Security;
using TuColmadoRD.Infrastructure.CrossCutting.Tenancy;

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

        services.AddScoped<TuColmadoRD.Core.Application.Interfaces.Security.ITimeGuard, TimeGuardService>();
        services.AddScoped<TuColmadoRD.Core.Application.Interfaces.Security.ILicenseVerifier, LicenseVerifierService>();

        var applicationAssembly = typeof(TuColmadoRD.Core.Application.Behaviors.ICommandMarker).Assembly;
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(applicationAssembly);
            cfg.AddBehavior(typeof(MediatR.IPipelineBehavior<,>), typeof(TuColmadoRD.Core.Application.Behaviors.ClockAdvancePipelineBehavior<,>));
        });

        return services;
    }

    public static IServiceCollection AddCloudTenancy(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<ITenantProvider, JwtTenantProvider>();
        return services;
    }

    public static IServiceCollection AddLocalTenancy(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<LocalDeviceOptions>(configuration.GetSection("LocalDevice"));
        services.AddSingleton<LocalDeviceTenantProvider>();
        services.AddSingleton<ITenantProvider>(sp => sp.GetRequiredService<LocalDeviceTenantProvider>());
        services.AddSingleton<IDeviceIdentityStore>(sp => sp.GetRequiredService<LocalDeviceTenantProvider>());
        services.AddHttpClient<DevicePairingService>(client =>
        {
            var authBaseUrl = configuration["AuthApi:BaseUrl"]
                ?? throw new InvalidOperationException("AuthApi:BaseUrl is not configured.");
            client.BaseAddress = new Uri(authBaseUrl);
        });
        services.AddScoped<IDevicePairingService, DevicePairingService>();
        return services;
    }
}