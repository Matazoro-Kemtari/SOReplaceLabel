using CsvHelper.Configuration.Attributes;
using Wada.SOReplaceLabel.Domain.ShopOrderAggregation;
using Wada.SOReplaceLabel.Domain.ValueObjects;

namespace Wada.SOReplaceLabel.Application.CsvData;

internal class ShopOrderCsvRecord : IEquatable<ShopOrderCsvRecord>
{
    /// <summary>
    /// 工場コード
    /// </summary>
    [Index(0)]
    public string? FactoryCode { get; set; }

    /// <summary>
    /// 機種
    /// </summary>
    [Index(1)]
    public string? SourceAircraftModel { get; set; }

    /// <summary>
    /// 部品番号
    /// </summary>
    [Index(2)]
    public string? PartsNumber { get; set; }

    /// <summary>
    /// IDNo
    /// </summary>
    [Index(3)]
    public string? IDNo { get; set; }

    /// <summary>
    /// バーコード
    /// </summary>
    [Index(4)]
    public string? BarCode { get; set; }

    /// <summary>
    /// 名称
    /// </summary>
    [Index(5)]
    public string? ProductName { get; set; }

    /// <summary>
    /// 左個数
    /// </summary>
    [Index(6)]
    public string? LeftQuantity { get; set; }

    /// <summary>
    /// 右個数
    /// </summary>
    [Index(7)]
    public string? RightQuantity { get; set; }

    /// <summary>
    /// 送り先
    /// </summary>
    [Index(8)]
    public string? Destination { get; set; }

    /// <summary>
    /// 登録者ID
    /// </summary>
    [Index(9)]
    public string? RegistrantName { get; set; }

    /// <summary>
    /// 使用ショップ
    /// </summary>
    [Index(10)]
    public string? UsingShop { get; set; }

    /// <summary>
    /// 責任ショップ
    /// </summary>
    [Index(11)]
    public string? ResponsibleShop { get; set; }

    /// <summary>
    /// オーダー
    /// </summary>
    [Index(12)]
    public string? OrderNumber { get; set; }

    /// <summary>
    /// アイテム
    /// </summary>
    [Index(13)]
    public string? Item { get; set; }

    /// <summary>
    /// ロット
    /// </summary>
    [Index(14)]
    public string? LotNumber { get; set; }

    /// <summary>
    /// 開始号機
    /// </summary>
    [Index(15)]
    public string? StartUnitNumber { get; set; }

    /// <summary>
    /// 終了号機
    /// </summary>
    [Index(16)]
    public string? EndUnitNumber { get; set; }

    /// <summary>
    /// ステータス
    /// </summary>
    [Index(17)]
    public string? Status { get; set; }

    /// <summary>
    /// エリア
    /// </summary>
    [Index(18)]
    public string? Area { get; set; }

    /// <summary>
    /// 連番
    /// </summary>
    [Index(19)]
    public string? SequenceNumber { get; set; }

    /// <summary>
    /// ショップ
    /// </summary>
    [Index(20)]
    public string? Shop { get; set; }

    /// <summary>
    /// 完了
    /// </summary>
    [Index(21)]
    public string? CompletionFlag { get; set; }

    /// <summary>
    /// 欠品サイン
    /// </summary>
    [Index(22)]
    public string? SourceShortageSign { get; set; }

    /// <summary>
    /// MND
    /// </summary>
    [Index(23)]
    public string? MND { get; set; }

    /// <summary>
    /// アラートフラグ
    /// </summary>
    [Index(24)]
    public string? AletFlag { get; set; }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public bool Equals(ShopOrderCsvRecord? other)
    {
        //objがnullのときは等価でない
        if (other == null)
        {
            return false;
        }

        // Optimization for a common success case.
        if (Object.ReferenceEquals(this, other))
        {
            return true;
        }

        // If run-time types are not exactly the same, return false.
        if (this.GetType() != other.GetType())
        {
            return false;
        }

        //Linqの遅延評価でマッチしないプロパティが見つかったら不一致と判定
        return PropertyEquals(other).All(isEqual => isEqual);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public override bool Equals(object obj)
    {
        return this.Equals(obj as ShopOrderCsvRecord);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public override int GetHashCode()
    {
        int hash = 17;
        hash = hash * 23 + (SourceAircraftModel?.GetHashCode() ?? 0);
        hash = hash * 23 + (PartsNumber?.GetHashCode() ?? 0);
        hash = hash * 23 + (BarCode?.GetHashCode() ?? 0);
        hash = hash * 23 + (ProductName?.GetHashCode() ?? 0);
        hash = hash * 23 + (Destination?.GetHashCode() ?? 0);
        hash = hash * 23 + (SourceShortageSign?.GetHashCode() ?? 0);
        return hash;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    private IEnumerable<bool> PropertyEquals(ShopOrderCsvRecord other)
    {
        yield return other != null;
        yield return FactoryCode == other?.FactoryCode;
        yield return SourceAircraftModel == other?.SourceAircraftModel;
        yield return PartsNumber == other?.PartsNumber;
        yield return IDNo == other?.IDNo;
        yield return BarCode == other?.BarCode;
        yield return ProductName == other?.ProductName;
        yield return LeftQuantity == other?.LeftQuantity;
        yield return RightQuantity == other?.RightQuantity;
        yield return Destination == other?.Destination;
        yield return RegistrantName == other?.RegistrantName;
        yield return UsingShop == other?.UsingShop;
        yield return ResponsibleShop == other?.ResponsibleShop;
        yield return OrderNumber == other?.OrderNumber;
        yield return Item == other?.Item;
        yield return LotNumber == other?.LotNumber;
        yield return StartUnitNumber == other?.StartUnitNumber;
        yield return EndUnitNumber == other?.EndUnitNumber;
        yield return Status == other?.Status;
        yield return Area == other?.Area;
        yield return SequenceNumber == other?.SequenceNumber;
        yield return Shop == other?.Shop;
        yield return CompletionFlag == other?.CompletionFlag;
        yield return SourceShortageSign == other?.SourceShortageSign;
        yield return MND == other?.MND;
        yield return AletFlag == other?.AletFlag;
    }

    // CSVレコードからドメインエンティティへ変換
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
