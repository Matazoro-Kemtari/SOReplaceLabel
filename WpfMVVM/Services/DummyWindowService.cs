namespace WpfMvvm.Services
{
    public class DummyWindowService : IWindowService
    {
        /// <summary>
        /// ダミーデータ
        /// </summary>
        public bool? DummyResult { get; set; }

        public void Show(object viewModel)
        {
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public bool? ShowDialog(object viewModel)
        {
            return DummyResult;
        }
    }
}