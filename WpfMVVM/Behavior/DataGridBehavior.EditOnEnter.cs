using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace WpfMvvm.Behavior
{
    public static partial class DataGridBehavior
    {
        /// <summary>
        /// ゲッタ
        /// </summary>
        /// <param name="dependencyObject"></param>
        /// <returns></returns>
        [AttachedPropertyBrowsableForType(typeof(DataGrid))]
        public static bool GetEditOnEnter(DependencyObject dependencyObject)
        {
            if (dependencyObject == null)
            {
                throw new System.ArgumentNullException(nameof(dependencyObject));
            }
            return (bool)dependencyObject.GetValue(EditOnEnterProperty);
        }

        /// <summary>
        /// セッタ
        /// </summary>
        /// <param name="dependencyObject"></param>
        /// <param name="value"></param>
        [AttachedPropertyBrowsableForType(typeof(DataGrid))]
        public static void SetEditOnEnter(DependencyObject dependencyObject, bool value)
        {
            if (dependencyObject == null)
            {
                throw new System.ArgumentNullException(nameof(dependencyObject));
            }
            dependencyObject.SetValue(EditOnEnterProperty, value);
        }

        // Using a DependencyProperty as the backing store for EditOnEnter.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty EditOnEnterProperty =
            DependencyProperty.RegisterAttached(
                "EditOnEnter",
                typeof(bool),
                typeof(DataGridBehavior),
                new PropertyMetadata(false, OnEditOnEnterChanged));

        /// <summary>
        ///
        /// </summary>
        /// <param name="dependencyObject"></param>
        /// <param name="e"></param>
        private static void OnEditOnEnterChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            //コントロール要素チェック
            if (!(dependencyObject is DataGrid element))
            {
                return;
            }

            // 設定された値を見てイベントを登録・削除
            var newValue = (bool)e.NewValue;
            var oldValue = (bool)e.OldValue;

            if (oldValue)
            {
                element.GotFocus -= DataGrid_GotFocus;
                element.PreviewKeyDown -= DataGrid_PreviewKeyDown;
            }
            if (newValue)
            {
                element.GotFocus += DataGrid_GotFocus;
                element.PreviewKeyDown += DataGrid_PreviewKeyDown;
            }
        }

        /// <summary>
        /// フォーカスを取得時に即座に編集モードへ移行する
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void DataGrid_GotFocus(object sender, RoutedEventArgs e)
        {
            if (e.OriginalSource is DataGridCell dataGridCell
                && !dataGridCell.IsReadOnly
                && sender is DataGrid dataGrid)
            {
                //編集を開始(BeginEdit時に発生するGotFocusイベントを抑制)
                SetEditOnEnter(dataGrid, false);
                dataGrid.BeginEdit(e);
                SetEditOnEnter(dataGrid, true);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void DataGrid_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (!(sender is DataGrid dataGrid))
            {
                return;
            }

            //Enterキー入力時次のセルへ移動する
            if (e.Key == Key.Enter)
            {
                e.Handled = true;
                //次のセルへ移動
                MoveNextCell(dataGrid);
            }
        }

        /// <summary>
        /// 現在位置の次のセルを選択する(Tabキー準拠）
        /// </summary>
        /// <param name="dataGrid"></param>
        private static void MoveNextCell(DataGrid dataGrid)
        {
            var currentCol = dataGrid.CurrentColumn;
            // 現在のカラムが最大かどうか
            bool isLastCol = (currentCol.DisplayIndex == dataGrid.Columns.Count - 1);
            if (!isLastCol)
            {
                // 編集を終了して次のカラムへ(CommitEdit時に発生するGotFocusイベントを抑制)
                SetEditOnEnter(dataGrid, false);
                dataGrid.CommitEdit();
                SetEditOnEnter(dataGrid, true);
                dataGrid.CurrentColumn = dataGrid.Columns[currentCol.DisplayIndex + 1];
            }
            else
            {
                // 現在行取得
                int currentrow = dataGrid.Items.IndexOf(dataGrid.SelectedItem);
                // 最大行数
                int rowMax = dataGrid.Items.Count;

                if ((currentrow + 1) != rowMax)
                {
                    // 編集を終了して次行の先頭へ(CommitEdit時に発生するGotFocusイベントを抑制)
                    SetEditOnEnter(dataGrid, false);
                    dataGrid.CommitEdit();
                    SetEditOnEnter(dataGrid, true);
                    dataGrid.SelectedIndex = currentrow + 1;
                    dataGrid.CurrentCell = new DataGridCellInfo(dataGrid.Items[currentrow + 1], dataGrid.Columns[0]);
                }
            }
        }
    }
}
