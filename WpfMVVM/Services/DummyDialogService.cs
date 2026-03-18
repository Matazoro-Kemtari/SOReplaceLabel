using System.Windows;

namespace WpfMvvm.Services
{
    public class DummyDialogService : IDialogService
    {
        /// <summary>
        /// ダミー戻り値
        /// </summary>
        public MessageBoxResult DummyResult { get; set; }

        public string Message { get; private set; }

        public string Caption { get; private set; }

        public DummyDialogService()
        {
            DummyResult = MessageBoxResult.OK;
        }

        public MessageBoxResult Show(string message)
        {
            Message = message;
            return DummyResult;
        }

        public MessageBoxResult Show(string message, string caption)
        {
            Message = message;
            Caption = caption;
            return DummyResult;
        }

        public MessageBoxResult Show(string message, string caption, MessageBoxButton button)
        {
            Message = message;
            Caption = caption;
            return DummyResult;
        }

        public MessageBoxResult Show(string message, string caption, MessageBoxButton button, MessageBoxImage icon)
        {
            Message = message;
            Caption = caption;
            return DummyResult;
        }

        public MessageBoxResult Show(string message, string caption, MessageBoxButton button, MessageBoxImage icon, MessageBoxResult defaultResult)
        {
            Message = message;
            Caption = caption;
            return DummyResult;
        }

        public MessageBoxResult Show(string message, string caption, MessageBoxButton button, MessageBoxImage icon, MessageBoxResult defaultResult, MessageBoxOptions options)
        {
            Message = message;
            Caption = caption;
            return DummyResult;
        }
    }
}