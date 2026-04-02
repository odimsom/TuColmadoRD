using System.Text;
using TuColmadoRD.ApiGateway.Middlewares;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMemoryCache();
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

builder.Services.AddSwaggerGen();

var app = builder.Build();

app.MapGet("/", async (HttpContext ctx) =>
{
    var header = ctx.Request.Headers.Authorization.ToString();
    if (!header.StartsWith("Basic "))
    {
        ctx.Response.Headers.WWWAuthenticate = "Basic realm=\"TuColmadoRD API\"";
        ctx.Response.StatusCode = 401;
        return;
    }

    var credentials = Encoding.UTF8.GetString(Convert.FromBase64String(header["Basic ".Length..]));
    if (credentials != "admin:ArroZZ12hju.,")
    {
        ctx.Response.Headers.WWWAuthenticate = "Basic realm=\"TuColmadoRD API\"";
        ctx.Response.StatusCode = 401;
        return;
    }

    var html = await File.ReadAllTextAsync(Path.Combine(app.Environment.WebRootPath, "index.html"));
    ctx.Response.ContentType = "text/html";
    await ctx.Response.WriteAsync(html);
});

app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/api-swagger/v1/swagger.json", ".NET API (TuColmadoRD)");
    c.SwaggerEndpoint("/auth-swagger/swagger.json", "Node.js Auth API");
    c.RoutePrefix = "swagger";
});

app.UseMiddleware<IdempotencyMiddleware>();
