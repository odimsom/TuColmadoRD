using TuColmadoRD.Infrastructure.Hosts;

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
