using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;

namespace Wada.SOReplaceLabel.Wpf.ViewModels;

// Services/WebView2Service.cs
public class WebView2Service(WebView2 webView) : IWebView2Service
{
    private readonly WebView2 _webView = webView ?? throw new ArgumentNullException(nameof(webView));
    private CoreWebView2Environment? _environment;

    public event EventHandler<string>? WebMessageReceived;

    public async Task InitializeAsync(string userDataFolder)
    {
        _environment = await CoreWebView2Environment.CreateAsync(
            userDataFolder: userDataFolder);
        await _webView.EnsureCoreWebView2Async(_environment);

        _webView.CoreWebView2.WebMessageReceived += (s, e) =>
        {
            var json = e.TryGetWebMessageAsString();
            WebMessageReceived?.Invoke(this, json);
        };
        // iframe からのメッセージも受け取るために、WebMessageReceived イベントを購読
        _webView.CoreWebView2.FrameCreated += (s, e) =>
        {
            e.Frame.WebMessageReceived += (fs, fe) =>
            {
                var json = fe.TryGetWebMessageAsString();
                WebMessageReceived?.Invoke(this, json);
            };
        };
    }

    public void Navigate(string url)
    {
        _webView.CoreWebView2?.Navigate(url);
    }

    public void AddScriptOnDocumentCreated(string script)
    {
        _webView.CoreWebView2?.AddScriptToExecuteOnDocumentCreatedAsync(script);
    }

    public Task PostWebMessageAsync(string message)
    {
        _webView.CoreWebView2?.PostWebMessageAsString(message);
        return Task.CompletedTask;
    }
}
