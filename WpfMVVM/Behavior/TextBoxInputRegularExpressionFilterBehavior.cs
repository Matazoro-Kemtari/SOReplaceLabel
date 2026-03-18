using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Text.RegularExpressions;

namespace WpfMvvm.Behavior
{
	public static class TextBoxInputRegularExpressionFilterBehavior
	{
		/// <summary>
		/// ゲッタ
		/// </summary>
		/// <param name="dependencyObject"></param>
		/// <returns></returns>
		[AttachedPropertyBrowsableForType(typeof(TextBox))]
		public static string GetPattern(DependencyObject dependencyObject)
		{
			if (dependencyObject == null)
			{
				throw new System.ArgumentNullException(nameof(dependencyObject));
			}
			return (string)dependencyObject.GetValue(PatternProperty);
		}

		/// <summary>
		/// セッタ
		/// </summary>
		/// <param name="dependencyObject"></param>
		/// <param name="value"></param>
		[AttachedPropertyBrowsableForType(typeof(TextBox))]
		public static void SetPattern(DependencyObject dependencyObject, string value)
		{
			if (dependencyObject == null)
			{
				throw new System.ArgumentNullException(nameof(dependencyObject));
			}
			dependencyObject.SetValue(PatternProperty, value);
		}

		// Using a DependencyProperty as the backing store for RegularExpressionFilterPattern.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty PatternProperty =
			DependencyProperty.RegisterAttached(
				"Pattern",
				typeof(string),
				typeof(TextBoxInputRegularExpressionFilterBehavior),
				new PropertyMetadata(null, OnPatternChanged));

		private static void OnPatternChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
		{
			//コントロール要素チェック
			if (!(dependencyObject is TextBox textBox))
			{
				return;
			}

			// 設定された値を見てイベントを登録・削除
			var newValue = (string)e.NewValue;
			var oldValue = (string)e.OldValue;

			if (oldValue != null)
			{
				textBox.PreviewTextInput -= TextBox_PreviewTextInput;
				DataObject.RemovePastingHandler(textBox, DataObject_Pasting);
			}
			if (newValue != null)
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
			if(textBox.SelectedText.Length > 0)
			{
				checkText = checkText.Remove(textBox.SelectionStart, textBox.SelectedText.Length);
			}
			//入力後のテキストを作成
			checkText = checkText.Insert(textBox.SelectionStart, e.Text);
			var pattern = GetPattern(sender as DependencyObject);
			//入力後のテキストを検証
			if (Regex.IsMatch(checkText, pattern))
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
			var pattern = GetPattern(sender as DependencyObject);
			//貼り付け後のテキストを検証
			if (Regex.IsMatch(checkText, pattern))
			{
				return;
			}

			//貼り付け処理無効化
			e.CancelCommand();
		}
	}
}
