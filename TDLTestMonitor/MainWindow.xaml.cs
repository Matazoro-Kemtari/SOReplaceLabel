using System.Formats.Asn1;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Windows;
using Microsoft.Web.WebView2.Core;

namespace TDLTestMonitor;
/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private const string ActiveXShimScript = @"
(function() {
    if (window.__activeXShimInstalled) return;
    window.__activeXShimInstalled = true;

    window.ActiveXObject = function(progId) {
        if (progId !== 'Scripting.FileSystemObject') {
            throw new Error('ActiveXObject not supported in this environment: ' + progId);
        }
        return {
            DeleteFile: function(path, force) {
                // 何もしない（元々ファイル削除→再作成の準備動作なので不要）
            },
            OpenTextFile: function(path, mode, create, format) {
                var buffer = '';
                return {
                    Write: function(text) { buffer += text; },
                    WriteLine: function(text) { buffer += text + '\n'; },
                    Close: function() {
                        // ここでC#側にデータを送信する
                        window.chrome.webview.postMessage(
                            JSON.stringify({ path: path, content: buffer })
                        );
                    }
                };
            }
        };
    };
})();
";
    
    public MainWindow()
    {
        InitializeComponent();
        InitializeWebView();
    }

    private async void InitializeWebView()
    {
        var userDataFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "LabelPrintApp", "WebView2Data");

        var env = await CoreWebView2Environment.CreateAsync(userDataFolder: userDataFolder);
        await webView.EnsureCoreWebView2Async(env);

        // 全フレーム・全ナビゲーションに対して document 作成時に自動注入
        await webView.CoreWebView2.AddScriptToExecuteOnDocumentCreatedAsync(ActiveXShimScript);

        webView.CoreWebView2.WebMessageReceived += OnWebMessageReceived;

        webView.CoreWebView2.Navigate(@"https://ghsam095mgs.mhi.co.jp/KIT/Transit/Transit_PT.asp");
        // 一時的にDevToolsを有効化
        webView.CoreWebView2.Settings.AreDevToolsEnabled = true;
    }

    private void OnWebMessageReceived(object? sender, CoreWebView2WebMessageReceivedEventArgs e)
    {
        try
        {
            using var fs = new FileStream(@"C:\temp\DebugLog.txt", FileMode.Create);
            using var writer = new StreamWriter(fs, Encoding.UTF8);

            var json = e.TryGetWebMessageAsString();
            if (string.IsNullOrEmpty(json))
            {
                writer.WriteLine("WebMessage受信: 空メッセージ");
                return;
            }
            var payload = JsonSerializer.Deserialize<ShopOrderPayload>(json);
            if (payload == null || string.IsNullOrEmpty(payload.content))
            {
                writer.WriteLine("WebMessage受信: contentなし");
                return;
            }

            // payload.path -> "C:\\temp\\ShopOrder.txt" （ファイル自体は作られないがログ用に有用）
            // payload.content -> タブ区切りの本文（既存の data.txt と同一フォーマット）
            // まずは受信の有無だけ確認
            writer.WriteLine(payload.content);
            writer.Flush();

            ProcessAndPrint(payload.content);

        }
        catch (Exception)
        {
            
            throw;
        }   
    }

    private void ProcessAndPrint(string content)
    {
        var fields = content.Split('\t');

        // 既存のパース・印刷ロジックをそのまま呼び出す
        //PrintLabel(fields);
    }

    class ShopOrderPayload
    {
        public string path { get; set; }
        public string content { get; set; }
    }
}