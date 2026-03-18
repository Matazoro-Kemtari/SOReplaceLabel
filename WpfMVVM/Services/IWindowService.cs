namespace WpfMvvm.Services
{
    /// <summary>
    /// VMからWindowを表示するサービス
    /// </summary>
    public interface IWindowService
    {
        /// <summary>
        ///
        /// </summary>
        void Show(object viewModel);

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        bool? ShowDialog(object viewModel);
    }
}