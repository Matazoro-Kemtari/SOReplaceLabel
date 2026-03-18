using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SOReplaceLabelLib
{
    /// <summary>
    /// SO読込、ラベルプリンタ制御クラス
    /// </summary>
    public class SOLabelPrinter : INotifyPropertyChanged
    {
        /// <summary>
        /// ファイルウオッチャー
        /// </summary>
        private readonly System.IO.FileSystemWatcher watcher;

        /// <summary>
        /// 最終変更時刻
        /// ファイル書き込み時にFileSystemWatcherのChangedイベントが複数回発生するため、
        /// 最終変更時刻から一定時間はChangedイベントが発生しても次の印刷処理へ行かないようにする
        /// </summary>
        public DateTime LastChangedDateTime { get; private set; }

        /// <summary>
        /// ファイルウォッチャー動作中フラグ
        /// </summary>
        public bool IsFileWatching
        {
            get
            {
                return watcher.EnableRaisingEvents;
            }
            private set
            {
                //値セット
                watcher.EnableRaisingEvents = value;
                //ファイルウォッチャー動作状態変更通知
                OnPropertyChanged("IsFileWatching");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public SOLabelPrinter(string watchFilePath)
        {
            //引数で渡したファイルの保存フォルダの存在を確認する
            if (watchFilePath == null)
            {
                throw new ArgumentNullException(nameof(watchFilePath));
            }
            var directroyName = System.IO.Path.GetDirectoryName(watchFilePath);
            var fileName = System.IO.Path.GetFileName(watchFilePath);
            if (!System.IO.Directory.Exists(directroyName))
            {
                throw new System.IO.DirectoryNotFoundException(directroyName);
            }

            //引数で渡したファイルの更新を監視する
            watcher = new System.IO.FileSystemWatcher();
            //監視するディレクトリとファイルを指定
            watcher.Path = directroyName;
            watcher.Filter = fileName;
            //最終書き込み日時の変更が発生したときにイベントを通知させる
            watcher.NotifyFilter = System.IO.NotifyFilters.LastWrite;
            //サブディレクトリは監視しない
            watcher.IncludeSubdirectories = false;
            //最終変更時刻初期化
            LastChangedDateTime = DateTime.Now;
            ///変更検知イベント処理登録
            watcher.Changed += Watcher_Changed;
        }

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
        /// メッセージ通知イベントハンドラ
        /// </summary>
        [field: NonSerialized]
        public event EventHandler<NotifyEventArgs<IList<string>>> NotifyLogMessage;

        /// <summary>
        /// NotifyLogMessageイベントを発生させる
        /// </summary>
        /// <param name="propertyName"></param>
        protected void OnNotifyLogMessage(IList<string> messages)
        {
            NotifyLogMessage?.Invoke(this, new NotifyEventArgs<IList<string>>(messages));
        }

        /// <summary>
        /// エラーメッセージ通知イベントハンドラ
        /// </summary>
        [field: NonSerialized]
        public event EventHandler<NotifyEventArgs<string>> NotifyErrorMessage;

        /// <summary>
        /// NotifyLogMessageイベントを発生させる
        /// </summary>
        /// <param name="propertyName"></param>
        protected void OnNotifyErrorMessage(string message)
        {
            NotifyErrorMessage?.Invoke(this, new NotifyEventArgs<string>(message));
        }

        /// <summary>
        /// エラーメッセージ通知イベントハンドラ
        /// </summary>
        [field: NonSerialized]
        public event EventHandler<NotifyEventArgs<string>> LabelTextUpdated;

        /// <summary>
        /// NotifyLogMessageイベントを発生させる
        /// </summary>
        /// <param name="propertyName"></param>
        protected void OnLabelTextUpdated(string labelText)
        {
            LabelTextUpdated?.Invoke(this, new NotifyEventArgs<string>(labelText));
        }

        /// <summary>
        /// ファイル監視開始
        /// </summary>
        public void StartWatcher()
        {
            //ファイルウォッチャー動作状態変更
            IsFileWatching = true;
            //ログメッセージ通知
            OnNotifyLogMessage(new string[] { "ファイル監視開始" });
        }

        /// <summary>
        /// ファイル監視開始
        /// </summary>
        public void StopWatcher()
        {
            //ファイルウォッチャー動作状態変更
            IsFileWatching = false;
            //ログメッセージ通知
            OnNotifyLogMessage(new string[] { "ファイル監視終了" });
        }

        /// <summary>
        /// ファイル変更イベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Watcher_Changed(object sender, System.IO.FileSystemEventArgs e)
        {
            //変更のあったファイルの更新時刻を取得し、最終更新時刻との差分が一定以下のとき処理を行わない
            var fi = new System.IO.FileInfo(e.FullPath);
            if (fi.LastWriteTime.Subtract(LastChangedDateTime).Seconds < 1.0)
            {
                return;
            }
            LastChangedDateTime = fi.LastWriteTime;

            //監視対象SOファイル(CSVデータ)を読込
            var (readresult, shopOrderTexts) = Data.ShopOrderReader.Read(e.FullPath);
            if(!readresult)
            {
                OnNotifyLogMessage(new string[] { "【読込結果】", "テキストが読み込めませんでした" });
                return;
            }
            //読取結果をログ表示
            OnNotifyLogMessage(new string[] { "【読込結果】" }.Concat(shopOrderTexts.GetTextsWithColumnNumber()).ToList());

            //ラベル出力用テキスト作成
            var labelText = shopOrderTexts.GetLabelLineTexts();
            //プリンタ印刷処理
            PrintLabel(labelText);
        }

        /// <summary>
        /// ラベルプリンタ印刷
        /// </summary>
        /// <param name="labelText"></param>
        private void PrintLabel(string labelText)
        {
            //ラベルテキスト変更通知
            OnLabelTextUpdated(labelText);

            //ラベルプリンタ接続確認
            var doc = new bpac.Document();
            if (!doc.Printer.IsPrinterOnline(doc.Printer.Name))
            {
                OnNotifyErrorMessage("プリンタが接続されていません");
                return;
            }

            //テンプレートファイルを開き、読込んだデータを挿入
            var readFilePath = @"lbx\ラベルテンプレート.lbx";
            if (doc.Open(readFilePath))
            {
                //改行単位にラベルプリンタのオブジェクトへデータを挿入する
                var setLines = labelText.Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.None);
                for (int rowNo = 1; rowNo <= 3; rowNo++)
                {
                    var obj_name = "obj_row" + rowNo;
                    var obj_row = doc.GetObject(obj_name);
                    if (obj_row == null)
                    {
                        OnNotifyErrorMessage(
                            "ラベルテンプレートファイル内に、テキスト入力用オブジェクトが定義されていません" + Environment.NewLine +
                            "オブジェクト名 : " + obj_name);
                        return;
                    }
                    if (rowNo - 1 < setLines.Length)
                    {
                        obj_row.Text = setLines[rowNo - 1];
                    }
                }

                //ラベルプリンタ印刷
                doc.SetMediaById(doc.Printer.GetMediaId(), true);
                doc.StartPrint("", bpac.PrintOptionConstants.bpoDefault);
                doc.PrintOut(1, bpac.PrintOptionConstants.bpoDefault);
                doc.EndPrint();
                doc.Close();
            }
            else
            {
                OnNotifyErrorMessage(
                    "ラベルテンプレートファイルが読み込めませんでした" + Environment.NewLine +
                    "Open() Error: " + doc.ErrorCode);
            }
        }
    }
}
