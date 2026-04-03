using TuColmadoRD.Infrastructure.Hosts;

namespace TuColmadoRD.Presentation.API;

public class Program
{
    public static void Main(string[] args)
    {
        // Entry point for standalone running
        if (args.Any(a => a.Contains("TuColmadoRD.Presentation.API.dll")) || args.Length == 0)
        {
            CoreApiHostBuilder.BuildCoreApi(args).Run();
        }
    }
}
