using Wada.SOReplaceLabel.Domain.ShopOrderAggregation;
using Wada.SOReplaceLabel.Domain.ValueObjects;

namespace Wada.SOReplaceLabel.Application.LabelPrinter;

public interface IPrintLabelUseCase
{
    Task ExecuteAsync(ShopOrderParam shopOrder);
}
public record class ShopOrderParam(string SourceAircraftModel, string LeftQuantity, string RightQuantity,
                                   string Destination, string SourceShortageSign)
{

    /// <summary>
    /// 機種
    /// </summary>
    public string SourceAircraftModel { get; } = SourceAircraftModel;

    /// <summary>
    /// 左個数
    /// </summary>
    public string LeftQuantity { get; } = LeftQuantity;

    /// <summary>
    /// 右個数
    /// </summary>
    public string RightQuantity { get; } = RightQuantity;

    /// <summary>
    /// 送り先
    /// </summary>
    public string Destination { get; } = Destination;

    /// <summary>
    /// 欠品サイン
    /// </summary>
    public string SourceShortageSign { get; } = SourceShortageSign;

    public LabelPrintContent ConvertLabelPrintContent()
    {
        return new LabelPrintContent(
            AircraftModelCode.FromSourceCode(SourceAircraftModel),
            LeftQuantity,
            RightQuantity,
            Destination,
            ShortageSign.FromSourceSign(SourceShortageSign)
        );
    }
}
