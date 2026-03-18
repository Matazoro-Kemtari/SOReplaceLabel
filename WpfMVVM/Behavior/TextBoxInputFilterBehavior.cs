using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace WpfMvvm.Behavior
{
    /// <summary>
    /// TextBoxの入力値条件を指定するビヘイビア
    /// </summary>
    public static class TextBoxInputFilterBehavior
    {
        public enum TextBoxInputFilter
        {
            None,
            NumericInput,
            PositiveNumericInput,
            PositiveNumericIncludeZeroInput,
            NegativeNumericInput,
            NegativeNumericIncludeZeroInput,
            IntegerInput,
            PositiveIntegerInput,
            PositiveIntegerIncludeZeroInput,
            NegativeIntegerInput,
            NegativeIntegerIncludeZeroInput,
        }

        /// <summary>
        /// ゲッタ
        /// </summary>
        /// <param name="dependencyObject"></param>
        /// <returns></returns>
        [AttachedPropertyBrowsableForType(typeof(TextBox))]
        public static TextBoxInputFilter GetTextBoxInputFilter(DependencyObject dependencyObject)
        {
            if (dependencyObject == null)
            {
                throw new System.ArgumentNullException(nameof(dependencyObject));
            }
            return (TextBoxInputFilter)dependencyObject.GetValue(TextBoxInputFilterProperty);
        }

        /// <summary>
        /// セッタ
        /// </summary>
        /// <param name="dependencyObject"></param>
        /// <param name="value"></param>
        [AttachedPropertyBrowsableForType(typeof(TextBox))]
        public static void SetTextBoxInputFilter(DependencyObject dependencyObject, TextBoxInputFilter value)
        {
            if (dependencyObject == null)
            {
                throw new System.ArgumentNullException(nameof(dependencyObject));
            }
            dependencyObject.SetValue(TextBoxInputFilterProperty, value);
        }

        // Using a DependencyProperty as the backing store for TextBoxInputFilter.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TextBoxInputFilterProperty =
            DependencyProperty.RegisterAttached(
                "TextBoxInputFilter",
                typeof(TextBoxInputFilter),
                typeof(TextBoxInputFilterBehavior),
                new PropertyMetadata(TextBoxInputFilter.None, OnTextBoxInputFilterChanged));

        private static void OnTextBoxInputFilterChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            //コントロール要素チェック
            if (!(dependencyObject is TextBox textBox))
            {
                return;
            }

            // 設定された値を見てイベントを登録・削除
            var newValue = (TextBoxInputFilter)e.NewValue;
            var oldValue = (TextBoxInputFilter)e.OldValue;

            if (oldValue != TextBoxInputFilter.None)
            {
                textBox.PreviewTextInput -= TextBox_PreviewTextInput;
                DataObject.RemovePastingHandler(textBox, DataObject_Pasting);
            }
            if (newValue != TextBoxInputFilter.None)
            {
                textBox.PreviewTextInput += TextBox_PreviewTextInput;
                DataObject.AddPastingHandler(textBox, DataObject_Pasting);
            }
        }

        /// <summary>
        /// テキスト入力前イベントで入力値を検証
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!(sender is TextBox textBox))
            {
                return;
            }

            //検証用入力テキストをテストする
            string checkText = textBox.Text;
            //選択済みテキストの消去
            if (textBox.SelectedText.Length > 0)
            {
                checkText = checkText.Remove(textBox.SelectionStart, textBox.SelectedText.Length);
            }
            //入力後のテキストを作成
            checkText = checkText.Insert(textBox.SelectionStart, e.Text);
            var filter = GetTextBoxInputFilter(sender as DependencyObject);
            //入力後のテキストを検証
            if (filter == TextBoxInputFilter.None
                || CanParse(checkText, filter))
            {
                return;
            }

            e.Handled = true;
        }

        /// <summary>
        /// テキスト貼り付けイベントで入力値を検証
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void DataObject_Pasting(object sender, DataObjectPastingEventArgs e)
        {
            if (!(sender is TextBox textBox))
            {
                return;
            }

            var pastedText = e.SourceDataObject.GetData(DataFormats.UnicodeText) as string;
            if (pastedText == null)
            {
                return;
            }

            //貼り付け後のテキストを作成
            var checkText = textBox.Text.Remove(textBox.SelectionStart, textBox.SelectionLength)
                                   .Insert(textBox.SelectionStart, pastedText);
            var filter = GetTextBoxInputFilter(sender as DependencyObject);
            //貼り付け後のテキストを検証
            if (filter == TextBoxInputFilter.None
                || CanParse(checkText, filter))
            {
                return;
            }

            //貼り付け処理無効化
            e.CancelCommand();
        }

        /// <summary>
        /// 指定したフィルターでの変換が可能か判定する
        /// </summary>
        /// <param name="checkText"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        private static bool CanParse(string checkText, TextBoxInputFilter filter)
        {
            switch(filter)
            {
                case TextBoxInputFilter.None:
                    return true;

                case TextBoxInputFilter.NumericInput:
                    return IsNumericInput(checkText);

                case TextBoxInputFilter.PositiveNumericInput:
                    return IsPositiveNumericInput(checkText, false);

                case TextBoxInputFilter.PositiveNumericIncludeZeroInput:
                    return IsPositiveNumericInput(checkText, true);

                case TextBoxInputFilter.NegativeNumericInput:
                    return IsNegativeNumericInput(checkText, false);

                case TextBoxInputFilter.NegativeNumericIncludeZeroInput:
                    return IsNegativeNumericInput(checkText, true);

                case TextBoxInputFilter.IntegerInput:
                    return IsIntegerInput(checkText);

                case TextBoxInputFilter.PositiveIntegerInput:
                    return IsPositiveIntegerInput(checkText, false);

                case TextBoxInputFilter.PositiveIntegerIncludeZeroInput:
                    return IsPositiveIntegerInput(checkText, true);

                case TextBoxInputFilter.NegativeIntegerInput:
                    return IsNegativeIntegerInput(checkText, false);

                case TextBoxInputFilter.NegativeIntegerIncludeZeroInput:
                    return IsNegativeIntegerInput(checkText, true);

                default:
                    return false;
            }
        }

        /// <summary>
        /// 数値入力フィルダー
        /// </summary>
        /// <param name="checkText"></param>
        /// <returns></returns>
        private static bool IsNumericInput(string checkText)
        {
            if(string.IsNullOrWhiteSpace(checkText))
            {
                return false;
            }

            //decimal値に変換
            var canParse = decimal.TryParse(checkText, out var parseValue);
            //"-"の入力のとき入力を許容する
            if (!canParse)
            {
                canParse = checkText == "-";
            }

            return canParse;
        }

        /// <summary>
        /// 正の数値入力フィルダー
        /// </summary>
        /// <param name="checkText"></param>
        /// <param name="includeZero"></param>
        /// <returns></returns>
        private static bool IsPositiveNumericInput(string checkText, bool includeZero = false)
        {
            if (string.IsNullOrWhiteSpace(checkText))
            {
                return false;
            }

            //decimal値に変換
            var canParse = decimal.TryParse(checkText, out var parseValue);
            //正負判定
            if(canParse)
            {
                canParse &= includeZero ? parseValue >= 0 : parseValue > 0;
            }

            return canParse;
        }

        /// <summary>
        /// 負の数値入力フィルター
        /// </summary>
        /// <param name="checkText"></param>
        /// <param name="includeZero"></param>
        /// <returns></returns>
        private static bool IsNegativeNumericInput(string checkText, bool includeZero = false)
        {
            if (string.IsNullOrWhiteSpace(checkText))
            {
                return false;
            }

            //decimal値に変換
            var canParse = decimal.TryParse(checkText, out var parseValue);
            //正負判定
            if (canParse)
            {
                canParse &= includeZero ? parseValue <= 0 : parseValue < 0;
            }

            return canParse;
        }

        /// <summary>
        /// 整数入力フィルター
        /// </summary>
        /// <param name="checkText"></param>
        /// <returns></returns>
        private static bool IsIntegerInput(string checkText)
        {
            if (string.IsNullOrWhiteSpace(checkText))
            {
                return false;
            }

            //int値に変換
            var canParse = int.TryParse(checkText, out var parseValue);
            //"-"の入力のとき入力を許容する
            if (!canParse)
            {
                canParse = checkText == "-";
            }

            return canParse;
        }

        /// <summary>
        /// 正の整数入力フィルター
        /// </summary>
        /// <param name="checkText"></param>
        /// <param name="includeZero"></param>
        /// <returns></returns>
        private static bool IsPositiveIntegerInput(string checkText, bool includeZero = false)
        {
            if (string.IsNullOrWhiteSpace(checkText))
            {
                return false;
            }

            //int値に変換
            var canParse = int.TryParse(checkText, out var parseValue);
            //正負判定
            if (canParse)
            {
                canParse &= includeZero ? parseValue >= 0 : parseValue > 0;
            }

            return canParse;
        }

        /// <summary>
        /// 負の整数入力フィルター
        /// </summary>
        /// <param name="checkText"></param>
        /// <param name="includeZero"></param>
        /// <returns></returns>
        private static bool IsNegativeIntegerInput(string checkText, bool includeZero)
        {
            if (string.IsNullOrWhiteSpace(checkText))
            {
                return false;
            }

            //int値に変換
            var canParse = int.TryParse(checkText, out var parseValue);
            //正負判定
            if (canParse)
            {
                canParse &= includeZero ? parseValue <= 0 : parseValue < 0;
            }

            return canParse;
        }
    }
}
