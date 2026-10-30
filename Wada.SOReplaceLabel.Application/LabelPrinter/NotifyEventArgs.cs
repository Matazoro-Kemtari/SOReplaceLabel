using System;

namespace Wada.SOReplaceLabel.Application.LabelPrinter;

/// <summary>
/// 独自の通知用オブジェクトを受け渡すイベント引数クラス
/// </summary>
/// <typeparam name="T"></typeparam>
/// <remarks>
/// 
/// </remarks>
/// <param name="notifyData"></param>
public class NotifyEventArgs<T>(T notifyData) : EventArgs
{
    /// <summary>
    /// 
    /// </summary>
    public T NotifyData { get; } = notifyData;
}
