using System.Windows;
using System.Windows.Controls;
using Wada.SOReplaceLabel.Wpf.ViewModels;

namespace Wada.SOReplaceLabel.Wpf.Views;
/// <summary>
/// Interaction logic for PrismUserControl1
/// </summary>
public partial class WebViewPage : UserControl
{
    public WebViewPage()
    {
        InitializeComponent();

        this.Loaded += OnLoaded;
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (DataContext is WebViewPageViewModel vm)
        {
            // WebView2 実体を使ってサービスを作成し、VM に注入
            var service = new WebView2Service(webView);
            // VM の private field にセットするか、Factory パターンを使う
            //vm.SetWebViewService(service);
            await vm.InitializeAsync(service);
        }
    }
}
