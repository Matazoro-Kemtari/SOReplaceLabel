using System.Windows;

namespace WpfMvvm.Behavior
{
    /// <summary>
    /// Windowを閉じる処理を提供する添付ビヘイビア
    /// </summary>
    public static class CloseWindowBehavior
    {
        /// <summary>
        /// ゲッター
        /// </summary>
        /// <param name="dependencyObject"></param>
        /// <returns></returns>
        [AttachedPropertyBrowsableForType(typeof(Window))]
        public static bool GetClose(DependencyObject dependencyObject)
        {
            if (dependencyObject == null)
            {
                throw new System.ArgumentNullException(nameof(dependencyObject));
            }
            return (bool)dependencyObject.GetValue(CloseProperty);
        }

        /// <summary>
        /// セッター
        /// </summary>
        /// <param name="dependencyObject"></param>
        /// <param name="value"></param>
        [AttachedPropertyBrowsableForType(typeof(Window))]
        public static void SetClose(DependencyObject dependencyObject, bool value)
        {
            if (dependencyObject == null)
            {
                throw new System.ArgumentNullException(nameof(dependencyObject));
            }
            dependencyObject.SetValue(CloseProperty, value);
        }

        /// <summary>
        /// 値がtrueになった時Windowを閉じる添付ビヘイビアを追加
        /// </summary>
        // Using a DependencyProperty as the backing store for Close.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CloseProperty =
            DependencyProperty.RegisterAttached(
                "Close",
                typeof(bool),
                typeof(CloseWindowBehavior),
                new PropertyMetadata(false, OnCloseChanged));

        /// <summary>
        /// プロパティが変更時の処理
        /// </summary>
        /// <param name="d"></param>
        /// <param name="e"></param>
        private static void OnCloseChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is Window window))
            {
                return;
            }

            // 設定された値を見てウインドウを閉じる
            var newValue = (bool)e.NewValue;

            if (newValue)
            {
                window.Close();
            }
        }

        /// <summary>
        /// ゲッター
        /// </summary>
        /// <param name="dependencyObject"></param>
        /// <returns></returns>
        [AttachedPropertyBrowsableForType(typeof(Window))]
        public static bool GetDialogResult(DependencyObject dependencyObject)
        {
            if (dependencyObject == null)
            {
                throw new System.ArgumentNullException(nameof(dependencyObject));
            }
            return (bool)dependencyObject.GetValue(DialogResultProperty);
        }

        /// <summary>
        /// セッター
        /// </summary>
        /// <param name="dependencyObject"></param>
        /// <param name="value"></param>
        [AttachedPropertyBrowsableForType(typeof(Window))]
        public static void SetDialogResult(DependencyObject dependencyObject, bool value)
        {
            if (dependencyObject == null)
            {
                throw new System.ArgumentNullException(nameof(dependencyObject));
            }
            dependencyObject.SetValue(DialogResultProperty, value);
        }

        /// <summary>
        /// DialogResultを添付ビヘイビアによりセットする
        /// </summary>
        public static readonly DependencyProperty DialogResultProperty =
            DependencyProperty.RegisterAttached(
                "DialogResult",
                typeof(bool?),
                typeof(CloseWindowBehavior),
                new PropertyMetadata(DialogResultChanged));

        /// <summary>
        /// DialogResult変更イベント
        /// </summary>
        /// <param name="d"></param>
        /// <param name="e"></param>
        private static void DialogResultChanged(
            DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {
            if (!(d is Window window))
            {
                return;
            }

            if (!window.IsLoaded)
            {
                return;
            }

            window.DialogResult = e.NewValue as bool?;
        }
    }
}