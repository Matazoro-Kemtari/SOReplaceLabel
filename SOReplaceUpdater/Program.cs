using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace SOReplaceUpdater
{
    class Program
    {
        static async Task Main(string[] args)
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

            Console.WriteLine($"Starting update for SOReplaceLabel...");
            Console.WriteLine($"Download URL: {downloadUrl}");
            Console.WriteLine($"Install Dir: {installDir}");
            Console.WriteLine($"Parent PID: {parentPid}");

            try
            {
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

                string tempRoot = Path.Combine(Path.GetTempPath(), "SOReplaceLabel_Update");
                string zipPath = Path.Combine(tempRoot, "update.zip");
                string extractDir = Path.Combine(tempRoot, "extract");
                string backupDir = Path.Combine(tempRoot, "backup");

                if (Directory.Exists(tempRoot)) Directory.Delete(tempRoot, true);
                Directory.CreateDirectory(tempRoot);
                Directory.CreateDirectory(extractDir);

                // 2. 最新版をダウンロード
                Console.WriteLine("Downloading update...");
                using (var client = new HttpClient())
                {
                    var response = await client.GetAsync(downloadUrl);
                    response.EnsureSuccessStatusCode();
                    using (var fs = new FileStream(zipPath, FileMode.Create))
                    {
                        await response.Content.CopyToAsync(fs);
                    }
                }

                // 3. 展開
                Console.WriteLine("Extracting update...");
                ZipFile.ExtractToDirectory(zipPath, extractDir);

                // 4. バックアップの作成
                Console.WriteLine("Creating backup...");
                if (Directory.Exists(backupDir)) Directory.Delete(backupDir, true);
                Directory.CreateDirectory(backupDir);
                CopyAll(installDir, backupDir);

                try
                {
                    // 5. 置換（上書き）
                    Console.WriteLine("Installing update...");
                    // 自分自身(updater.exe)を除いてコピー
                    CopyAll(extractDir, installDir, excludePatterns: new[] { "SOReplaceUpdater.exe" });
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Update failed: {ex.Message}");
                    Console.WriteLine("Rolling back...");
                    CopyAll(backupDir, installDir);
                    throw;
                }

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
            catch (Exception ex)
            {
                Console.WriteLine("ERROR: " + ex.ToString());
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
            }
        }

        static void CopyAll(string source, string target, string[] excludePatterns = null)
        {
            foreach (var dir in Directory.GetDirectories(source, "*", SearchOption.AllDirectories))
            {
                Directory.CreateDirectory(dir.Replace(source, target));
            }

            foreach (var file in Directory.GetFiles(source, "*.*", SearchOption.AllDirectories))
            {
                string fileName = Path.GetFileName(file);
                if (excludePatterns != null && excludePatterns.Any(p => string.Equals(fileName, p, StringComparison.OrdinalIgnoreCase)))
                {
                    continue;
                }

                var dest = file.Replace(source, target);
                Retry(() => File.Copy(file, dest, true), 5, 500);
            }
        }

        static void Retry(Action action, int retry, int delayMs)
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
}
