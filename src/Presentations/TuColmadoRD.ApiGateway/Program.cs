using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using TuColmadoRD.ApiGateway.Middlewares;

namespace TuColmadoRD.ApiGateway;

public class Program
{
    public static void Main(string[] args)
    {
        // Entry point for standalone running
        if (args.Any(a => a.Contains("TuColmadoRD.ApiGateway.dll")) || args.Length == 0)
        {
            GatewayHostBuilder.BuildGateway(args).Run();
        }
    }
}

public class GatewayOptions
{
    public bool IsLocalMode { get; set; }
    public string AuthApiUrl { get; set; } = "http://localhost:3000";
    public string CoreApiUrl { get; set; } = "http://localhost:5000";
    public string JwtSecret { get; set; } = "dominican-street-premium-secret-key-2026";
    public string[] AllowedOrigins { get; set; } = Array.Empty<string>();
}

public static class GatewayHostBuilder
{
    public static WebApplication BuildGateway(string[] args, GatewayOptions? options = null)
    {
        var builder = WebApplication.CreateBuilder(args);

        var configOptions = builder.Configuration.GetSection("GatewayOptions").Get<GatewayOptions>() ?? new GatewayOptions();
        var authApiUrl = options?.AuthApiUrl ?? configOptions.AuthApiUrl;
        var coreApiUrl = options?.CoreApiUrl ?? configOptions.CoreApiUrl;
        var jwtSecret = options?.JwtSecret ?? configOptions.JwtSecret;
        var allowedOrigins = options?.AllowedOrigins ?? configOptions.AllowedOrigins;

        builder.Services.AddMemoryCache();
        builder.Services.AddHttpClient("AuthClient", client => client.BaseAddress = new Uri(authApiUrl));
        builder.Services.AddHttpClient("CoreClient", client => client.BaseAddress = new Uri(coreApiUrl));

        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(opt =>
            {
                opt.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret))
                };
            });

        builder.Services.AddAuthorization();

        builder.Services.AddCors(opt =>
        {
            opt.AddDefaultPolicy(policy =>
            {
                policy.WithOrigins(allowedOrigins)
                      .AllowAnyHeader()
                      .AllowAnyMethod();
            });
        });

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        var app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseCors();
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseMiddleware<IdempotencyMiddleware>();

        var authGroup = app.MapGroup("/gateway/auth");

        authGroup.MapPost("/register", async (HttpContext ctx, IHttpClientFactory factory) =>
            await ProxyRequest(ctx, factory.CreateClient("AuthClient"), "/auth/register"));

        authGroup.MapPost("/login", async (HttpContext ctx, IHttpClientFactory factory) =>
            await ProxyRequest(ctx, factory.CreateClient("AuthClient"), "/auth/login"));

        app.MapPost("/gateway/devices/pair", async (HttpContext ctx, IHttpClientFactory factory) =>
            await ProxyRequest(ctx, factory.CreateClient("AuthClient"), "/pair-device"))
            .RequireAuthorization();

        app.Map("/gateway/{**path}", async (string path, HttpContext ctx, IHttpClientFactory factory) =>
        {
            return await ProxyRequest(ctx, factory.CreateClient("CoreClient"), $"/{path}");
        }).RequireAuthorization();

        return app;
    }

    private static async Task<IResult> ProxyRequest(HttpContext ctx, HttpClient client, string targetPath)
    {
        var request = ctx.Request;
        var targetUri = new Uri(client.BaseAddress!, targetPath + request.QueryString);

        var proxyRequest = new HttpRequestMessage(new HttpMethod(request.Method), targetUri);

        if (request.ContentLength > 0 || request.HasJsonContentType())
        {
            proxyRequest.Content = new StreamContent(request.Body);
            if (request.ContentType != null)
            {
                proxyRequest.Content.Headers.ContentType = System.Net.Http.Headers.MediaTypeHeaderValue.Parse(request.ContentType);
            }
        }

        foreach (var header in request.Headers)
        {
            if (header.Key.Equals("Host", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (!proxyRequest.Headers.TryAddWithoutValidation(header.Key, (IEnumerable<string?>)header.Value) && proxyRequest.Content != null)
            {
                proxyRequest.Content.Headers.TryAddWithoutValidation(header.Key, (IEnumerable<string?>)header.Value);
            }
        }

        try
        {
            var response = await client.SendAsync(proxyRequest, HttpCompletionOption.ResponseHeadersRead);

            ctx.Response.StatusCode = (int)response.StatusCode;
            return Microsoft.AspNetCore.Http.Results.Stream(
                await response.Content.ReadAsStreamAsync(),
                response.Content.Headers.ContentType?.ToString() ?? "application/json"
            );
        }
        catch (Exception ex)
        {
            return Microsoft.AspNetCore.Http.Results.Json(new { message = ex.Message }, statusCode: 500);
        }
    }
}
