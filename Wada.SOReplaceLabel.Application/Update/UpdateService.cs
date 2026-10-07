using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SOReplaceLabelLib.Update;

// 汎用的なバージョン情報クラス
public class VersionInfo
{
    [JsonPropertyName("latestVersion")]
    public string LatestVersion { get; set; }
    
    [JsonPropertyName("downloadUrl")]
    public string DownloadUrl { get; set; }
}

public class UpdateService(string versionUrl)
{
    public async Task<VersionInfo> GetLatestVersionAsync()
    {
        using var client = new HttpClient();
        
        client.DefaultRequestHeaders.Add("User-Agent", "SOReplaceLabel-Updater");

        try
        {
            var response = await client.GetAsync(versionUrl).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode) return null;

            var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            return JsonSerializer.Deserialize<VersionInfo>(json);
        }
        catch
        {
            return null;    // 何か問題だとしてもバージョン確認なのでスルーする
        }
    }

    public bool IsNewerVersion(string currentVersion, string latestTag)
    {
        if (string.IsNullOrEmpty(latestTag)) return false;

        // Remove 'v' prefix if present
        var latest = latestTag.TrimStart('v');

        if (Version.TryParse(currentVersion, out var v1) && Version.TryParse(latest, out var v2))
        {
            return v2 > v1;
        }
        return false;
    }

    public void LaunchUpdater(string downloadUrl, string installDir)
    {
        string updaterPath = Path.Combine(installDir, "SOReplaceUpdater.exe");
        if (!File.Exists(updaterPath))
        {
            throw new FileNotFoundException("Updater executable not found.", updaterPath);
        }

        int currentPid = Process.GetCurrentProcess().Id;

        var psi = new ProcessStartInfo
        {
            FileName = updaterPath,
            Arguments = $"\"{downloadUrl}\" \"{installDir.TrimEnd('\\')}\" \"{currentPid}\"",
            UseShellExecute = false,
            CreateNoWindow = false
        };

        Process.Start(psi);
    }
}
