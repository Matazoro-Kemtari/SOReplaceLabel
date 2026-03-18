using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace WpfMvvm.Behavior
{
    public static partial class TextBoxBehavior
    {
        #region OnFocusSelectText
        /// <summary>
        /// ゲッタ
        /// </summary>
        /// <param name="dependencyObject"></param>
        /// <returns></returns>
        [AttachedPropertyBrowsableForType(typeof(TextBox))]
        public static bool GetOnFocusSelectText(DependencyObject dependencyObject)
        {
            if (dependencyObject == null)
            {
                throw new System.ArgumentNullException(nameof(dependencyObject));
            }
            return (bool)dependencyObject.GetValue(OnFocusSelectText);
        }

        /// <summary>
        /// セッタ
        /// </summary>
        /// <param name="dependencyObject"></param>
        /// <param name="value"></param>
        [AttachedPropertyBrowsableForType(typeof(TextBox))]
        public static void SetOnFocusSelectText(DependencyObject dependencyObject, bool value)
        {
            if (dependencyObject == null)
            {
                throw new System.ArgumentNullException(nameof(dependencyObject));
            }
            dependencyObject.SetValue(OnFocusSelectText, value);
        }

        /// <summary>
        /// マウスカーソルの出入りで
        /// フォーカスを切り替える
        /// </summary>
        // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty OnFocusSelectText =
            DependencyProperty.RegisterAttached(
                "OnFocusSelectText",
                typeof(bool),
                typeof(TextBoxBehavior),
                new UIPropertyMetadata(false, OnFocusSelectTextChanged));

        /// <summary>
        /// MouseEnterFocus の値が変更されたときに呼び出される。
        /// KeyDown イベントハンドラの登録＆解除を行う。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void OnFocusSelectTextChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            //コントロール要素チェック
            if (!(sender is TextBox element))
            {
                return;
            }

            // 設定された値を見てイベントを登録・削除
            var newValue = (bool)e.NewValue;
            var oldValue = (bool)e.OldValue;

            if (oldValue)
            {
                element.PreviewMouseLeftButtonDown -= TextBox_PreviewMouseLeftButtonDownSelectText;
                element.GotFocus -= TextBox_GotFocusSelectText;
            }
            if (newValue)
            {
                element.PreviewMouseLeftButtonDown += TextBox_PreviewMouseLeftButtonDownSelectText;
                element.GotFocus += TextBox_GotFocusSelectText;
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void TextBox_PreviewMouseLeftButtonDownSelectText(object sender, MouseButtonEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                if (!textBox.IsFocused)
                {
                    textBox.Focus();
                    e.Handled = true;
                }
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void TextBox_GotFocusSelectText(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                textBox.SelectAll();
            }
        }
        #endregion OnFocusSelectText
    }
}
