using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace SOReplaceLabel.ViewModel
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        /// <summary>
        /// 監視フォルダパス
        /// </summary>
        private string _WatchFilePath;
        /// <summary>
        /// 監視フォルダパス
        /// </summary>
        public string WatchFilePath
        {
            get
            {
                return _WatchFilePath;
            }
            set
            {
                if(!SetProperty(ref _WatchFilePath, value))
                {
                    return;
                }
            }
        }

        private string _LabelPreviewText;
        /// <summary>
        /// ラベルプレビューデータ
        /// </summary>
        public string LabelPreviewText
        {
            get
            {
                return _LabelPreviewText;
            }
            set
            {
                if (!SetProperty(ref _LabelPreviewText, value))
                {
                    return;
                }
            }
        }

        public bool IsFlieWatching
        {
            get
            {
                return _SOLabelPrinter?.IsFileWatching ?? false;
            }
        }

        /// <summary>
        /// SO読込ラベルプリンタ制御クラス
        /// </summary>
        private SOReplaceLabelLib.SOLabelPrinter _SOLabelPrinter;

        /// <summary>
        /// ログメッセージ
        /// </summary>
        public ObservableCollection<LogMessageViewModel> LogMessages { get; set; }

        /// <summary>
        /// ファイル選択コマンド
        /// </summary>
        public WpfMvvm.DelegateCommand SelectWatcherFileCommand { get; }

        /// <summary>
        /// ファイル監視開始コマンド
        /// </summary>
        public WpfMvvm.DelegateCommand StartWatcherCommand { get; }

        /// <summary>
        /// ファイル監視開始コマンド
        /// </summary>
        public WpfMvvm.DelegateCommand StopWatcherCommand { get; }


        /// <summary>
        /// メッセージダイアログ表示
        /// </summary>
        private readonly WpfMvvm.Services.IDialogService _dialogService;

        /// <summary>
        /// ファイル選択ダイアログ表示
        /// </summary>
        private readonly WpfMvvm.Services.IFileDialogService _fileDialogService;

        #region INotifyPropertyChanged
        /// <summary>
        /// プロパティ変更通知イベント
        /// </summary>
        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// PropertyChangedイベントを発生させる
        /// </summary>
        /// <param name="propertyName"></param>
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// プロパティに値をセットする。
        /// 値が変更されない場合は
        /// PropertyChanged通知イベントを行わない
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="field"></param>
        /// <param name="value"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName]string propertyName = null)
        {
            if (Equals(field, value))
            {
                return false;
            }

            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
        #endregion

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public MainWindowViewModel(
            WpfMvvm.Services.IDialogService dialogService,
            WpfMvvm.Services.IFileDialogService fileDialogService)
        {
            if (System.IO.File.Exists(Properties.Settings.Default.WatchFilePath))
            {
                WatchFilePath = Properties.Settings.Default.WatchFilePath;
                CreateSOLabelPrinter();
            }
            else
            {
                Properties.Settings.Default.Reset();
            }

            LogMessages = new ObservableCollection<LogMessageViewModel>();

            //Command作成
            SelectWatcherFileCommand = new WpfMvvm.DelegateCommand(SelectWatcherFile, () => !IsFlieWatching);
            StartWatcherCommand = new WpfMvvm.DelegateCommand(StartWatcher, CanStartWatcher);
            StopWatcherCommand = new WpfMvvm.DelegateCommand(StopWatcher, () => IsFlieWatching);

            //サービス処理を外部注入
            _dialogService = dialogService;
            _fileDialogService = fileDialogService;
        }

        /// <summary>
        /// 監視用ファイル選択処理
        /// </summary>
        private void SelectWatcherFile()
        {
            //ダイアログ表示初期位置を設定したファイルの存在するディレクトリに指定
            var initialDir = System.IO.File.Exists(WatchFilePath) ? WatchFilePath : null;
            //ファイル選択ダイアログ表示
            var (result, filepath) = _fileDialogService.OpenFileDialogShowDialog("監視ファイル選択", initialDir);
            if(result != Microsoft.WindowsAPICodePack.Dialogs.CommonFileDialogResult.Ok)
            {
                return;
            }
            //監視対象ファイルパス表示変更
            WatchFilePath = filepath;
            //アプリケーション設定値保存
            Properties.Settings.Default.WatchFilePath = WatchFilePath;
            Properties.Settings.Default.Save();
            //監視クラス初期化
            CreateSOLabelPrinter();
        }

        /// <summary>
        /// ファイル監視開始
        /// </summary>
        private void StartWatcher()
        {
            //ログ消去
            LogMessages.Clear();
            //監視開始
            _SOLabelPrinter?.StartWatcher();
        }

        /// <summary>
        /// ファイル監視コマンド実行可能状態を取得
        /// </summary>
        /// <returns></returns>
        private bool CanStartWatcher()
        {
            //監視対象ファイルが存在し、監視中でないとき有効
            return System.IO.File.Exists(WatchFilePath) && !IsFlieWatching;
        }

        /// <summary>
        /// ファイル監視停止コマンド
        /// </summary>

        private void StopWatcher()
        {
            _SOLabelPrinter?.StopWatcher();
        }

        /// <summary>
        ///  SO読込ラベルプリンタ制御クラス初期化
        /// </summary>
        private void CreateSOLabelPrinter()
        {
            _SOLabelPrinter = new SOReplaceLabelLib.SOLabelPrinter(WatchFilePath);

            //ログメッセージ追加通知イベント
            _SOLabelPrinter.NotifyLogMessage += (s, e) =>
            {
                //UIスレッド実行
                WpfMvvm.DispatcherHelper.CheckInvokeOnUI(() => InsertLogMessages(e.NotifyData));
            };

            //エラーメッセージ表示通知イベント
            _SOLabelPrinter.NotifyErrorMessage += (s, e) =>
            {
                //UIスレッド実行
                WpfMvvm.DispatcherHelper.CheckInvokeOnUI(() => ShowErrorMessage(e.NotifyData));
            };

            //ラベルテキスト更新通知イベント
            _SOLabelPrinter.LabelTextUpdated += (s, e) =>
            {
                //UIスレッド実行
                WpfMvvm.DispatcherHelper.CheckInvokeOnUI(() =>
                {
                    LabelPreviewText = e.NotifyData;
                });
            };
        }

        /// <summary>
        /// ログメッセージ表示（要UIスレッド実行）
        /// </summary>
        /// <param name="messages"></param>
        private void InsertLogMessages(IList<string> messages)
        {
            for (int index = 0; index < messages.Count; index++)
            {
                LogMessages.Insert(index, new LogMessageViewModel(index == 0 ? DateTime.Now : (DateTime?)null, messages[index]));
            }

            //ログの表示数が最大値以上となったとき古いデータを消去
            while (LogMessages.Count > 1000)
            {
                LogMessages.RemoveAt(1000);
            }
        }

        /// <summary>
        /// ログメッセージ表示（要UIスレッド実行）
        /// </summary>
        /// <param name="messages"></param>
        private void ShowErrorMessage(string message)
        {
            _dialogService.Show(message, "エラー", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
        }
    }
}
