namespace Wada.SOReplaceLabel.Domain.ValueObjects;

public record class AircraftModelCode
{
    private AircraftModelCode(string printValue) => PrintValue = printValue;

    public string PrintValue { get; }

    public static AircraftModelCode FromSourceCode(string? sourceCode)
    {
        var converted = sourceCode?.Trim() switch
        {
            "FP1" => "777",
            "FP3" => "737",
            "FPB" => "747",
            "FPD" => "787",
            "T" => "767",
            "FPC" => "MRJ",
            _ => sourceCode ?? string.Empty // 上記以外はそのまま出力
        };
        return new AircraftModelCode(converted);
    }
}
