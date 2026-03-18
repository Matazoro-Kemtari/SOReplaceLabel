using System;
using System.Windows;

namespace WpfMvvm.Behavior
{
    /// <summary>
    /// ファイルのDrap&Drop処理ビヘイビア
    /// </summary>
    public static class FileDragDropBehavior
    {
        #region IsAttached 添付プロパティ定義

        public static readonly DependencyProperty IsAttachedProperty
            = DependencyProperty.RegisterAttached(
                "IsAttached",
                typeof(bool),
                typeof(FileDragDropBehavior),
                new FrameworkPropertyMetadata(false, OnIsAttachedChanged));

        public static bool GetIsAttached(DependencyObject dependencyObject)
        {
            if (dependencyObject == null)
            {
                throw new ArgumentNullException(nameof(dependencyObject));
            }
            return (bool)dependencyObject.GetValue(IsAttachedProperty);
        }

        public static void SetIsAttached(DependencyObject dependencyObject, bool value)
        {
            if (dependencyObject == null)
            {
                throw new ArgumentNullException(nameof(dependencyObject));
            }
            dependencyObject.SetValue(IsAttachedProperty, value);
        }

        /// <summary>
        /// IsAttached 添付プロパティ値変更イベントハンドラ
        /// </summary>
        /// <param name="d">イベント発生元</param>
        /// <param name="e">イベント引数</param>
        private static void OnIsAttachedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is UIElement element))
            {
                return;
            }

            if (GetIsAttached(element))
            {
                element.PreviewDragOver += Element_PreviewDragOver;
                element.PreviewDrop += Element_PreviewDrop;
            }
            else
            {
                element.PreviewDragOver -= Element_PreviewDragOver;
                element.PreviewDrop -= Element_PreviewDrop;
            }
        }

        /// <summary>
        /// PreviewDragEnter
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void Element_PreviewDragOver(object sender, DragEventArgs e)
        {
            //無効データチェック
            if (!IsValidDrop(sender as DependencyObject, e))
            {
                e.Effects = DragDropEffects.None;
                e.Handled = true;
                return;
            }

            //CallBack
            var previewDragOverCommand = GetPreviewDragOverCommand(sender as DependencyObject);
            if (previewDragOverCommand?.CanExecute(e) ?? false)
            {
                previewDragOverCommand?.Execute(e);
            }

            e.Effects = DragDropEffects.Copy;
            e.Handled = true;
        }

        /// <summary>
        /// 有効なファイルドロップかを判定する
        /// </summary>
        /// <returns></returns>
        private static bool IsValidDrop(DependencyObject dependencyObject, DragEventArgs e)
        {
            return IsUIElement(dependencyObject)
                && (MuitiDropIsValid(dependencyObject, e) ?? true)
                && DragFileIsValid(dependencyObject, e);
        }

        /// <summary>
        /// ドラッグ先がWPFのUIコントロールか判定
        /// </summary>
        /// <param name="checkObject"></param>
        /// <returns></returns>
        private static bool IsUIElement(object checkObject)
        {
            return checkObject is UIElement;
        }

        /// <summary>
        /// 複数ファイルDropの有効性を判定
        /// </summary>
        /// <param name="dependencyObject"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        private static bool? MuitiDropIsValid(DependencyObject dependencyObject, DragEventArgs e)
        {
            //ファイルドラッグか判断
            if (e == null
                || e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                return null;
            }

            //複数ファイルドラッグ有効時はファイル数に依存しない
            if (GetMultiDrop(dependencyObject))
            {
                return true;
            }

            //複数ファイル無効時は１つ以下のファイルしたドラッグを有効にしない
            var filePathes = e.Data.GetData(DataFormats.FileDrop) as string[];
            return filePathes.Length <= 1;
        }

        /// <summary>
        /// Dropファイルの有効を判定
        /// </summary>
        /// <param name="dependencyObject"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        private static bool DragFileIsValid(DependencyObject dependencyObject, DragEventArgs e)
        {
            var filePathes = e.Data.GetData(DataFormats.FileDrop) as string[];

            //ファイルが有効かを判定（検証用メソッドが定義されていない時は有効）
            var fileIsValidFunc = GetFileIsValidFunc(dependencyObject);
            return fileIsValidFunc?.Invoke(filePathes) ?? true;
        }

        /// <summary>
        /// PreviewDrop
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void Element_PreviewDrop(object sender, DragEventArgs e)
        {
            if (!(sender is UIElement element))
            {
                return;
            }

            var filePathes = e.Data.GetData(DataFormats.FileDrop) as string[];

            //CallBack
            var previewDropCommand = GetPreviewDropCommand(element);
            if (previewDropCommand?.CanExecute(filePathes) ?? false)
            {
                previewDropCommand?.Execute(filePathes);
            }
        }

        #endregion IsAttached 添付プロパティ定義

        #region MultiDrop 添付プロパティ定義

        public static readonly DependencyProperty MultiDropProperty
            = DependencyProperty.RegisterAttached(
                "MultiDrop",
                typeof(bool),
                typeof(FileDragDropBehavior),
                new FrameworkPropertyMetadata(true));

        public static bool GetMultiDrop(DependencyObject dependencyObject)
        {
            if (dependencyObject == null)
            {
                throw new ArgumentNullException(nameof(dependencyObject));
            }
            return (bool)dependencyObject.GetValue(MultiDropProperty);
        }

        public static void SetMultiDrop(DependencyObject dependencyObject, bool value)
        {
            if (dependencyObject == null)
            {
                throw new ArgumentNullException(nameof(dependencyObject));
            }
            dependencyObject.SetValue(MultiDropProperty, value);
        }

        #endregion MultiDrop 添付プロパティ定義

        #region FileIsValidFunc 添付プロパティ定義

        public static Func<string[], bool> GetFileIsValidFunc(DependencyObject dependencyObject)
        {
            if (dependencyObject == null)
            {
                throw new System.ArgumentNullException(nameof(dependencyObject));
            }
            return (Func<string[], bool>)dependencyObject.GetValue(FileIsValidFuncProperty);
        }

        public static void SetFileIsValidFunc(DependencyObject dependencyObject, Func<string[], bool> value)
        {
            if (dependencyObject == null)
            {
                throw new System.ArgumentNullException(nameof(dependencyObject));
            }
            dependencyObject.SetValue(FileIsValidFuncProperty, value);
        }

        // Using a DependencyProperty as the backing store for FileIsValidFunc.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FileIsValidFuncProperty =
            DependencyProperty.RegisterAttached(
                "FileIsValidFunc",
                typeof(Func<string[], bool>),
                typeof(FileDragDropBehavior),
                new PropertyMetadata(null));

        #endregion FileIsValidFunc 添付プロパティ定義

        #region PreviewDragOverCommand 添付プロパティ定義

        public static readonly DependencyProperty PreviewDragOverCommandProperty
            = DependencyProperty.RegisterAttached(
                "PreviewDragOverCommand",
                typeof(DelegateCommand<DragEventArgs>),
                typeof(FileDragDropBehavior),
                new FrameworkPropertyMetadata(null));

        public static DelegateCommand<DragEventArgs> GetPreviewDragOverCommand(DependencyObject dependencyObject)
        {
            if (dependencyObject == null)
            {
                throw new ArgumentNullException(nameof(dependencyObject));
            }
            return (DelegateCommand<DragEventArgs>)dependencyObject.GetValue(PreviewDragOverCommandProperty);
        }

        public static void SetPreviewDragOverCommand(DependencyObject dependencyObject, DelegateCommand<DragEventArgs> value)
        {
            if (dependencyObject == null)
            {
                throw new ArgumentNullException(nameof(dependencyObject));
            }
            dependencyObject.SetValue(PreviewDragOverCommandProperty, value);
        }

        #endregion PreviewDragOverCommand 添付プロパティ定義

        #region PreviewDropCommand 添付プロパティ定義

        public static readonly DependencyProperty PreviewDropCommandProperty
            = DependencyProperty.RegisterAttached(
                "PreviewDropCommand",
                typeof(DelegateCommand<string[]>),
                typeof(FileDragDropBehavior),
                new FrameworkPropertyMetadata(null));

        public static DelegateCommand<string[]> GetPreviewDropCommand(DependencyObject dependencyObject)
        {
            if (dependencyObject == null)
            {
                throw new ArgumentNullException(nameof(dependencyObject));
            }
            return (DelegateCommand<string[]>)dependencyObject.GetValue(PreviewDropCommandProperty);
        }

        public static void SetPreviewDropCommand(DependencyObject dependencyObject, DelegateCommand<string[]> value)
        {
            if (dependencyObject == null)
            {
                throw new ArgumentNullException(nameof(dependencyObject));
            }
            dependencyObject.SetValue(PreviewDropCommandProperty, value);
        }

        #endregion PreviewDropCommand 添付プロパティ定義
    }
}