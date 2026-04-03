using System.Diagnostics;
using System.Net.Http;
using System.Reflection;
using System.Text.Json;

namespace TuColmadoRD.Desktop;

internal static class UpdateService
{
    private const string ReleasesUrl = "https://api.github.com/repos/odimsom/TuColmadoRD.Frontend/releases";

    public static async Task<UpdateCheckResult> CheckForUpdateAsync()
    {
        using var http = new HttpClient();
        http.DefaultRequestHeaders.UserAgent.ParseAdd("TuColmadoRD-Desktop-Updater/1.0");

        using var response = await http.GetAsync(ReleasesUrl);
        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync();
        var releases = await JsonSerializer.DeserializeAsync<List<GitHubRelease>>(stream, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        }) ?? new List<GitHubRelease>();

        var latest = releases
            .Where(r => !string.IsNullOrWhiteSpace(r.TagName))
            .Select(r => new
            {
                Release = r,
                Version = ParseVersion(r.TagName!)
            })
            .Where(x => x.Version != null)
            .OrderByDescending(x => x.Version)
            .FirstOrDefault();

        if (latest == null)
        {
            return UpdateCheckResult.NoUpdate;
        }

        var current = GetCurrentVersion();
        if (latest.Version! <= current)
        {
            return UpdateCheckResult.NoUpdate;
        }

        var installerAsset = latest.Release.Assets
            .FirstOrDefault(a => a.BrowserDownloadUrl.EndsWith(".exe", StringComparison.OrdinalIgnoreCase));

        return new UpdateCheckResult
        {
            IsUpdateAvailable = installerAsset != null,
            LatestVersion = latest.Version!.ToString(),
            InstallerUrl = installerAsset?.BrowserDownloadUrl
        };
    }

    public static async Task<string> DownloadInstallerAsync(string installerUrl)
    {
        using var http = new HttpClient();
        http.DefaultRequestHeaders.UserAgent.ParseAdd("TuColmadoRD-Desktop-Updater/1.0");

        var fileName = Path.GetFileName(new Uri(installerUrl).LocalPath);
        if (string.IsNullOrWhiteSpace(fileName))
        {
            fileName = "TuColmadoRD-Setup-latest.exe";
        }

        var targetDir = Path.Combine(Path.GetTempPath(), "TuColmadoRD", "updates");
        Directory.CreateDirectory(targetDir);
        var targetPath = Path.Combine(targetDir, fileName);

        await using var remote = await http.GetStreamAsync(installerUrl);
        await using var local = File.Create(targetPath);
        await remote.CopyToAsync(local);

        return targetPath;
    }

    public static void LaunchInstaller(string installerPath)
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = installerPath,
            UseShellExecute = true,
            Verb = "runas"
        });
    }

    private static Version GetCurrentVersion()
    {
        var infoVersion = Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
        var parsed = ParseVersion(infoVersion);
        if (parsed != null)
        {
            return parsed;
        }

        return Assembly.GetExecutingAssembly().GetName().Version ?? new Version(0, 0, 0, 0);
    }

    private static Version? ParseVersion(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var normalized = value.Trim();
        if (normalized.StartsWith("v", StringComparison.OrdinalIgnoreCase))
        {
            normalized = normalized[1..];
        }

        var dash = normalized.IndexOf('-');
        if (dash >= 0)
        {
            normalized = normalized[..dash];
        }

        return Version.TryParse(normalized, out var version) ? version : null;
    }

    private sealed class GitHubRelease
    {
        public string? TagName { get; set; }
        public List<GitHubAsset> Assets { get; set; } = new();
    }

    private sealed class GitHubAsset
    {
        public string BrowserDownloadUrl { get; set; } = string.Empty;
    }
}

internal sealed class UpdateCheckResult
{
    public static readonly UpdateCheckResult NoUpdate = new()
    {
        IsUpdateAvailable = false
    };

    public bool IsUpdateAvailable { get; init; }
    public string LatestVersion { get; init; } = string.Empty;
    public string? InstallerUrl { get; init; }
}
