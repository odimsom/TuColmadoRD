using System.Net.Sockets;
using System.IO;
using Microsoft.AspNetCore.Builder;
using TuColmadoRD.ApiGateway;
using TuColmadoRD.Presentation.API;

namespace TuColmadoRD.Desktop;

static class Program
{
    [STAThread]
    static void Main(string[] args)
    {
        Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
        Application.ThreadException += (_, e) => AppLogger.Error("Unhandled UI exception", e.Exception);
        AppDomain.CurrentDomain.UnhandledException += (_, e) =>
        {
            if (e.ExceptionObject is Exception exception)
            {
                AppLogger.Error("Unhandled domain exception", exception);
            }
        };

        ApplicationConfiguration.Initialize();
        AppLogger.Info("Desktop startup begin");

        using var splash = new SplashForm();
        splash.Show();
        Application.DoEvents();

        // 1. Start Core API on 5200
        splash.SetStatus("Iniciando API local...");
        var coreApp = CoreApiHostBuilder.BuildCoreApi(args, isLocal: true);
        _ = Task.Run(() => coreApp.RunAsync("http://localhost:5200"));

        // 2. Start Auth Mock on 5300
        splash.SetStatus("Iniciando autenticacion local...");
        var authApp = AuthLocalHostBuilder.BuildAuthLocal(args);
        _ = Task.Run(() => authApp.RunAsync("http://localhost:5300"));

        // 3. Start Gateway on 5100
        splash.SetStatus("Iniciando gateway local...");
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

        // 4. Wait for Gateway to be ready (keep execution on STA thread)
        splash.SetStatus("Verificando puertos locales...");
        WaitForPort(5200, 30000).GetAwaiter().GetResult();
        WaitForPort(5100, 30000).GetAwaiter().GetResult();

        // 5. Run WinForms
        var hasIdentity = HasDeviceIdentity();
        var startUrl = "http://localhost:5100/auth/login";

        var mainForm = new MainForm(startUrl, openWebViewOnStart: !hasIdentity);
        mainForm.Ready += (_, _) =>
        {
            AppLogger.Info("Main form ready");
            splash.Close();
        };
        Application.Run(mainForm);
    }

    private static bool HasDeviceIdentity()
    {
        var candidatePaths = new[]
        {
            Path.Combine(AppContext.BaseDirectory, "device_identity.dat"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "TuColmadoRD", "device_identity.dat")
        };

        return candidatePaths.Any(File.Exists);
    }

    private static async Task WaitForPort(int port, int timeoutMs = 10000)
    {
        var startTime = DateTime.Now;
        while ((DateTime.Now - startTime).TotalMilliseconds < timeoutMs)
        {
            try
            {
                using var client = new TcpClient();
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
