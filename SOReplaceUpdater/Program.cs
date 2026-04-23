using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace SOReplaceUpdater;

public class Program
{
    public static async Task Main(string[] args)
    {
        if (args.Length < 3)
        {
            Console.WriteLine("Usage: SOReplaceUpdater.exe <downloadUrl> <installDir> <parentPid>");
            Console.WriteLine("Please press any key.");
            Console.ReadKey();
            return;
        }

        string downloadUrl = args[0];
        string installDir = args[1];
        if (!int.TryParse(args[2], out int parentPid))
        {
            Console.WriteLine("Invalid parent PID.");
            return;
        }

        var manager = new UpdateManager();
        try
        {
            await manager.RunUpdateAsync(downloadUrl, installDir, parentPid);
        }
        catch (Exception ex)
        {
            Console.WriteLine("ERROR: " + ex.ToString());
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
}

internal class UpdateManager(HttpClient? httpClient = null, string? tempRoot = null)
{
    private readonly HttpClient _httpClient = httpClient ?? new HttpClient();
    private readonly string _tempRoot = tempRoot ?? Path.Combine(Path.GetTempPath(), "SOReplaceLabel_Update");

    internal async Task RunUpdateAsync(string downloadUrl, string installDir, int parentPid, bool restart = true)
    {
        Console.WriteLine($"Starting update for SOReplaceLabel...");
        Console.WriteLine($"Download URL: {downloadUrl}");
        Console.WriteLine($"Install Dir: {installDir}");
        Console.WriteLine($"Parent PID: {parentPid}");

        // 1. 親プロセス（本体アプリ）の終了を待つ
        try
        {
            var parent = Process.GetProcessById(parentPid);
            Console.WriteLine("Waiting for parent process to exit...");
            parent.WaitForExit();
        }
        catch (ArgumentException)
        {
            // すでに終了している場合はここに来る
        }

        string zipPath = Path.Combine(_tempRoot, "update.zip");
        string extractDir = Path.Combine(_tempRoot, "extract");
        string backupDir = Path.Combine(_tempRoot, "backup");

        if (Directory.Exists(_tempRoot)) Directory.Delete(_tempRoot, true);
        Directory.CreateDirectory(_tempRoot);
        Directory.CreateDirectory(extractDir);

        // 2. 最新版をダウンロード
        Console.WriteLine("Downloading update...");
        var response = await _httpClient.GetAsync(downloadUrl);
        response.EnsureSuccessStatusCode();
        using (var fs = new FileStream(zipPath, FileMode.Create))
        {
            await response.Content.CopyToAsync(fs);
        }

        // 3. 展開
        Console.WriteLine("Extracting update...");
        ZipFile.ExtractToDirectory(zipPath, extractDir);

        // 4. バックアップの作成
        Console.WriteLine("Creating backup...");
        if (Directory.Exists(backupDir)) Directory.Delete(backupDir, true);
        Directory.CreateDirectory(backupDir);
        // 既存のバックアップフォルダとアップデーター自体は除外する
        CopyAll(installDir, backupDir, excludePatterns: ["backup", "SOReplaceUpdater.exe"]);

        try
        {
            // 5. 置換（上書き）
            Console.WriteLine("Installing update...");
            // 自分自身(updater.exe)を除いてコピー
            CopyAll(extractDir, installDir, excludePatterns: ["SOReplaceUpdater.exe"]);

            // 5.5 1世代残すためにインストールディレクトリにバックアップを配置
            Console.WriteLine("Preserving previous version...");
            string persistentBackupDir = Path.Combine(installDir, "backup");
            if (Directory.Exists(persistentBackupDir)) Directory.Delete(persistentBackupDir, true);
            Directory.CreateDirectory(persistentBackupDir);
            CopyAll(backupDir, persistentBackupDir);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Update failed: {ex.Message}");
            Console.WriteLine("Rolling back...");
            CopyAll(backupDir, installDir);
            throw;
        }

        if (restart)
        {
            // 6. 再起動
            Console.WriteLine("Update complete. Restarting application...");
            var exe = Directory.GetFiles(installDir, "SOReplaceLabel.exe").FirstOrDefault();
            if (exe != null)
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = exe,
                    WorkingDirectory = installDir
                });
            }
        }
    }

    internal void CopyAll(string source, string target, string[]? excludePatterns = null)
    {
        if (!Directory.Exists(target))
        {
            Directory.CreateDirectory(target);
        }

        foreach (var file in Directory.GetFiles(source))
        {
            string name = Path.GetFileName(file);
            if (excludePatterns != null && excludePatterns.Any(p => string.Equals(name, p, StringComparison.OrdinalIgnoreCase)))
            {
                continue;
            }

            var dest = Path.Combine(target, name);
            Retry(() => File.Copy(file, dest, true), 5, 500);
        }

        foreach (var dir in Directory.GetDirectories(source))
        {
            string name = Path.GetFileName(dir);
            if (excludePatterns != null && excludePatterns.Any(p => string.Equals(name, p, StringComparison.OrdinalIgnoreCase)))
            {
                continue;
            }

            var dest = Path.Combine(target, name);
            CopyAll(dir, dest, excludePatterns);
        }
    }

    private void Retry(Action action, int retry, int delayMs)
    {
        for (int i = 0; i < retry; i++)
        {
            try
            {
                action();
                return;
            }
            catch (IOException)
            {
                if (i == retry - 1) throw;
                Thread.Sleep(delayMs);
            }
        }
    }
}
