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
    public int LeftQuantity { get; } = int.TryParse(leftQuantity, out var result) ? result : 0;
    public int RightQuantity { get; } = int.TryParse(rightQuantity, out var result) ? result : 0;
    public string Destination { get; } = destination ?? string.Empty;
    public ShortageSign Shortage { get; } = shortage ?? throw new ArgumentNullException(nameof(shortage));
}
