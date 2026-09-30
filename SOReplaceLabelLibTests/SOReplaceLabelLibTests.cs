using Shouldly;
using SOReplaceLabelLib;

namespace SOReplaceLabelLibTests;

[TestClass]
public sealed class SOReplaceLabelLibTests
{
    [TestMethod]
    public void 正常系_ラベルが印字されること()
    {
        var watchFilePath = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            "__dummy__.txt");
        var soLabelPrinter = new SOLabelPrinter(watchFilePath);

        soLabelPrinter.PrintLabel("987\n東京\n超特急");
    }

    #region 基本ケース（null / empty）

    [TestMethod]
    public void 正常系_nullのときは最大値が返ること()
    {
        int actual = SOLabelPrinter.GetOptimalWidthMultiple(null);
        actual.ShouldBe(6);
    }

    [TestMethod]
    public void 正常系_空のテキストは最大幅を返すこと()
    {
        int actual = SOLabelPrinter.GetOptimalWidthMultiple("");
        actual.ShouldBe(6);
    }

    #endregion

    #region 半角文字のみ

    [TestMethod]
    [DataRow("A", 6)]        // 1*6=6 <= 42
    [DataRow("ABCDEFGH", 5)] // 8*6=48 > 42, 8*5=40 <= 43
    [DataRow("ABCDEFGHI", 4)]// 9*5=45 > 43, 9*4=36 <= 44
    [DataRow("123456789012345678901234567890123456789012345678", 1)] // 48*1=48 > 47, 最低1を返す
    [DataRow("1234567890123456789012345678901234567890123456789", 1)] // 49*1=49 > 47, 最低1を返す
    public void 正常系_半角文字の時期待値が返ること(string text, int expected)
    {
        int actual = SOLabelPrinter.GetOptimalWidthMultiple(text);
        actual.ShouldBe(expected);

    }

    #endregion

    #region 全角文字のみ

    [TestMethod]
    [DataRow("あ", 6)]           // 2*6=12 <= 42
    [DataRow("あいうえおか", 3)] // 12*4=48 > 44, 12*3=36 <= 45
    [DataRow("あいうえおき", 3)] // 14*3=42 <= 45
    [DataRow("あいうえおかきくけこ", 2)] // 20*2=40 <= 46
    public void 正常系_全角文字のみのとき期待値が返ること(string text, int expected)
    {
        int actual = SOLabelPrinter.GetOptimalWidthMultiple(text);
        actual.ShouldBe(expected);
    }

    #endregion

    #region 半角/全角混合

    [TestMethod]
    [DataRow("No.8", 6)]       // 4*6=24 <= 42
    [DataRow("No.88", 6)]      // 5*6=30 <= 42
    [DataRow("品番:AB", 6)]    // 7*6=42 <= 42 → 元ロジックでは6、修正後も42<=42? 
                               // 修正後: 48-6=42なので 42<=42 true → 6
    [DataRow("品番:ABC", 5)]   // 8*6=48 > 42, 8*5=40 <= 43
    [DataRow("品番:ABCD", 4)]  // 9*5=45 > 43, 9*4=36 <= 44
    public void 正常系_半角全角混合のとき期待値が返ること(string text, int expected)
    {
        int actual = SOLabelPrinter.GetOptimalWidthMultiple(text);
        actual.ShouldBe(expected);
    }

    #endregion

    #region カスタムパラメータ（maxWidth / printableColumns 変更）

    [TestMethod]
    [DataRow("ABC", 10, 100, 10)]                       // 3*10=30 <= 90
    [DataRow("ABCDEFGHIJKLMNOPQRSTUVWXYZ", 3, 30, 1)]  // 26*1=26 <= 29
    [DataRow("あいう", 4, 10, 1)]                       // 6*1=6 <= 9, 6*2=12 > 8
    public void 正常系_カスタムパラメーターで期待値が返ること(
        string text, int maxWidth, int printableColumns, int expected)
    {
        int actual = SOLabelPrinter.GetOptimalWidthMultiple(text, maxWidth, printableColumns);
        actual.ShouldBe(expected);
    }

    #endregion

    #region リグレッション: 中央揃えによる折り返し問題

    // ============================================
    // 重要: 以下の期待値は、前回提案した
    //       「columns * multiple <= printableColumns - multiple」
    //       （中央揃え対策済み）のロジックを前提としています。
    //
    //       もしまだ元の「<= printableColumns」のままなら、
    //       これらのテストは失敗し、折り返しバグが再現できます。
    // ============================================

    [TestMethod]
    [DataRow("松阪（真和）", 3)] // 12*4=48 > 44, 12*3=36 <= 45
    [DataRow("30:03:10", 5)]     // 8*5=40 <= 43
    [DataRow("777", 6)]          // 3*6=18 <= 42
    [DataRow("2:0(2)", 6)]       // 6*6=36 <= 42
    public void 正常系_中央揃えのとき折り返さないこと(string text, int expected)
    {
        int actual = SOLabelPrinter.GetOptimalWidthMultiple(text, maxWidth: 6, printableColumns: 48);
        actual.ShouldBe(expected);
    }

    #endregion

    [TestMethod]
    [DataRow('A', true)]
    [DataRow('1', true)]
    [DataRow('あ', false)]
    [DataRow('ー', false)] // 全角長音記号
    [DataRow('(', true)]   // 半角括弧
    [DataRow('（', false)] // 全角括弧
    public void 正常系_半角全角が判定できること(char c, bool expected)
    {
        SOLabelPrinter.IsHalfWidth(c).ShouldBe(expected);
    }
}
