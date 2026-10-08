using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;

namespace Wada.SOReplaceLabel.Application.CsvData;

internal class ShopOrderReader
{
    /// <summary>
    /// タブ区切りのCSVデータを読み込み、ShopOrderModelに変換する
    /// </summary>
    /// <param name="content">タブ区切りのShopOrder</param>
    /// <returns></returns>
    public static ShopOrderCsvRecord Read(string content)
    {
        var config = new CsvConfiguration(new CultureInfo("ja-JP", false))
        {
            HasHeaderRecord = false,        // ヘッダ無（デフォルトtrue）
            Delimiter = "\t",               // 区切り文字（デフォルト,）
            TrimOptions = TrimOptions.Trim, // 両端の空白を削除（デフォルトTrimOptions.None）
        };

        using var reader = new StringReader(content);
        using var csv = new CsvReader(reader, config);
        var shopOrderTexts = csv.GetRecords<ShopOrderCsvRecord>().FirstOrDefault();

        return shopOrderTexts;
    }
}
