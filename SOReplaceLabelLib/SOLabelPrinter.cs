using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using StarMicronics.StarIO;
using StarMicronics.StarIOExtension;

namespace SOReplaceLabelLib;

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
    /// プリンターの接続ポート名
    /// Bluetooth接続の場合、"BT:<デバイスアドレス>" を指定します。
    /// </summary>
    public string PortName { get; set; } = "BT:Star Micronics";

    /// <summary>
    /// ポート設定 (通常は空文字で問題ありません)
    /// </summary>
    public string PortSettings { get; set; } = "";

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
    protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
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

        Thread.Sleep(300);

        //監視対象SOファイル(CSVデータ)を読込
        var (readresult, shopOrderTexts) = Data.ShopOrderReader.Read(e.FullPath);
        if (!readresult)
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
    internal void PrintLabel(string labelText)
    {
        //ラベルテキスト変更通知
        OnLabelTextUpdated(labelText);

        IPort port = null;
        try
        {
            // StarIOを使用してポートをオープン
            // Bluetoothの場合、デバイスのペアリングが完了している必要があります
            port = Factory.I.GetPort(PortName, PortSettings, 10000);

            // mC-Print3用のBuilderを作成 (Emulation.StarPRNT)
            var builder = StarIoExt.CreateCommandBuilder(Emulation.StarPRNT);

            builder.AppendInitialization(InitializationType.Command);

            builder.BeginDocument();

            // 文字サイズ設定 (縦横2倍)
            builder.AppendMultiple(2, 2);
            // 行揃え指定: 中央揃え
            builder.AppendAlignment(AlignmentPosition.Center);

            // 漢字コード設定
            builder.AppendCodePage(CodePageType.UTF8);

            builder.AppendFontStyle(FontStyleType.A);

            var setLines = labelText.Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.None);
            for (int i = 0; i < setLines.Length; i++)
            {
                var line = setLines[i];
                if (string.IsNullOrWhiteSpace(line)) continue;

                var data = Encoding.UTF8.GetBytes(line + "\n");
                switch (i)
                {
                    case 0:
                        builder.AppendMultiple(data, 6, 5);
                        break;
                    case 1:
                        builder.AppendMultiple(data,
                            line.Length switch
                            {
                                > 6 => 2,
                                _ => 4,
                            }, 3);
                        break;
                    case 2:
                        builder.AppendMultiple(data, 5, 5);
                        break;
                    default:
                        builder.AppendMultiple(1, 1);
                        break;
                }
            }

            // 文字サイズと位置を元に戻す
            builder.AppendMultiple(1, 1);
            builder.AppendAlignment(AlignmentPosition.Left);

            // カット
            builder.AppendCutPaper(CutPaperAction.PartialCutWithFeed);

            builder.EndDocument();

            // データの送信
            byte[] commands2 = builder.Commands;
            port.WritePort(commands2, 0, (uint)commands2.Length);

            OnNotifyLogMessage(new[] { "印刷成功: " + PortName });
        }
        catch (PortException ex)
        {
            // ポートオープンに失敗したとき
            // 誤ったportNameを設定したとき
            if (ex.ErrorCode == StarResultCode.ErrorFailed)
            {
                // 何らかのエラーが発生したとき
                OnNotifyErrorMessage(
                    "ラベルプリンタで、原因不明のエラーがありました");
            }
            else if (ex.ErrorCode == StarResultCode.ErrorInUse)
            {
                // プリンターから接続拒否されたとき（他ホストが接続中など）
                OnNotifyErrorMessage(
                    "ラベルプリンタが使用中です");
            }
            else
            {
                OnNotifyErrorMessage(
                    "システムで致命的なエラーがありました\n" +
                    $"Error: {ex.Message}");
            }
        }
        finally
        {
            Factory.I.ReleasePort(port);
        }
    }
}
