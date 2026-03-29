using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

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

app.MapReverseProxy();

app.Run();
