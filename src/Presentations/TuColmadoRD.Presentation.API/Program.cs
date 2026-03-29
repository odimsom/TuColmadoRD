using TuColmadoRD.Infrastructure.CrossCutting.Security;
using TuColmadoRD.Infrastructure.IOC.ServiceRegistrations;
using TuColmadoRD.Presentation.API.Endpoints.Inventory;

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

app.Run();
