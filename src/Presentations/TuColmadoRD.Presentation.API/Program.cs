using TuColmadoRD.Infrastructure.CrossCutting.Security;
using TuColmadoRD.Infrastructure.IOC.ServiceRegistrations;
using TuColmadoRD.Presentation.API.Endpoints.Customers;
using TuColmadoRD.Presentation.API.Endpoints.Expenses;
using TuColmadoRD.Presentation.API.Endpoints.Inventory;
using TuColmadoRD.Presentation.API.Endpoints.Sales;
using TuColmadoRD.Presentation.API.Endpoints.Sales.Shifts;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddAuthorization();

// Services registrations
builder.Services.AddGlobalServices(builder.Configuration);

var app = builder.Build();

app.UseSwagger();
if (app.Environment.IsDevelopment())
{
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseMiddleware<SubscriptionGuardMiddleware>();

app.MapInventoryEndpoints();
app.MapCustomerEndpoints();
app.MapExpenseEndpoints();
app.MapSalesEndpoints();
app.MapShiftEndpoints();

app.Run();
