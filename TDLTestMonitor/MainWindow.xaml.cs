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
    if (window.__activeXShimInstalled)
    {
        console.log('Return immediately.', 'window.__activeXShimInstalled:', window.__activeXShimInstalled);
        return;
    }
    window.__activeXShimInstalled = true;

    const isMainFrame = window === window.top;
    const location = window.location.href;
    console.log(
        'ActiveXShimScript installed.',
        'isMainFrame:', isMainFrame,
        'location:', location);

    window.ActiveXObject = function(progId) {
        if (progId !== 'Scripting.FileSystemObject') {
            throw new Error('ActiveXObject not supported in this environment: ' + progId);
        }
        return {
            DeleteFile: function(path, force) {
                console.log('DeleteFile called (ignored):', path, force);
                // 何もしない（元々ファイル削除→再作成の準備動作なので不要）
            },
            OpenTextFile: function(path, mode, create, format) {
                console.log('OpenTextFile called:', path, mode, create, format);
                var buffer = '';
                return {
                    Write: function(text) {
                        console.log('Write called:', text);
                        buffer += text;
                    },
                    WriteLine: function(text) {
                        console.log('WriteLine called:', text);
                        buffer += text + '\n';
                    },
                    Close: function() {
                        console.log('Close called, buffer length:', buffer.length);
                        // ここでC#側にデータを送信する
                        try {
                            window.chrome.webview.postMessage(
                                JSON.stringify({ path: path, content: buffer })
                            );
                            console.log('postMessage called successfully.');
                        } catch (e) {
                            console.error('postMessage failed:', e);
                        }
                    }
                };
            }
        };
    };
})();
";
    
    public MainWindow()
    {
        Log("MainWindow MainWindow constructor fired.");
        InitializeComponent();
        Log("InitializeWebViewAsync completed in constructor.");
        InitializeWebView();
        Log("WebView Source set.");
    }

    private async void InitializeWebView()
    {
        Log("InitializeWebViewAsync started.");

        try
        {
            var userDataFolder = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "LabelPrintApp", "WebView2Data");

            Log("Calling EnsureCoreWebView2Async...");
            var env = await CoreWebView2Environment.CreateAsync(userDataFolder: userDataFolder);
            Log("EnsureCoreWebView2Async completed.");
            await webView.EnsureCoreWebView2Async(env);

            // 全フレーム・全ナビゲーションに対して document 作成時に自動注入
            await webView.CoreWebView2.AddScriptToExecuteOnDocumentCreatedAsync(ActiveXShimScript);
            Log("AddScriptToExecuteOnDocumentCreatedAsync completed.");

            // デバッグ用: ドキュメント作成時にホストへ到達確認用の ping メッセージを送るスクリプトを注入
            var diagnosticPingScript = @"(function(){
    try {
        if (window.__webview_ping_sent) return;
        window.__webview_ping_sent = true;
        if (window.chrome && window.chrome.webview && window.chrome.webview.postMessage) {
            window.chrome.webview.postMessage(JSON.stringify({ __ping__: true, url: location.href }));
            console.log('webview ping posted:', location.href);
        } else {
            console.log('webview ping: chrome.webview not available yet');
        }
    } catch(e) { console.error('webview ping error', e); }
})();";
            await webView.CoreWebView2.AddScriptToExecuteOnDocumentCreatedAsync(diagnosticPingScript);
            Log("Diagnostic ping script injected.");

            webView.CoreWebView2.WebMessageReceived += OnWebMessageReceived;
            // JavaScript の console 出力を受け取ってホストへ送る（デバッグ用）
            var consoleOverrideScript = @"(function(){
    try {
        if (window.__console_overridden) return;
        window.__console_overridden = true;
        function sendConsole(level, args){
            try {
                var msg = '';
                for (var i=0;i<args.length;i++){
                    try { msg += (typeof args[i] === 'object' ? JSON.stringify(args[i]) : String(args[i])) + ' '; } catch(e){ msg += String(args[i]) + ' '; }
                }
                if (window.chrome && window.chrome.webview && window.chrome.webview.postMessage) {
                    window.chrome.webview.postMessage(JSON.stringify({ __console__: true, level: level, message: msg.trim(), url: location.href }));
                }
            } catch(e) { }
        }
        var orig = { log: console.log, debug: console.debug, info: console.info, warn: console.warn, error: console.error };
        console.log = function(){ sendConsole('log', arguments); orig.log.apply(console, arguments); };
        console.debug = function(){ sendConsole('debug', arguments); orig.debug.apply(console, arguments); };
        console.info = function(){ sendConsole('info', arguments); orig.info.apply(console, arguments); };
        console.warn = function(){ sendConsole('warn', arguments); orig.warn.apply(console, arguments); };
        console.error = function(){ sendConsole('error', arguments); orig.error.apply(console, arguments); };
    } catch(e) { }
})();";
            await webView.CoreWebView2.AddScriptToExecuteOnDocumentCreatedAsync(consoleOverrideScript);
            Log("Console override script injected.");
            Log("WebMessageReceived event registered.");

            // ★ 追加：iframe からのメッセージ受信
            webView.CoreWebView2.FrameCreated += (s, e) =>
            {
                Log($"FrameCreated event fired. FrameId={e.Frame.FrameId}");

                // iframe からの WebMessage も同じハンドラで受信
                e.Frame.WebMessageReceived += OnWebMessageReceived;
                Log($"Frame WebMessageReceived registered. FrameId={e.Frame.FrameId}");

                // クリーンアップ（iframe破棄時に追跡から外す）
                e.Frame.Destroyed += (sender, args) =>
                {
                    Log($"Frame destroyed. FrameId={e.Frame.FrameId}");
                };
            };
            
            webView.CoreWebView2.Navigate(@"https://ghsam095mgs.mhi.co.jp/KIT/Transit/Transit_PT.asp");
            // 一時的にDevToolsを有効化
            webView.CoreWebView2.Settings.AreDevToolsEnabled = true;
            Log("Settings configured.");
        }
        catch (Exception ex)
        {
            Log($"InitializeWebViewAsync failed: {ex.Message}");
            MessageBox.Show($"WebView2の初期化に失敗しました: {ex.Message}");
        }    
    }

    private void OnWebMessageReceived(object? sender, CoreWebView2WebMessageReceivedEventArgs e)
    {
        Log("WebMessageReceived event fired.");
        try
        {
            using var fs = new FileStream(@"C:\temp\DebugLog.txt", FileMode.Append);
            Log(@"Saving to: C:\temp\DebugLog.txt");
            using var writer = new StreamWriter(fs, Encoding.UTF8);

            var json = e.TryGetWebMessageAsString();
            Log($"Received JSON (length): {json?.Length ?? 0}");
            if (string.IsNullOrEmpty(json))
            {
                Log("JSON is empty, ignoring.");
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
            Log("File.WriteAllText completed.");

            ProcessAndPrint(payload.content);
            Log("UI status updated.");
        }
        catch (Exception ex)
        {
            Log($"OnWebMessageReceived failed: {ex.Message}");
            // 受信時の例外をアプリ全体で落とさないようにログに記録して継続
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

    private void Log(string message)
    {
        try
        {
            string logPath = @"C:\temp\DebugLog.txt";
            string? dir = Path.GetDirectoryName(logPath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            string line = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} - {message}\n";
            File.AppendAllText(logPath, line);
        }
        catch (Exception ex)
        {
            // ログ書き込み自体が失敗した場合は諦める or 別の場所に出力
            System.Diagnostics.Debug.WriteLine($"Log failed: {ex.Message}");
        }
    }
}