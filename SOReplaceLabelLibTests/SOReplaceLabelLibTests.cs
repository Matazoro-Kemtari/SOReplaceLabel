using SOReplaceLabelLib;

namespace SOReplaceLabelLibTests;

[TestClass]
public sealed class SOReplaceLabelLibTests
{
    [TestMethod]
    public void TestMethod1()
    {
        var watchFilePath = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            "__dummy__.txt");
        var soLabelPrinter = new SOLabelPrinter(watchFilePath);

        soLabelPrinter.PrintLabel("987\n東京\n超特急");        
    }
}
