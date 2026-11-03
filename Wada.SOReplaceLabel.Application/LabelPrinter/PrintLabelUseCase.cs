using Wada.SOReplaceLabel.Domain;

namespace Wada.SOReplaceLabel.Application.LabelPrinter;

public class PrintLabelUseCase(ILabelPrinter labelPrinter) : IPrintLabelUseCase
{
    /// <summary>
    /// ラベル印刷する
    /// </summary>
    /// <param name="shopOrder">ShopOrder</param>
    /// <returns></returns>
    public async Task ExecuteAsync(ShopOrderParam shopOrder)
    {
        if (shopOrder is null) throw new ArgumentNullException(nameof(shopOrder));

        await labelPrinter.PrintAsync(shopOrder.ConvertLabelPrintContent());
    }
}
