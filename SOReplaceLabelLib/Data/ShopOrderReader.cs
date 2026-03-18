using CsvHelper;
using CsvHelper.Configuration;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace SOReplaceLabelLib.Data;

public class ShopOrderReader
{
    static string LastErr { get; set; }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static (bool, ShopOrderTexts) Read(string path)
    {
        bool result = false;
        ShopOrderTexts shopOrderTexts = null;
        try
        {
            using var reader = new StreamReader(path, Encoding.GetEncoding("shift_jis"));
            var config = new CsvConfiguration(new CultureInfo("ja-JP", false))
            {
                HasHeaderRecord = false,        // ヘッダ無（デフォルトtrue）
                Delimiter = "\t",               // 区切り文字（デフォルト,）
                TrimOptions = TrimOptions.Trim, // 両端の空白を削除（デフォルトTrimOptions.None）
            };
            using var csv = new CsvReader(reader, config);
            shopOrderTexts = csv.GetRecords<ShopOrderTexts>().FirstOrDefault();
            result = shopOrderTexts != null;
        }
        catch (Exception exp)
        {
            LastErr = exp.Message;
            return (result, shopOrderTexts);
        }

        return (result, shopOrderTexts); 
    }
}
