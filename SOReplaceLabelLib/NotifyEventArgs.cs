using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SOReplaceLabelLib
{
    /// <summary>
    /// 独自の通知用オブジェクトを受け渡すイベント引数クラス
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class NotifyEventArgs<T> : EventArgs
    {
        /// <summary>
        /// 
        /// </summary>
        public T NotifyData { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="notifyData"></param>
        public NotifyEventArgs(T notifyData)
        {
            NotifyData = notifyData;
        }
    }
}
