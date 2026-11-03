using Microsoft.Web.WebView2.Core;

namespace Wada.SOReplaceLabel.Wpf.ViewModels;

// Services/IWebView2Service.cs
public interface IWebView2Service
{
    Task InitializeAsync(string userDataFolder);
    void Navigate(string url);
    void AddScriptOnDocumentCreated(string script);
    event EventHandler<string> WebMessageReceived;
    Task PostWebMessageAsync(string message);
}
