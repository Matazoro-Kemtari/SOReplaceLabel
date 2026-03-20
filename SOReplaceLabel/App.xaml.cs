using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace SOReplaceLabel
{
    /// <summary>
    /// App.xaml の相互作用ロジック
    /// </summary>
    public partial class App : Application
    {
        private async void Application_Startup(object sender, StartupEventArgs e)
        {
            WpfMvvm.DispatcherHelper.Initialize();

            // アップデートチェック (バックグラウンドで開始)
            _ = CheckForUpdatesAsync();
        }

        private async Task CheckForUpdatesAsync()
        {
            try
            {
                var assembly = System.Reflection.Assembly.GetExecutingAssembly();
                var currentVersion = assembly.GetName().Version.ToString();

                var updateService = new SOReplaceLabelLib.Update.UpdateService("Matazoro-Kemtari", "SOReplaceLabel");

                var release = await updateService.GetLatestReleaseAsync().ConfigureAwait(false);
                if (release != null && updateService.IsNewerVersion(currentVersion, release.tag_name))
                {
                    var asset = release.assets.FirstOrDefault(a => a.name.EndsWith(".zip", StringComparison.OrdinalIgnoreCase));
                    if (asset != null)
                    {
                        // UI スレッドでメッセージボックスを表示
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            var result = MessageBox.Show(
                                $"新しいバージョン ({release.tag_name}) が見つかりました。アップデートを適用しますか？\n\n※アップデート中はアプリが一時的に終了します。",
                                "アップデートのお知らせ",
                                MessageBoxButton.YesNo,
                                MessageBoxImage.Information);

                            if (result == MessageBoxResult.Yes)
                            {
                                updateService.LaunchUpdater(asset.browser_download_url, AppDomain.CurrentDomain.BaseDirectory);
                                Application.Current.Shutdown();
                            }
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                // アップデートチェックの失敗は実行を妨げない
                Debug.WriteLine("Update check failed: " + ex.Message);
            }
        }
    }
}
