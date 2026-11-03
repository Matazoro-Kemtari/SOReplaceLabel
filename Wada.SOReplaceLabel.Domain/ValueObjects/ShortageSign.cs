namespace Wada.SOReplaceLabel.Domain.ValueObjects;

public record class ShortageSign
{
    private ShortageSign(string printValue) => PrintValue = printValue;

    public string PrintValue { get; }

    public static ShortageSign FromSourceSign(string? sourceSign)
    {
        var converted = sourceSign?.Trim() switch
        {
            "1" => "欠品",
            "0" or "2" or "3" => "",
            _ => "エラー(欠品サイン)",
        };
        return new ShortageSign(converted);
    }
}
