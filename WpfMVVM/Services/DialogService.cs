using System.Windows;

namespace WpfMvvm.Services
{
    public class DialogService : IDialogService
    {
        /// <summary>
        /// オーナーウインドウ
        /// </summary>
        private readonly Window _owner;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="owner"></param>
        public DialogService(Window owner = null)
        {
            _owner = owner;
        }

        /// <summary>
        /// ウインドウの前面にメッセージボックスを表示する
        /// </summary>
        /// <param name="message"></param>
        public MessageBoxResult Show(string message)
        {
            if (_owner != null)
            {
                _owner.Activate();
                return MessageBox.Show(_owner, message);
            }
            else
            {
                return MessageBox.Show(message);
            }
        }

        /// <summary>
        /// ウインドウの前面にメッセージボックスを表示する
        /// </summary>
        /// <param name="message"></param>
        /// <param name="caption"></param>
        /// <returns></returns>
        public MessageBoxResult Show(string message, string caption)
        {
            if (_owner != null)
            {
                _owner.Activate();
                return MessageBox.Show(_owner, message, caption);
            }
            else
            {
                return MessageBox.Show(message, caption);
            }
        }

        /// <summary>
        /// ウインドウの前面にメッセージボックスを表示する
        /// </summary>
        /// <param name="message"></param>
        /// <param name="caption"></param>
        /// <param name="button"></param>
        /// <returns></returns>
        public MessageBoxResult Show(string message, string caption, MessageBoxButton button)
        {
            if (_owner != null)
            {
                _owner.Activate();
                return MessageBox.Show(_owner, message, caption, button);
            }
            else
            {
                return MessageBox.Show(message, caption, button);
            }
        }

        /// <summary>
        /// ウインドウの前面にメッセージボックスを表示する
        /// </summary>
        /// <param name="message"></param>
        /// <param name="caption"></param>
        /// <param name="button"></param>
        /// <param name="icon"></param>
        /// <returns></returns>
        public MessageBoxResult Show(string message, string caption, MessageBoxButton button, MessageBoxImage icon)
        {
            if (_owner != null)
            {
                _owner.Activate();
                return MessageBox.Show(_owner, message, caption, button, icon);
            }
            else
            {
                return MessageBox.Show(message, caption, button, icon);
            }
        }

        /// <summary>
        /// ウインドウの前面にメッセージボックスを表示する
        /// </summary>
        /// <param name="message"></param>
        /// <param name="caption"></param>
        /// <param name="button"></param>
        /// <param name="icon"></param>
        /// <param name="defaultResult"></param>
        /// <returns></returns>
        public MessageBoxResult Show(string message, string caption, MessageBoxButton button, MessageBoxImage icon, MessageBoxResult defaultResult)
        {
            if (_owner != null)
            {
                _owner.Activate();
                return MessageBox.Show(_owner, message, caption, button, icon, defaultResult);
            }
            else
            {
                return MessageBox.Show(message, caption, button, icon, defaultResult);
            }
        }

        /// <summary>
        /// ウインドウの前面にメッセージボックスを表示する
        /// </summary>
        /// <param name="message"></param>
        /// <param name="caption"></param>
        /// <param name="button"></param>
        /// <param name="icon"></param>
        /// <param name="defaultResult"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public MessageBoxResult Show(string message, string caption, MessageBoxButton button, MessageBoxImage icon, MessageBoxResult defaultResult, MessageBoxOptions options)
        {
            if (_owner != null)
            {
                _owner.Activate();
                return MessageBox.Show(_owner, message, caption, button, icon, defaultResult, options);
            }
            else
            {
                return MessageBox.Show(message, caption, button, icon, defaultResult, options);
            }
        }
    }
}