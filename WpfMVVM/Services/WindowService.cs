using System;
using System.Collections.Generic;
using System.Windows;

namespace WpfMvvm.Services
{
    public class WindowService : IWindowService
    {
        /// <summary>
        /// オーナーウインドウ
        /// </summary>
        private readonly Window _owner;

        /// <summary>
        /// ViewModelをキーに対応するView(Window)を紐づけする
        /// </summary>
        private Dictionary<Type, Type> _dictvm2v;

        public WindowService(Window owner)
        {
            _owner = owner;
            _dictvm2v = new Dictionary<Type, Type>();
        }

        /// <summary>
        /// ViewModelのTypeとViewのTypeの対応関係を追加する
        /// </summary>
        /// <param name="vmType"></param>
        /// <param name="vType"></param>
        /// <returns></returns>
        public bool AddTypeDict(Type vmType, Type vType)
        {
            _dictvm2v.Add(vmType, vType);
            return true;
        }

        /// <summary>
        /// ViewModelからViewを生成する
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="viewModel"></param>
        /// <returns></returns>
        private Window CreateView<T>(T viewModel)
        {
            //ViewModel に対応する Viewを取得
            if (_dictvm2v.ContainsKey(viewModel.GetType()))
            {
                // View を生成し、DataContext に ViewModel を設定する
                var viewType = _dictvm2v[viewModel.GetType()];
                var wnd = Activator.CreateInstance(viewType) as Window;
                if (wnd != null)
                {
                    wnd.DataContext = viewModel;
                }
                return wnd;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// ViewModelからモーダルでViewを表示する
        /// </summary>
        /// <param name="dataContext"></param>
        public bool? ShowDialog(object viewModel)
        {
            var view = CreateView(viewModel);
            if (view != null)
            {
                view.Owner = _owner;
                return view.ShowDialog();
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// ViewModelからモードレスでViewを表示する
        /// </summary>
        /// <param name="dataContext"></param>
        public void Show(object viewModel)
        {
            var view = CreateView(viewModel);
            if (view != null)
            {
                view.Owner = _owner;
                view.Show();
            }
        }
    }
}