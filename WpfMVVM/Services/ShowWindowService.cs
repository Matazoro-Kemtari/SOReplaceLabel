using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace WpfMvvm.Services
{
    public class ShowWindowService<TWindow, TViewModel> : IShowWindowService<TViewModel>
        where TWindow : Window, new()
    {
        /// <summary>
        /// 所有Window指定
        /// </summary>
        public Window Owner { get; set; }

        /// <summary>
        /// ダイアログウインドウ表示
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public bool? ShowDialog(TViewModel context)
        {
            var dlg = new TWindow()
            {
                Owner = this.Owner,
                DataContext = context,
            };

            return dlg.ShowDialog();
        }

        /// <summary>
        /// ウインドウ表示
        /// </summary>
        /// <param name="context"></param>
        public void Show(TViewModel context)
        {
            var dlg = new TWindow()
            {
                Owner = this.Owner,
                DataContext = context,
            };

            dlg.Show();
        }
    }
}
