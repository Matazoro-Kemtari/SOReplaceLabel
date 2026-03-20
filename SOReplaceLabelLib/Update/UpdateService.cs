using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace SOReplaceLabelLib.Update
{
    public class GitHubRelease
    {
        public string tag_name { get; set; }
        public List<Asset> assets { get; set; }

        public class Asset
        {
            public string name { get; set; }
            public string browser_download_url { get; set; }
        }
    }

    public class UpdateService
    {
        private readonly string _owner;
        private readonly string _repo;

        public UpdateService(string owner, string repo)
        {
            _owner = owner;
            _repo = repo;
        }

        public async Task<GitHubRelease> GetLatestReleaseAsync()
        {
            using (var client = new HttpClient())
            {
                // GitHub API requires a User-Agent
                client.DefaultRequestHeaders.Add("User-Agent", "SOReplaceLabel-Updater");

                var url = $"https://api.github.com/repos/{_owner}/{_repo}/releases/latest";
                try
                {
                    var response = await client.GetAsync(url).ConfigureAwait(false);
                    if (!response.IsSuccessStatusCode) return null;

                    var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    return JsonSerializer.Deserialize<GitHubRelease>(json);
                }
                catch
                {
                    return null;
                }
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
}
