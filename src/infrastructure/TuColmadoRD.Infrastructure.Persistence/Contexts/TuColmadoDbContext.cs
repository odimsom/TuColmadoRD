using System.Reflection;
using Microsoft.EntityFrameworkCore;
using TuColmadoRD.Core.Domain.Entities.Audit;
using TuColmadoRD.Core.Domain.Entities.Customers;
using TuColmadoRD.Core.Domain.Entities.Fiscal;
using TuColmadoRD.Core.Domain.Entities.HumanResources;
using TuColmadoRD.Core.Domain.Entities.Inventory;
using TuColmadoRD.Core.Domain.Entities.Logistics;
using TuColmadoRD.Core.Domain.Entities.Purchasing;
using TuColmadoRD.Core.Domain.Entities.Sales;
using TuColmadoRD.Core.Domain.Entities.Treasury;

namespace TuColmadoRD.Infrastructure.Persistence.Contexts;

public class TuColmadoDbContext : DbContext
{
    public TuColmadoDbContext(DbContextOptions<TuColmadoDbContext> options) : base(options)
    {
    }

    // Audit
    public DbSet<AuditTrail> AuditTrails => Set<AuditTrail>();

    // Customers
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<CustomerAccount> CustomerAccounts => Set<CustomerAccount>();
    public DbSet<DebtTransaction> DebtTransactions => Set<DebtTransaction>();

    // Fiscal
    public DbSet<FiscalReceipt> FiscalReceipts => Set<FiscalReceipt>();
    public DbSet<FiscalSequence> FiscalSequences => Set<FiscalSequence>();
    public DbSet<Tax> Taxes => Set<Tax>();

    // HumanResources
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<WorkShift> WorkShifts => Set<WorkShift>();

    // Inventory
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<UnitConversion> UnitConversions => Set<UnitConversion>();
    public DbSet<UnitOfMeasureEntity> UnitOfMeasures => Set<UnitOfMeasureEntity>();

    // Logistics
    public DbSet<DeliveryOrder> DeliveryOrders => Set<DeliveryOrder>();
    public DbSet<DeliveryPerson> DeliveryPersons => Set<DeliveryPerson>();

    // Purchasing
    public DbSet<PurchaseDetail> PurchaseDetails => Set<PurchaseDetail>();
    public DbSet<PurchaseOrder> PurchaseOrders => Set<PurchaseOrder>();
    public DbSet<Supplier> Suppliers => Set<Supplier>();

    // Sales
    public DbSet<Sale> Sales => Set<Sale>();
    public DbSet<SaleDetail> SaleDetails => Set<SaleDetail>();
    public DbSet<Shift> Shifts => Set<Shift>();

    // Treasury
    public DbSet<CashBox> CashBoxes => Set<CashBox>();
    public DbSet<CashDrawer> CashDrawers => Set<CashDrawer>();
    public DbSet<Expense> Expenses => Set<Expense>();
    public DbSet<PettyCash> PettyCashes => Set<PettyCash>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // This line applies all IEntityTypeConfigurations found in the current assembly.
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
