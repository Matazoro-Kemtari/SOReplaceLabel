using Microsoft.WindowsAPICodePack.Dialogs;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace WpfMvvm.Services
{
    /// <summary>
    ///
    /// </summary>
    public class FileDialogService : IFileDialogService
    {
        /// <summary>
        /// オーナーウインドウ
        /// </summary>
        private readonly Window _owner;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="owner"></param>
        public FileDialogService(Window owner = null)
        {
            _owner = owner;
        }

        /// <summary>
        /// フィルタを作成
        /// </summary>
        /// <param name="filterStr"></param>
        /// <returns></returns>
        private static IEnumerable<CommonFileDialogFilter> CreateDialogFilter(string filterStr)
        {
            if (!string.IsNullOrWhiteSpace(filterStr))
            {
                //OpenFileDialog形式に従いフィルタを作成
                var splitFilter = filterStr.Split('|');
                //パターン形式かどうかの判定を'|'で区切られた項目が偶数で判定
                if (splitFilter.Length > 0 && splitFilter.Length % 2 == 0)
                {
                    //分割された配列を2個ずつ取得
                    var chunks = splitFilter.Select((value, index) => new { Value = value, Index = index })
                                    .GroupBy(item => item.Index / 2)
                                    .Select(g => g.Select(item => item.Value).ToArray());
                    //前半部を表記名、後半部を拡張子としてフィルタに変換
                    var filter = chunks.Select(chunk => new CommonFileDialogFilter(chunk[0], chunk[1]));
                    return filter;
                }
            }

            return System.Array.Empty<CommonFileDialogFilter>();
        }

        /// <summary>
        /// ファイル選択ダイアログ
        /// </summary>
        /// <returns></returns>
        public (CommonFileDialogResult result, string filename)
            OpenFileDialogShowDialog(string title = null,
                                     string initialDirectory = null,
                                     string filter = null)
        {
            //ダイアログ作成
            using (var openFileDialog = new CommonOpenFileDialog())
            {
                //初期化
                openFileDialog.Title = title;
                openFileDialog.InitialDirectory = initialDirectory;
                //フィルタ設定
                openFileDialog.Filters.Clear();
                foreach (var dialogFilter in CreateDialogFilter(filter))
                {
                    openFileDialog.Filters.Add(dialogFilter);
                }

                //ダイアログを表示
                var dialogResult = (_owner == null) ? openFileDialog.ShowDialog()
                                                    : openFileDialog.ShowDialog(_owner);

                //結果とパスを返す
                if (dialogResult == CommonFileDialogResult.Ok)
                {
                    return (dialogResult, openFileDialog.FileName);
                }
                else
                {
                    return (dialogResult, null);
                }
            }
        }

        /// <summary>
        /// フォルダ選択ダイアログ
        /// </summary>
        /// <param name="title"></param>
        /// <param name="initialDirectory"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        public (CommonFileDialogResult result, string filename)
            OpenFolderDialogShowDialog(string title = null,
                                       string initialDirectory = null,
                                       string filter = null)
        {
            //ダイアログ作成
            using (var openFileDialog = new CommonOpenFileDialog())
            {
                //初期化
                openFileDialog.Title = title;
                openFileDialog.InitialDirectory = initialDirectory;
                //フィルタ設定
                openFileDialog.Filters.Clear();
                foreach (var dialogFilter in CreateDialogFilter(filter))
                {
                    openFileDialog.Filters.Add(dialogFilter);
                }
                //フォルダ選択有効
                openFileDialog.IsFolderPicker = true;
                //ダイアログを表示
                var dialogResult = (_owner == null) ? openFileDialog.ShowDialog()
                                                    : openFileDialog.ShowDialog(_owner);

                //結果とパスを返す
                if (dialogResult == CommonFileDialogResult.Ok)
                {
                    return (dialogResult, openFileDialog.FileName);
                }
                else
                {
                    return (dialogResult, null);
                }
            }
        }

        /// <summary>
        /// ファイル保存ダイアログ
        /// </summary>
        /// <param name="title"></param>
        /// <param name="initialDirectory"></param>
        /// <param name="filter"></param>
        /// <param name="defaultExtension"></param>
        /// <returns></returns>
        public (CommonFileDialogResult result, string filename)
            SaveFileDialogShowDialog(string title = null,
                                     string initialDirectory = null,
                                     string filter = null,
                                     string defaultExtension = null,
                                     string defaultFileName = null)
        {
            //ダイアログ作成
            using (var saveFileDialog = new CommonSaveFileDialog())
            {
                //初期化
                saveFileDialog.Title = title;
                saveFileDialog.InitialDirectory = initialDirectory;
                //フィルタ設定
                saveFileDialog.Filters.Clear();
                foreach (var dialogFilter in CreateDialogFilter(filter))
                {
                    saveFileDialog.Filters.Add(dialogFilter);
                }
                //拡張子
                saveFileDialog.DefaultExtension = defaultExtension;
                //保存ファイル名
                saveFileDialog.DefaultFileName = defaultFileName;

                //ダイアログを表示
                var dialogResult = (_owner == null) ? saveFileDialog.ShowDialog()
                                                    : saveFileDialog.ShowDialog(_owner);

                //結果とパスを返す
                if (dialogResult == CommonFileDialogResult.Ok)
                {
                    return (dialogResult, saveFileDialog.FileName);
                }
                else
                {
                    return (dialogResult, null);
                }
            }
        }
    }
}