using Microsoft.WindowsAPICodePack.Dialogs;

namespace WpfMvvm.Services
{
    /// <summary>
    /// VMからファイルダイアログを表示するサービス
    /// 参考
    /// http://sourcechord.hatenablog.com/entry/2016/01/23/170753
    /// </summary>
    public interface IFileDialogService
    {
        (CommonFileDialogResult result, string filename) OpenFileDialogShowDialog(string title = null, string initialDirectory = null, string filter = null);

        (CommonFileDialogResult result, string filename) OpenFolderDialogShowDialog(string title = null, string initialDirectory = null, string filter = null);

        (CommonFileDialogResult result, string filename) SaveFileDialogShowDialog(
            string title = null, string initialDirectory = null, string filter = null, string defaultExtension = null, string defaultFileName = null);
    }
}