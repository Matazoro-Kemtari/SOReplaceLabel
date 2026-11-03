using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Disposables;
using System.Text.Json;
using System.Windows;
using Microsoft.Web.WebView2.Core;
using Prism.Commands;
using Prism.Mvvm;

namespace Wada.SOReplaceLabel.Wpf.ViewModels;

public class WebViewPageViewModel : BindableBase, IDestructible
{
    private IWebView2Service? _webViewService;

    /// <summary>
    /// レガシーな IE 向けシステムにおける ActiveX (Scripting.FileSystemObject) のファイル書き込み処理を模擬（Shim）するスクリプト。
    /// WebView2 (Chromium) では未サポートの FileSystemObject によるファイル出力をインターセプトし、
    /// 内容をバッファリングして Close 時に window.chrome.webview.postMessage 経由で C# 側へ JSON 送信します。
    /// </summary>
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
})();";

    /// <summary>
    /// ドキュメント作成時に WebView2 の疎通・読み込み状態を確認するための診断用 Ping スクリプト。
    /// ページ読み込み完了時に 1 回だけ現在の URL と __ping__: true を C# 側に postMessage します。
    /// </summary>
    private const string DiagnosticPingScript = @"
(function(){
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

    /// <summary>
    /// ブラウザ内の console.log / info / warn / error / debug 出力をフックし、
    /// 元のブラウザコンソールに出力しつつ C# 側へ __console__: true として postMessage 転送するデバッグ用スクリプト。
    /// </summary>
    private const string ConsoleOverrideScript = @"
(function(){
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

    public async Task InitializeAsync(IWebView2Service webView2Service)
    {
        _webViewService = webView2Service;
        _webViewService.WebMessageReceived += OnWebMessageReceived;

        var userDataFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Wada.SOReplaceLabel", "WebView2Data");

        await _webViewService.InitializeAsync(userDataFolder);

        // スクリプト注入
        _webViewService.AddScriptOnDocumentCreated(ActiveXShimScript);
        // TODO: 後で消す: ドキュメント作成時にホストへ到達確認用の ping メッセージを送るスクリプトを注入
        //_webViewService.AddScriptOnDocumentCreated(DiagnosticPingScript);
        //_webViewService.AddScriptOnDocumentCreated(ConsoleOverrideScript);

        // ナビゲート
        _webViewService.Navigate("https://ghsam095mgs.mhi.co.jp/KIT/Transit/Transit_PT.asp");
    }

    private void OnWebMessageReceived(object sender, string json)
    {
        if (string.IsNullOrEmpty(json)) return;

        try
        {
            var payload = JsonSerializer.Deserialize<ShopOrderPayload>(json);
            if (payload?.content == null) return;

            // ファイル書き込み
            File.AppendAllText(@"C:\temp\ShopOrder.txt", payload.content + Environment.NewLine);

            // 印刷処理
            //ProcessAndPrint(payload.content);

        }
        catch (Exception ex)
        {
            throw new NotImplementedException($"Error processing web message: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Disposeが必要なReactivePropertyやReactiveCommandを集約させるための仕掛け
    /// </summary>
    private CompositeDisposable Disposables { get; } = [];

    public void Destroy()
    {
        if (_webViewService != null)
        {
            _webViewService.WebMessageReceived -= OnWebMessageReceived;
        }
        Disposables.Dispose();
    }

    private class ShopOrderPayload
    {
        public string path { get; set; } = string.Empty;
        public string content { get; set; } = string.Empty;
    }
}
