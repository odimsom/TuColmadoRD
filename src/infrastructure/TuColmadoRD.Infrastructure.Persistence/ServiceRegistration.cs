using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TuColmadoRD.Core.Domain.Interfaces.Repositories.Base;
using TuColmadoRD.Core.Domain.Interfaces.Repositories.Audit;
using TuColmadoRD.Core.Domain.Interfaces.Repositories.Customers;
using TuColmadoRD.Core.Domain.Interfaces.Repositories.Fiscal;
using TuColmadoRD.Core.Domain.Interfaces.Repositories.HumanResources;
using TuColmadoRD.Core.Domain.Interfaces.Repositories.Inventory;
using TuColmadoRD.Core.Domain.Interfaces.Repositories.Logistics;
using TuColmadoRD.Core.Domain.Interfaces.Repositories.Purchasing;
using TuColmadoRD.Core.Domain.Interfaces.Repositories.Sales;
using TuColmadoRD.Core.Domain.Interfaces.Repositories.Treasury;
using TuColmadoRD.Infrastructure.Persistence.Contexts;
using TuColmadoRD.Infrastructure.Persistence.Repositories.Base;
using TuColmadoRD.Infrastructure.Persistence.Repositories.Audit;
using TuColmadoRD.Infrastructure.Persistence.Repositories.Customers;
using TuColmadoRD.Infrastructure.Persistence.Repositories.Fiscal;
using TuColmadoRD.Infrastructure.Persistence.Repositories.HumanResources;
using TuColmadoRD.Infrastructure.Persistence.Repositories.Inventory;
using TuColmadoRD.Infrastructure.Persistence.Repositories.Logistics;
using TuColmadoRD.Infrastructure.Persistence.Repositories.Purchasing;
using TuColmadoRD.Infrastructure.Persistence.Repositories.Sales;
using TuColmadoRD.Infrastructure.Persistence.Repositories.Treasury;

namespace TuColmadoRD.Infrastructure.Persistence;

public static class ServiceRegistration
{
    public static IServiceCollection AddPersistenceInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<TuColmadoDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection") ?? throw new System.InvalidOperationException("Connection string 'DefaultConnection' not found."),
                b => b.MigrationsAssembly(typeof(TuColmadoDbContext).Assembly.FullName)));

        services.AddTransient(typeof(IGenericRepository<>), typeof(GenericRepository<>));

        // Audit
        services.AddScoped<IAuditTrailRepository, AuditTrailRepository>();

        // Customers
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<ICustomerAccountRepository, CustomerAccountRepository>();
        services.AddScoped<IDebtTransactionRepository, DebtTransactionRepository>();

        // Fiscal
        services.AddScoped<IFiscalReceiptRepository, FiscalReceiptRepository>();
        services.AddScoped<IFiscalSequenceRepository, FiscalSequenceRepository>();
        services.AddScoped<ITaxRepository, TaxRepository>();

        // HumanResources
        services.AddScoped<IEmployeeRepository, EmployeeRepository>();
        services.AddScoped<IWorkShiftRepository, WorkShiftRepository>();

        // Inventory
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IUnitConversionRepository, UnitConversionRepository>();

        // Logistics
        services.AddScoped<IDeliveryOrderRepository, DeliveryOrderRepository>();
        services.AddScoped<IDeliveryPersonRepository, DeliveryPersonRepository>();

        // Purchasing
        services.AddScoped<IPurchaseDetailRepository, PurchaseDetailRepository>();
        services.AddScoped<IPurchaseOrderRepository, PurchaseOrderRepository>();
        services.AddScoped<ISupplierRepository, SupplierRepository>();

        // Sales
        services.AddScoped<ISaleRepository, SaleRepository>();
        services.AddScoped<ISaleDetailRepository, SaleDetailRepository>();
        services.AddScoped<IShiftRepository, ShiftRepository>();

        // Treasury
        services.AddScoped<ICashBoxRepository, CashBoxRepository>();
        services.AddScoped<ICashDrawerRepository, CashDrawerRepository>();
        services.AddScoped<IExpenseRepository, ExpenseRepository>();
        services.AddScoped<IPettyCashRepository, PettyCashRepository>();

        return services;
    }
}
