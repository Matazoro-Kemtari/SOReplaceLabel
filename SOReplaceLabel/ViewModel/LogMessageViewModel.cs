using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SOReplaceLabel.ViewModel
{
    /// <summary>
    /// ログメッセージ
    /// </summary>
    public class LogMessageViewModel : INotifyPropertyChanged
    {
        private DateTime? _Date;
        /// <summary>
        /// 日付
        /// </summary>
        public DateTime? Date
        {
            get
            {
                return _Date;
            }
            set
            {
                if(!SetProperty(ref _Date, value))
                {
                    return;
                }
            }
        }

        private string _Message;
        /// <summary>
        /// メッセージ
        /// </summary>
        public string Message
        {
            get
            {
                return _Message;
            }
            set
            {
                if (!SetProperty(ref _Message, value))
                {
                    return;
                }
            }
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
        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName]string propertyName = null)
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

        public LogMessageViewModel()
        {
            _Date = DateTime.Now;
        }

        public LogMessageViewModel(DateTime? date, string message)
        {
            _Date = date;
            _Message = message;
        }
    }
}
