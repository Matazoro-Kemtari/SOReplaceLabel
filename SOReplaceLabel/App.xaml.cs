using System;
using System.Diagnostics;
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

                var updateService = new SOReplaceLabelLib.Update.UpdateService(SOReplaceLabel.Properties.Settings.Default.UpdateVersionUrl);

                var info = await updateService.GetLatestVersionAsync().ConfigureAwait(false);
                if (info != null && updateService.IsNewerVersion(currentVersion, info.LatestVersion))
                {
                    // UI スレッドでメッセージボックスを表示
                    Current.Dispatcher.Invoke(() =>
                    {
                        var result = MessageBox.Show(
                            $"新しいバージョン ({info.LatestVersion}) が見つかりました。アップデートを適用しますか？\n\n※アップデート中はアプリが一時的に終了します。",
                            "アップデートのお知らせ",
                            MessageBoxButton.YesNo,
                            MessageBoxImage.Information);

                        if (result == MessageBoxResult.Yes)
                        {
                            updateService.LaunchUpdater(info.DownloadUrl, AppDomain.CurrentDomain.BaseDirectory);
                            Current.Shutdown();
                        }
                    });
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
