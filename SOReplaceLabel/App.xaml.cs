using System.Diagnostics;
using System.Windows;
using SOReplaceLabel.Views;
using Wada.SOReplaceLabel.Application.LabelPrinter;
using Wada.SOReplaceLabel.Domain;
using Wada.SOReplaceLabel.Infrastructure.StarIO;
using Wada.SOReplaceLabel.Wpf;

namespace SOReplaceLabel;

/// <summary>
/// App.xaml の相互作用ロジック
/// </summary>
public partial class App
{
    protected override Window CreateShell()
    {
        // アップデートチェック (バックグラウンドで開始)
        _ = CheckForUpdatesAsync();

        return Container.Resolve<MainWindow>();
    }

    protected override void RegisterTypes(IContainerRegistry containerRegistry)
    {
        _ = containerRegistry.RegisterScoped<ILabelPrinter, LabelPrinter>();
        _ = containerRegistry.RegisterScoped<IPrintLabelUseCase, PrintLabelUseCase>();
    }

    protected override void ConfigureModuleCatalog(IModuleCatalog moduleCatalog)
    {
        base.ConfigureModuleCatalog(moduleCatalog);

        // ModuleCatalog にモジュールを追加
        moduleCatalog.AddModule<SOReplaceLabelModule>(InitializationMode.WhenAvailable);
    }

    private async Task CheckForUpdatesAsync()
    {
        try
        {
            var assembly = System.Reflection.Assembly.GetExecutingAssembly();
            var currentVersion = assembly.GetName().Version.ToString();

            var updateService = new Wada.SOReplaceLabel.Application.Update.UpdateService(SOReplaceLabel.Properties.Settings.Default.UpdateVersionUrl);

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
