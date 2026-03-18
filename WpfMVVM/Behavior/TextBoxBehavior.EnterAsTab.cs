using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace WpfMvvm.Behavior
{
    public static partial class TextBoxBehavior
    {
        #region GetEnterAsTab
        /// <summary>
        /// ゲッタ
        /// </summary>
        /// <param name="dependencyObject"></param>
        /// <returns></returns>
        [AttachedPropertyBrowsableForType(typeof(TextBox))]
        public static bool GetEnterAsTab(DependencyObject dependencyObject)
        {
            if (dependencyObject == null)
            {
                throw new System.ArgumentNullException(nameof(dependencyObject));
            }
            return (bool)dependencyObject.GetValue(EnterAsTab);
        }

        /// <summary>
        /// セッタ
        /// </summary>
        /// <param name="dependencyObject"></param>
        /// <param name="value"></param>
        [AttachedPropertyBrowsableForType(typeof(TextBox))]
        public static void SetEnterAsTab(DependencyObject dependencyObject, bool value)
        {
            if (dependencyObject == null)
            {
                throw new System.ArgumentNullException(nameof(dependencyObject));
            }
            dependencyObject.SetValue(EnterAsTab, value);
        }

        /// <summary>
        /// Enterキーを入力時
        /// Tabキーと同様に次のコントロールへフォーカスを移す
        /// </summary>
        // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty EnterAsTab =
            DependencyProperty.RegisterAttached(
                "EnterAsTab",
                typeof(bool),
                typeof(TextBoxBehavior),
                new UIPropertyMetadata(false, EnterAsTabChanged));

        /// <summary>
        /// EnterAsTab の値が変更されたときに呼び出される。
        /// KeyDown イベントハンドラの登録＆解除を行う。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void EnterAsTabChanged(
            DependencyObject sender, DependencyPropertyChangedEventArgs e)
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
                element.KeyDown -= TextBox_EnterKeyDownMoveFocus;
            }
            if (newValue)
            {
                element.KeyDown += TextBox_EnterKeyDownMoveFocus;
            }
        }

        /// <summary>
        /// TextBox上でのEnterキー入力時にTabと同じ動作を行う
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void TextBox_EnterKeyDownMoveFocus(object sender, KeyEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.None
                && e.Key == Key.Enter)
            {
                //コントロール要素チェック
                if (!(sender is TextBox textBox))
                {
                    return;
                }

                //方向を決定
                var direction = FocusNavigationDirection.Next;
                textBox.MoveFocus(new TraversalRequest(direction));
            }
        }
        #endregion GetEnterAsTab
    }
}
