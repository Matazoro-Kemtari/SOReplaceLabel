using Shouldly;
using Wada.SOReplaceLabel.Application.LabelPrinter;

namespace Wada.SOReplaceLabel.ApplicationTests.LabelPrinter;

[TestClass]
public sealed class ShopOrderParamTests
{
    [TestMethod]
    public void 正常系_有効な値でLabelPrintContentに変換できること()
    {
        // given
        var param = new ShopOrderParam(
            SourceAircraftModel: "FP1",
            LeftQuantity: "10",
            RightQuantity: "20",
            Destination: "名古屋",
            SourceShortageSign: "1");

        // when
        var content = param.ConvertLabelPrintContent();

        // then
        content.AircraftModel.PrintValue.ShouldBe("777");
        content.LeftQuantity.ShouldBe(10);
        content.RightQuantity.ShouldBe(20);
        content.Destination.ShouldBe("名古屋");
        content.Shortage.PrintValue.ShouldBe("欠品");
    }

    [TestMethod]
    [DataRow("abc", "xyz", 0, 0)]
    [DataRow("", "", 0, 0)]
    [DataRow(" 5 ", " 15 ", 5, 15)]
    public void 正常系_数量が数値以外または空白の場合は0またはパース値にフォールバックすること(
        string leftQty,
        string rightQty,
        int expectedLeft,
        int expectedRight)
    {
        // given
        var param = new ShopOrderParam(
            SourceAircraftModel: "FP3",
            LeftQuantity: leftQty,
            RightQuantity: rightQty,
            Destination: "東京",
            SourceShortageSign: "0");

        // when
        var content = param.ConvertLabelPrintContent();

        // then
        content.LeftQuantity.ShouldBe(expectedLeft);
        content.RightQuantity.ShouldBe(expectedRight);
    }

    [TestMethod]
    public void 異常系_未定義の欠品サインはエラー文字列として変換されること()
    {
        // given
        var param = new ShopOrderParam(
            SourceAircraftModel: "FP1",
            LeftQuantity: "1",
            RightQuantity: "1",
            Destination: "東京",
            SourceShortageSign: "999");

        // when
        var content = param.ConvertLabelPrintContent();

        // then
        content.Shortage.PrintValue.ShouldBe("エラー(欠品サイン)");
    }
}
