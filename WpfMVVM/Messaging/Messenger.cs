using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace WpfMvvm.Messaging
{
    /// <summary>
    /// Messengerクラス
    /// </summary>
    public class Messenger
    {
        /// <summary>
        /// シングルトンインスタンス
        /// </summary>
        private static readonly Messenger _instance = new Messenger();
        /// <summary>
        /// シングルトンインスタンス取得
        /// </summary>
        public static Messenger Default => _instance;

        private List<ActionInfo> list = new List<ActionInfo>();

        public virtual void Register<TMessage>(
            FrameworkElement recipient,
            Action<TMessage> action)
        {
            list.Add(new ActionInfo
            {
                Type = typeof(TMessage),
                sender = recipient.DataContext as INotifyPropertyChanged,
                action = action,
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TMessage"></typeparam>
        /// <param name="sender"></param>
        /// <param name="message"></param>
        public void Send<TMessage>(INotifyPropertyChanged sender, TMessage message)
        {
            //ViewModelとメッセージクラスが一致するデリゲートを取得
            var query = list.Where(o => o.sender == sender && o.Type == message.GetType())
                            .Select(o => o.action as Action<TMessage>);

            //各処理を実行
            foreach (var action in query)
            {
                action(message);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private class ActionInfo
        {
            public Type Type { get; set; }
            public INotifyPropertyChanged sender { get; set; }
            public Delegate action { get; set; }

        }
    }
}
