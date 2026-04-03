using TuColmadoRD.Infrastructure.CrossCutting.Security;
using TuColmadoRD.Infrastructure.IOC.ServiceRegistrations;
using TuColmadoRD.Presentation.API.Endpoints.Customers;
using TuColmadoRD.Presentation.API.Endpoints.Expenses;
using TuColmadoRD.Presentation.API.Endpoints.Inventory;
using TuColmadoRD.Presentation.API.Endpoints.Sales;
using TuColmadoRD.Presentation.API.Endpoints.Sales.Shifts;
using TuColmadoRD.Presentation.API.Endpoints.Purchasing;

namespace TuColmadoRD.Presentation.API;

public static class CoreApiHostBuilder
{
    public static WebApplication BuildCoreApi(string[] args, bool isLocal = false)
    {
        var builder = WebApplication.CreateBuilder(args);

        if (isLocal)
        {
            builder.Configuration["GatewayOptions:IsLocalMode"] = "true";
        }

        builder.Configuration
            .AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true);

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        builder.Services.AddAuthorization();

        // Services registrations
        builder.Services.AddGlobalServices(builder.Configuration);
        builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(CoreApiHostBuilder).Assembly));

        var app = builder.Build();

        app.UseSwagger();
        if (app.Environment.IsDevelopment())
        {
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();
        app.UseMiddleware<SubscriptionGuardMiddleware>();

        app.MapInventoryEndpoints();
        app.MapPurchasingEndpoints();
        app.MapCustomerEndpoints();
        app.MapExpenseEndpoints();
        app.MapSalesEndpoints();
        app.MapShiftEndpoints();

        return app;
    }
}

// Entry point for standalone running
if (Environment.GetCommandLineArgs().Any(a => a.Contains("TuColmadoRD.Presentation.API.dll")))
{
    CoreApiHostBuilder.BuildCoreApi(Environment.GetCommandLineArgs()).Run();
}
