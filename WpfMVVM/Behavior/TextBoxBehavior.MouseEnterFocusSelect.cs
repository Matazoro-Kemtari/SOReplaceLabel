using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace WpfMvvm.Behavior
{
    public static partial class TextBoxBehavior
    {
        #region GetMouseEnterFocusSelect
        /// <summary>
        /// ゲッタ
        /// </summary>
        /// <param name="dependencyObject"></param>
        /// <returns></returns>
        [AttachedPropertyBrowsableForType(typeof(TextBox))]
        public static bool GetMouseEnterFocusSelect(DependencyObject dependencyObject)
        {
            if (dependencyObject == null)
            {
                throw new System.ArgumentNullException(nameof(dependencyObject));
            }
            return (bool)dependencyObject.GetValue(MouseEnterFocusSelect);
        }

        /// <summary>
        /// セッタ
        /// </summary>
        /// <param name="dependencyObject"></param>
        /// <param name="value"></param>
        [AttachedPropertyBrowsableForType(typeof(TextBox))]
        public static void SetMouseEnterFocusSelect(DependencyObject dependencyObject, bool value)
        {
            if (dependencyObject == null)
            {
                throw new System.ArgumentNullException(nameof(dependencyObject));
            }
            dependencyObject.SetValue(MouseEnterFocusSelect, value);
        }

        /// <summary>
        /// マウスカーソルの出入りで
        /// フォーカスを切り替える
        /// </summary>
        // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MouseEnterFocusSelect =
            DependencyProperty.RegisterAttached(
                "MouseEnterFocusSelect",
                typeof(bool),
                typeof(TextBoxBehavior),
                new UIPropertyMetadata(false, MouseEnterFocusChanged));

        /// <summary>
        /// MouseEnterFocus の値が変更されたときに呼び出される。
        /// KeyDown イベントハンドラの登録＆解除を行う。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void MouseEnterFocusChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
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
                element.MouseEnter -= TextBox_MouseEnterFocusSelect;
            }
            if (newValue)
            {
                element.MouseEnter += TextBox_MouseEnterFocusSelect;
            }
        }

        /// <summary>
        /// TextBox上でのEnterキー入力時にTabと同じ動作を行う
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void TextBox_MouseEnterFocusSelect(object sender, MouseEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                if (!textBox.IsFocused)
                {
                    textBox.Focus();
                    textBox.SelectAll();
                }
            }
        }
        #endregion GetMouseEnterFocusSelect
    }
}
