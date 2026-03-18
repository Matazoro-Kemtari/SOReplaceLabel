using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PackingMethodTeacher
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : MahApps.Metro.Controls.MetroWindow
    {
        /// <summary>
        /// ファイルウォッチャー
        /// </summary>
        private System.IO.FileSystemWatcher watcher;

        /// <summary>
        /// データベースファイルパス
        /// </summary>
        private readonly string dbPath;

        /// <summary>
        /// 部品梱包指示
        /// </summary>
        private SOReplaceLabelLib.Data.PartsPacking partsPacking;

        /// <summary>
        /// アプリケーションフィール
        /// </summary>
        private bool isDark;

        /// <summary>
        /// 最終更新日
        /// </summary>
        private DateTime lastChangedDate;

        public AppState State { get; private set; }

        public enum AppState
        {
            Setting,
            Watch,
            Regist,
            Update,
            Packing,
        }

        public MainWindow()
        {
            State = AppState.Setting;

            InitializeComponent();

            if (!System.IO.File.Exists(Properties.Settings.Default.WatchFilePath))
            {
                Properties.Settings.Default.Reset();
            }
            CheckFilePath.Text = Properties.Settings.Default.WatchFilePath;

            //データベースファイルパス取得
            dbPath = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(),
                                            "db",
                                            "databese.db");
        }

        /// <summary>
        /// 設定ファイル読込
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void MetroWindow_ContentRendered(object sender, EventArgs e)
        {
            Cursor = Cursors.Wait;
            maingrid.IsEnabled = false;

            //梱包方法リスト読込
            var (listLoadResult, packingSelectList) = await Task.Run(() =>
            {
                return SOReplaceLabelLib.Packing.PackingMethodReader.ReadSettingFile(@"db\梱包方法選択リスト.csv");
            });

            if(!listLoadResult)
            {
                Cursor = Cursors.Arrow;
                MessageBox.Show("梱包方法選択リストが読み込めませんでした" + Environment.NewLine + 
                                "ファイルが正しい場所に保存されているか確認してください");
                this.Close();
            }
            packing_select_ComboBox.ItemsSource = packingSelectList;

            //データベース読込
            var (dbLoadResult, messge) = await SOReplaceLabelLib.Packing.DBControler.OpenDB(dbPath);

            if(!dbLoadResult)
            {
                Cursor = Cursors.Arrow;
                MessageBox.Show("データベースが読み込めませんでした" + Environment.NewLine +
                                "ファイルが正しい場所に保存されているか確認してください");
                this.Close();
            }

            Cursor = Cursors.Arrow;
            maingrid.IsEnabled = true;
        }

        /// <summary>
        /// 監視ファイル選択
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FileChangeButton_Click(object sender, RoutedEventArgs e)
        {
            using (var openFileDialog = new Microsoft.WindowsAPICodePack.Dialogs.CommonOpenFileDialog())
            {
                openFileDialog.Title = "監視ファイル選択";
                if (System.IO.File.Exists(CheckFilePath.Text))
                {
                    openFileDialog.InitialDirectory = System.IO.Path.GetDirectoryName(CheckFilePath.Text);
                }
                if (openFileDialog.ShowDialog() != Microsoft.WindowsAPICodePack.Dialogs.CommonFileDialogResult.Ok)
                {
                    return;
                }
                CheckFilePath.Text = openFileDialog.FileName;
                Properties.Settings.Default.WatchFilePath = openFileDialog.FileName;
                Properties.Settings.Default.Save();
            }
        }

        /// <summary>
        /// 監視開始ボタン
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void WatchStartButton_Click(object sender, RoutedEventArgs e)
        {
            var directroyName = System.IO.Path.GetDirectoryName(CheckFilePath.Text);
            var fileName = System.IO.Path.GetFileName(CheckFilePath.Text);

            if (!System.IO.Directory.Exists(directroyName))
            {
                MessageBox.Show("監視対象のフォルダが存在していません");
                return;
            }

            watcher = new System.IO.FileSystemWatcher();
            //監視するディレクトリを指定
            watcher.Path = directroyName;
            //*.txtファイルを監視、すべて監視するときは""にする
            watcher.Filter = fileName;
            //ファイル名とディレクトリ名と最終書き込む日時の変更を監視
            watcher.NotifyFilter = System.IO.NotifyFilters.LastWrite;
            //サブディレクトリは監視しない
            watcher.IncludeSubdirectories = false;

            WatchStartButton.IsEnabled = false;
            WatchEndButton.IsEnabled = true;

            //変更イベント
            watcher.Changed += (ws, we) =>
            {
                //読込待ち・梱包指示以外の時は反応しない
                if(!(State == AppState.Watch || State == AppState.Packing))
                {
                    Dispatcher.BeginInvoke((Action)(() =>
                    {
                    }));
                    return;
                }

                //Changedイベントが複数発生する可能性があるため、
                //一定時間はファイル更新日が変わっても更新対応をしない
                var fi = new System.IO.FileInfo(we.FullPath);
                if (fi.LastWriteTime.Subtract(lastChangedDate).Seconds < 0.5)
                {
                    return;
                }
                lastChangedDate = fi.LastWriteTime;
                System.Threading.Thread.Sleep(500);

                Dispatcher.BeginInvoke((Action)(() =>
                {
                    //SO出力データ読込
                    var partsNo = GetParstNo(we.FullPath);

                    //問い合せ
                    var (get_result, get_message, packings) = SOReplaceLabelLib.Packing.DBControler.GetDatasByWorkNo(dbPath, partsNo);
                    if(!get_result)
                    {
                        MessageBox.Show("データベースの読込ができませんでした" + Environment.NewLine
                                        + get_message);
                        return;
                    }

                    isDark = !isDark;
                    var ph = new MaterialDesignThemes.Wpf.PaletteHelper();
                    //ph.SetLightDark(isDark);


                    if (packings.Count == 0)
                    {
                        partsPacking = new SOReplaceLabelLib.Data.PartsPacking() { PartsNo = partsNo };
                        packing_select_ComboBox.Text = string.Empty;
                        //ページ切り替え
                        ChangeTabPage(AppState.Regist);
                    }
                    else
                    {
                        partsPacking = packings.FirstOrDefault();
                        packingMethodLabel.Content = partsPacking.PackMethod;
                        //ページ切り替え
                        ChangeTabPage(AppState.Packing);
                    }
                }));
            };

            //監視を開始する
            watcher.EnableRaisingEvents = true;
            ChangeTabPage(AppState.Watch);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void WatchStopButton_Click(object sender, RoutedEventArgs e)
        {
            //監視を停止する
            watcher.EnableRaisingEvents = false;
            watcher.Dispose();
            watcher = null;

            WatchStartButton.IsEnabled = true;
            WatchEndButton.IsEnabled = false;
            PartsNoText.Text = string.Empty;

            ChangeTabPage(AppState.Setting);
        }

        /// <summary>
        /// 登録変更ボタン
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ChangeRegistorButton_Click(object sender, RoutedEventArgs e)
        {
            //選択した梱包方法を取得
            packing_select_ComboBox.Text = string.Empty;
            ChangeTabPage(AppState.Update);
        }

        /// <summary>
        /// 登録ボタン
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RegistorButton_Click(object sender, RoutedEventArgs e)
        {
            if(string.IsNullOrWhiteSpace(packing_select_ComboBox.Text))
            {
                MessageBox.Show("梱包方法を選択してください");
                return;
            }

            //選択した梱包方法を取得
            partsPacking.PackMethod = packing_select_ComboBox.Text;
            //データベース更新
            if (State == AppState.Regist)
            {
                var (result, message) = SOReplaceLabelLib.Packing.DBControler.InsertData(dbPath, partsPacking);
                if(!result)
                {
                    MessageBox.Show("データベースファイルを更新できませんでした" + Environment.NewLine + 
                                    "ファイル共有ロックの無い状態で再度更新を行ってください");
                    ChangeTabPage(AppState.Watch);
                    return;
                }
            } else if(State == AppState.Update)
            {
                var (result, message) = SOReplaceLabelLib.Packing.DBControler.UpdateData(dbPath, partsPacking);
                if (!result)
                {
                    MessageBox.Show("データベースファイルを更新できませんでした" + Environment.NewLine +
                                    "ファイル共有ロックの無い状態で再度更新を行ってください");
                    ChangeTabPage(AppState.Watch);
                    return;
                }
            }

            packingMethodLabel.Content = partsPacking.PackMethod;
            ChangeTabPage(AppState.Packing);
        }

        /// <summary>
        /// 梱包完了クリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PackingFinishButton_Click(object sender, RoutedEventArgs e)
        {
            PartsNoText.Text = string.Empty;
            ChangeTabPage(AppState.Watch);
        }

        /// <summary>
        /// ページ切り替え
        /// </summary>
        /// <param name="setState"></param>
        private void ChangeTabPage(AppState setState)
        {
            switch(setState)
            {
                case AppState.Setting:
                    TabSetting.IsSelected = true;
                    State = setState;
                    break;
                case AppState.Watch:
                    TabWatching.IsSelected = true;
                    State = setState;
                    break;
                case AppState.Regist:
                case AppState.Update:
                    TabRegister.IsSelected = true;
                    State = setState;
                    break;
                case AppState.Packing:
                    TabPacking.IsSelected = true;
                    State = setState;
                    break;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="filepath"></param>
        private string GetParstNo(string filepath)
        {
            var partsNo = string.Empty;
            var labelDelimeter = Environment.NewLine;
            //ファイルを読込み
            try
            {
                var (readresult, shopOrderText) = SOReplaceLabelLib.Data.ShopOrderReader.Read(filepath);
                if (!readresult)
                {
                    return partsNo;
                }
                else
                {
                    partsNo = shopOrderText.PartsNo;
                }

                PartsNoText.Text = partsNo;
            }
            catch (Exception)
            {
                MessageBox.Show("保存されたテキストが読み込めませんでした");
            }

            return partsNo;
        }

    }
}
