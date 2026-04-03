using System.Net.Sockets;
using TuColmadoRD.ApiGateway;
using TuColmadoRD.Presentation.API;

namespace TuColmadoRD.Desktop;

static class Program
{
    [STAThread]
    static async Task Main(string[] args)
    {
        ApplicationConfiguration.Initialize();

        // 1. Start Core API on 5200
        var coreApp = CoreApiHostBuilder.BuildCoreApi(args, isLocal: true);
        _ = Task.Run(() => coreApp.RunAsync("http://localhost:5200"));

        // 2. Start Auth Mock on 5300
        var authApp = AuthLocalHostBuilder.BuildAuthLocal(args);
        _ = Task.Run(() => authApp.RunAsync("http://localhost:5300"));

        // 3. Start Gateway on 5100
        var gatewayApp = GatewayHostBuilder.BuildGateway(args, new GatewayOptions
        {
            IsLocalMode = true,
            AuthApiUrl = "http://localhost:5300",
            CoreApiUrl = "http://localhost:5200",
            AllowedOrigins = new[] { "http://localhost:5100" }
        });
        
        // Serve static files (Angular build in wwwroot)
        gatewayApp.UseStaticFiles();
        // spa should be handled by UseStaticFiles if it's index.html, 
        // but we can add fallback for Angular routing
        gatewayApp.MapFallbackToFile("index.html");

        _ = Task.Run(() => gatewayApp.RunAsync("http://localhost:5100"));

        // 4. Wait for Gateway to be ready
        await WaitForPort(5100);

        // 5. Run WinForms
        var mainForm = new MainForm("http://localhost:5100/portal/dashboard");
        Application.Run(mainForm);
    }

    private static async Task WaitForPort(int port, int timeoutMs = 10000)
    {
        using var client = new TcpClient();
        var startTime = DateTime.Now;
        while ((DateTime.Now - startTime).TotalMilliseconds < timeoutMs)
        {
            try
            {
                await client.ConnectAsync("localhost", port);
                return;
            }
            catch
            {
                await Task.Delay(500);
            }
        }
    }
}
