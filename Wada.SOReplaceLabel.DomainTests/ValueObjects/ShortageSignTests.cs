using Shouldly;
using Wada.SOReplaceLabel.Domain.ValueObjects;

namespace Wada.SOReplaceLabel.DomainTests.ValueObjects;

[TestClass]
public sealed class ShortageSignTests
{
    [TestMethod]
    [DataRow("1", "欠品")]
    [DataRow("0", "")]
    [DataRow("2", "")]
    [DataRow("3", "")]
    public void 正常系_欠品サインを印刷用文字列に変換すること(
        string sourceSign,
        string expectedPrintValue)
    {
        var shortageSign = ShortageSign.FromSourceSign(sourceSign);

        shortageSign.PrintValue.ShouldBe(expectedPrintValue);
    }

    [TestMethod]
    [DataRow(" 1 ", "欠品")]
    [DataRow("\t2\r\n", "")]
    public void 正常系_欠品サイン前後の空白を除去して変換すること(
        string sourceSign,
        string expectedPrintValue)
    {
        var shortageSign = ShortageSign.FromSourceSign(sourceSign);

        shortageSign.PrintValue.ShouldBe(expectedPrintValue);
    }

    [TestMethod]
    [DataRow("4")]
    [DataRow("invalid")]
    [DataRow("")]
    [DataRow("   ")]
    [DataRow(null)]
    public void 異常系_未定義の欠品サインはエラー文字列に変換すること(string? sourceSign)
    {
        var shortageSign = ShortageSign.FromSourceSign(sourceSign);

        shortageSign.PrintValue.ShouldBe("エラー(欠品サイン)");
    }
}
