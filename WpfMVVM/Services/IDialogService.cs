using System.Windows;

namespace WpfMvvm.Services
{
    /// <summary>
    /// VMからダイアログを表示するサービス
    /// 参考
    /// http://sourcechord.hatenablog.com/entry/2016/01/23/170753
    /// </summary>
    public interface IDialogService
    {
        MessageBoxResult Show(string message);

        MessageBoxResult Show(string message, string caption);

        MessageBoxResult Show(string message, string caption, MessageBoxButton button);

        MessageBoxResult Show(string message, string caption, MessageBoxButton button, MessageBoxImage icon);

        MessageBoxResult Show(string message, string caption, MessageBoxButton button, MessageBoxImage icon, MessageBoxResult defaultResult);

        MessageBoxResult Show(string message, string caption, MessageBoxButton button, MessageBoxImage icon, MessageBoxResult defaultResult, MessageBoxOptions options);
    }
}