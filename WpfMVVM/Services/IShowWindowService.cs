using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfMvvm.Services
{
    interface IShowWindowService<TViewModel>
    {
        /// <summary>
        ///
        /// </summary>
        void Show(TViewModel viewModel);

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        bool? ShowDialog(TViewModel viewModel);
    }
}
