using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using WpfMvvm;
using System.Collections;
using System.ComponentModel;
using System.Linq;
using System.Windows.Controls.Primitives;

namespace WpfMvvm.Behavior
{
	public static partial class DataGridBehavior
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="dependencyObject"></param>
		/// <returns></returns>
		[AttachedPropertyBrowsableForType(typeof(DataGrid))]
		public static bool GetCanUserReorderRows(DependencyObject dependencyObject)
		{
			if (dependencyObject == null)
			{
				throw new System.ArgumentNullException(nameof(dependencyObject));
			}
			return (bool)dependencyObject.GetValue(CanUserReorderRowsProperty);
		}

		/// <summary>
		/// /
		/// </summary>
		/// <param name="dependencyObject"></param>
		/// <param name="value"></param>
		[AttachedPropertyBrowsableForType(typeof(DataGrid))]
		public static void SetCanUserReorderRows(DependencyObject dependencyObject, bool value)
		{
			if (dependencyObject == null)
			{
				throw new System.ArgumentNullException(nameof(dependencyObject));
			}
			dependencyObject.SetValue(CanUserReorderRowsProperty, value);
		}

		// Using a DependencyProperty as the backing store for CanUserReorderRows.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty CanUserReorderRowsProperty =
			DependencyProperty.RegisterAttached(
				"CanUserReorderRows",
				typeof(bool),
				typeof(DataGridBehavior),
				new PropertyMetadata(false, OnCanUserReorderRowsChanged));

		/// <summary>
		/// 
		/// </summary>
		/// <param name="dependencyObject"></param>
		/// <param name="e"></param>
		private static void OnCanUserReorderRowsChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
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
				element.MouseLeftButtonDown -= DataGrid_MouseLeftButtonDown;
				element.MouseLeftButtonUp -= DataGrid_MouseLeftButtonUp;
				element.MouseLeave -= Element_MouseLeave;
			}
			if (newValue)
			{
				element.PreviewMouseLeftButtonDown += DataGrid_MouseLeftButtonDown;
				element.MouseLeftButtonUp += DataGrid_MouseLeftButtonUp;
				element.MouseLeave += Element_MouseLeave;
			}
		}

		#region IsDragging
		[AttachedPropertyBrowsableForType(typeof(DataGrid))]
		public static bool GetIsDragging(DependencyObject dependencyObject)
		{
			if (dependencyObject == null)
			{
				throw new System.ArgumentNullException(nameof(dependencyObject));
			}
			return (bool)dependencyObject.GetValue(IsDraggingProperty);
		}

		[AttachedPropertyBrowsableForType(typeof(DataGrid))]
		private static void SetIsDragging(DependencyObject dependencyObject, bool value)
		{
			System.Console.WriteLine("IsDragging:" + value);
			if (dependencyObject == null)
			{
				throw new System.ArgumentNullException(nameof(dependencyObject));
			}
			dependencyObject.SetValue(IsDraggingProperty, value);
		}

		// Using a DependencyProperty as the backing store for IsDragging.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty IsDraggingProperty =
			DependencyProperty.RegisterAttached(
				"IsDragging", 
				typeof(bool), 
				typeof(DataGridBehavior), 
				new PropertyMetadata(false));
		#endregion

		#region DragStartIndex
		private static int? GetDragStartIndex(DependencyObject dependencyObject)
		{
			return (int?)dependencyObject.GetValue(DragStartIndexProperty);
		}

		private static void SetDragStartIndex(DependencyObject dependencyObject, int? value)
		{
			dependencyObject.SetValue(DragStartIndexProperty, value);
		}

		// Using a DependencyProperty as the backing store for DragStartIndex.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty DragStartIndexProperty =
			DependencyProperty.RegisterAttached(
				"DragStartIndex",
				typeof(int?),
				typeof(DataGridBehavior),
				new PropertyMetadata(null));
		#endregion

		#region DragSourceIndexes
		[AttachedPropertyBrowsableForType(typeof(DataGrid))]
		private static object[] GetDragSourceItems(DependencyObject dependencyObject)
		{
			return (object[])dependencyObject.GetValue(DragSourceItemsProperty);
		}

		[AttachedPropertyBrowsableForType(typeof(DataGrid))]
		private static void SetDragSourceItems(DependencyObject dependencyObject, object[] value)
		{
			dependencyObject.SetValue(DragSourceItemsProperty, value);
		}

		// Using a DependencyProperty as the backing store for DragSourceItems.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty DragSourceItemsProperty =
			DependencyProperty.RegisterAttached(
				"DragSourceItems",
				typeof(object[]),
				typeof(DataGridBehavior),
				new PropertyMetadata(System.Array.Empty<object>()));
		#endregion

		/// <summary>
		/// Drag開始
		/// </summary>
		/// <param name="dataGrid"></param>
		private static void DragStart(DataGrid dataGrid, int startIndex)
		{
			SetDragStartIndex(dataGrid, startIndex);
			SetDragSourceItems(dataGrid, dataGrid.SelectedItems.OfType<object>().ToArray());
			SetIsDragging(dataGrid, true);
		}

		/// <summary>
		/// Drag終了
		/// </summary>
		/// <param name="dataGrid"></param>
		private static void DragEnd(DataGrid dataGrid)
		{
			SetDragStartIndex(dataGrid, null);
			SetDragSourceItems(dataGrid, System.Array.Empty<object>());
			SetIsDragging(dataGrid, false);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private static void DataGrid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			//コントロール要素チェック
			if (!(sender is DataGrid dataGrid))
			{
				return;
			}

			//行をクリックしているか把握する
			var row = UIHelpers.TryFindFromPoint<DataGridRow>((UIElement)sender, e.GetPosition(dataGrid));
			if (row == null)
			{
				return;
			}

			//クリックした行を選択してDrag開始
			row.IsSelected = true;
			DragStart(dataGrid, row.GetIndex());
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private static void DataGrid_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			//コントロール要素チェック
			if (!(sender is DataGrid dataGrid))
			{
				return;
			}
			//Drag中か判断
			if(!GetIsDragging(dataGrid))
			{
				return;
			}

			//マウスが離れたときの行位置を取得する
			int dropTargetIndex = 0;
			var row = UIHelpers.TryFindFromPoint<DataGridRow>((UIElement)sender, e.GetPosition(dataGrid));
			if(row != null)
			{
				dropTargetIndex = row.GetIndex();
			}
			else
			{
				if(UIHelpers.TryFindFromPoint<DataGrid>((UIElement)sender, e.GetPosition(dataGrid)) != null)
				{
					dropTargetIndex = dataGrid.Items.Count;
				}
			}

			//ドラッグ位置が変化するときDrag&Dropを実行
			if (dropTargetIndex == GetDragStartIndex(dataGrid))
			{
				DragEnd(dataGrid);
				return;
			}

			var gridSourceList = dataGrid.ItemsSource?.TryGetList();
			var dragSources = GetDragSourceItems(dataGrid);

			foreach(var dragSource in dragSources)
			{
				if(gridSourceList != null)
				{
					var removeIndex = gridSourceList.IndexOf(dragSource);
					if (removeIndex < dropTargetIndex)
					{
						dropTargetIndex--;
					}
					gridSourceList.RemoveAt(removeIndex);
				}
				else
				{
					var removeIndex = dataGrid.Items.IndexOf(dragSource);
					if (removeIndex < dropTargetIndex)
					{
						dropTargetIndex--;
					}
					dataGrid.Items.RemoveAt(removeIndex);
				}
			}

			foreach (var dragSource in dragSources)
			{
				if (gridSourceList != null)
				{
					gridSourceList.Insert(dropTargetIndex++, dragSource);
				}
				else
				{
					dataGrid.Items.Add(dropTargetIndex++);
				}
			}

			DragEnd(dataGrid);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private static void Element_MouseLeave(object sender, RoutedEventArgs e)
		{
			//コントロール要素チェック
			if (!(sender is DataGrid dataGrid))
			{
				return;
			}

			SetIsDragging(dataGrid, false);
		}

		/// <summary>
		/// Gets the enumerable as list.
		/// If enumerable is an ICollectionView then it returns the SourceCollection as list.
		/// </summary>
		/// <param name="enumerable">The enumerable.</param>
		/// <returns>Returns a list.</returns>
		private static IList TryGetList(this IEnumerable enumerable)
		{
			if (enumerable is ICollectionView)
			{
				return ((ICollectionView)enumerable).SourceCollection as IList;
			}
			else
			{
				var list = enumerable as IList;
				return list ?? (enumerable != null ? enumerable.OfType<object>().ToList() : null);
			}
		}
	}
}
