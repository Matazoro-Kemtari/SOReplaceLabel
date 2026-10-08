using Shouldly;
using Wada.SOReplaceLabel.Domain.ValueObjects;

namespace Wada.SOReplaceLabel.DomainTests.ValueObjects;

[TestClass]
public sealed class AircraftModelCodeTests
{
    [TestMethod]
    [DataRow("FP1", "777")]
    [DataRow("FP3", "737")]
    [DataRow("FPB", "747")]
    [DataRow("FPD", "787")]
    [DataRow("T", "767")]
    [DataRow("FPC", "MRJ")]
    public void 正常系_機種コードを印刷用コードに変換すること(string sourceCode, string expectedPrintValue)
    {
        var aircraftModelCode = AircraftModelCode.FromSourceCode(sourceCode);

        aircraftModelCode.PrintValue.ShouldBe(expectedPrintValue);
    }

    [TestMethod]
    [DataRow(" FP1 ", "777")]
    [DataRow("\tFP3\r\n", "737")]
    public void 正常系_既知の機種コード前後の空白を除去して変換すること(
        string sourceCode,
        string expectedPrintValue)
    {
        var aircraftModelCode = AircraftModelCode.FromSourceCode(sourceCode);

        aircraftModelCode.PrintValue.ShouldBe(expectedPrintValue);
    }

    [TestMethod]
    public void 正常系_未知の機種コードは入力値をそのまま返すこと()
    {
        const string sourceCode = " UNKNOWN ";

        var aircraftModelCode = AircraftModelCode.FromSourceCode(sourceCode);

        aircraftModelCode.PrintValue.ShouldBe(sourceCode);
    }

    [TestMethod]
    public void 正常系_nullは空文字に変換すること()
    {
        var aircraftModelCode = AircraftModelCode.FromSourceCode(null);

        aircraftModelCode.PrintValue.ShouldBe(string.Empty);
    }
}
