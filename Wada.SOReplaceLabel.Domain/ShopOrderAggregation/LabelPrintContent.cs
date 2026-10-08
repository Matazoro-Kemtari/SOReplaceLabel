using Wada.SOReplaceLabel.Domain.ValueObjects;

namespace Wada.SOReplaceLabel.Domain.ShopOrderAggregation;

public class LabelPrintContent(
    AircraftModelCode aircraftModel,
    string? leftQuantity,
    string? rightQuantity,
    string? destination,
    ShortageSign shortage)
{
    public AircraftModelCode AircraftModel { get; } = aircraftModel ?? throw new ArgumentNullException(nameof(aircraftModel));
    public string LeftQuantity { get; } = leftQuantity ?? string.Empty;
    public string RightQuantity { get; } = rightQuantity ?? string.Empty;
    public string Destination { get; } = destination ?? string.Empty;
    public ShortageSign Shortage { get; } = shortage ?? throw new ArgumentNullException(nameof(shortage));

    // 印字用にフォーマットされた文字列を取得（プリンターへの送信形式）
    public string[] ToPrintArray()
    {
        return
        [
            AircraftModel.PrintValue,
            LeftQuantity,
            RightQuantity,
            Destination,
            Shortage.PrintValue,
        ];
    }
}
