using Microsoft.WindowsAPICodePack.Dialogs;

namespace WpfMvvm.Services
{
    public class DummyFileDialogService : IFileDialogService
    {
        /// <summary>
        /// ダミー出力
        /// </summary>
        public CommonFileDialogResult DummyResult { get; set; }

        /// <summary>
        /// ダミー出力
        /// </summary>
        public string DummyFilePath { get; set; }

        public (CommonFileDialogResult result, string filename) OpenFileDialogShowDialog(string title = null, string initialDirectory = null, string filter = null)
        {
            return (DummyResult, DummyFilePath);
        }

        public (CommonFileDialogResult result, string filename) OpenFolderDialogShowDialog(string title = null, string initialDirectory = null, string filter = null)
        {
            return (DummyResult, DummyFilePath);
        }

        public (CommonFileDialogResult result, string filename) SaveFileDialogShowDialog(
            string title = null, string initialDirectory = null, string filter = null, string defaultExtension = null, string defaultFileName = null)
        {
            return (DummyResult, DummyFilePath);
        }
    }
}