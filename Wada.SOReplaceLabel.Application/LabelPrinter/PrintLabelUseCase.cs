using Wada.SOReplaceLabel.Application.CsvData;
using Wada.SOReplaceLabel.Domain;

namespace Wada.SOReplaceLabel.Application.LabelPrinter;

public class PrintLabelUseCase(ILabelPrinter labelPrinter) : IPrintLabelUseCase
{
    /// <summary>
    /// ラベル印刷する
    /// </summary>
    /// <param name="content">タブ区切りのShopOrder</param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public async Task ExecuteAsync(string content)
    {
        if (content is null) throw new ArgumentNullException(nameof(content));

        var shopOrder = ShopOrderReader.Read(content);

        await labelPrinter.PrintAsync(shopOrder.ConvertLabelPrintContent());
    }
}
