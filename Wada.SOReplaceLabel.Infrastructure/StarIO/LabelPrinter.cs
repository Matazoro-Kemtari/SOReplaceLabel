using System.Text;
using StarMicronics.StarIO;
using StarMicronics.StarIOExtension;
using Wada.SOReplaceLabel.Application.LabelPrinter;
using Wada.SOReplaceLabel.Domain;
using Wada.SOReplaceLabel.Domain.ShopOrderAggregation;

namespace Wada.SOReplaceLabel.Infrastructure.StarIO;

public class LabelPrinter : ILabelPrinter
{
    /// <summary>
    /// プリンターの接続ポート名
    /// Bluetooth接続の場合、"BT:<デバイスアドレス>" を指定します。
    /// </summary>
    private readonly string _portName = "USBPRN:Star MCP31";

    /// <summary>
    /// プリンターの接続ポート設定 (通常は空文字で問題ありません)
    /// </summary>
    private readonly string _portSettings = "";

    /// <summary>
    /// プリンターの接続タイムアウト時間（ミリ秒）
    /// </summary>
    private readonly int _timeoutMilliseconds = 10000;

    public async Task PrintAsync(LabelPrintContent content)
    {
        if (content is null) throw new ArgumentNullException(nameof(content));

        await Task.Run(() =>
        {
            IPort? port = null;
            try
            {
                // mC-Print3用のBuilderを作成 (Emulation.StarPRNT)
                var builder = StarIoExt.CreateCommandBuilder(Emulation.StarPRNT);

                builder.AppendInitialization(InitializationType.Command);

                builder.BeginDocument();

                // 行揃え指定: 中央揃え
                builder.AppendAlignment(AlignmentPosition.Center);

                // 漢字コード設定
                builder.AppendCodePage(CodePageType.UTF8);

                builder.AppendFontStyle(FontStyleType.A);

                /*** 印字内容設定 ***/
                // 機種
                var line = content.AircraftModel.PrintValue;
                var width = GetOptimalWidthMultiple(line, 6);
                var height = 5;
                builder.AppendMultiple(Encoding.UTF8.GetBytes(content.AircraftModel.PrintValue + "\n"), width, height);

                // 送り先 欠品サイン
                line = content.Destination + content.Shortage.PrintValue;
                width = GetOptimalWidthMultiple(line, 4);
                height = 3;
                builder.AppendMultiple(Encoding.UTF8.GetBytes(line + "\n"), width, height);

                // 左右数量
                line = $"{content.LeftQuantity}:{content.RightQuantity}({content.LeftQuantity + content.RightQuantity})";
                width = GetOptimalWidthMultiple(line, 5);
                height = 5;
                builder.AppendMultiple(Encoding.UTF8.GetBytes(line + "\n"), width, height);

                // 文字サイズと位置を元に戻す
                builder.AppendMultiple(1, 1);
                builder.AppendAlignment(AlignmentPosition.Left);

                // カット
                builder.AppendCutPaper(CutPaperAction.PartialCutWithFeed);

                builder.EndDocument();

                // StarIOを使用してポートをオープン
                // Bluetoothの場合、デバイスのペアリングが完了している必要があります
                port = Factory.I.GetPort(_portName, _portSettings, _timeoutMilliseconds);

                // データの送信
                byte[] commands2 = builder.Commands;
                port.WritePort(commands2, 0, (uint)commands2.Length);
            }
            catch (PortException ex)
            {
                if (ex.ErrorCode == StarResultCode.ErrorInUse)
                {
                    throw new PrintException("プリンターから接続拒否されました。", ex);
                }
                else
                {
                    throw new PrintException("ラベルプリンターへの印字中にエラーが発生しました。", ex);
                }
            }
            finally
            {
                if (port != null)
                {
                    Factory.I.ReleasePort(port);
                }
            }
        });
    }

    /// <summary>
    /// 指定した文字列が印字領域からはみ出さない最大の横倍率を取得します。
    /// 半角文字を1桁、全角文字を2桁として計算します。
    /// </summary>
    /// <param name="text">印刷する文字列</param>
    /// <param name="maxWidth">最大横倍率</param>
    /// <param name="printableColumns">倍率1倍時の半角文字の最大桁数</param>
    /// <returns>横倍率（1～maxWidth）</returns>
    internal static int GetOptimalWidthMultiple(
        string text,
        int maxWidth = 6,
        int printableColumns = 48)
    {
        if (string.IsNullOrEmpty(text))
        {
            return maxWidth;
        }

        // 半角=1、全角=2として文字幅を計算
        int columns = text.Sum(c => IsHalfWidth(c) ? 1 : 2);

        // 1倍で使用する幅を基準に、入る最大倍率を探す
        for (int multiple = maxWidth; multiple >= 1; multiple--)
        {
            if (columns * multiple <= printableColumns - multiple)
            {
                return multiple;
            }
        }

        return 1;
    }

    /// <summary>
    /// 半角文字かどうかを判定します。
    /// </summary>
    internal static bool IsHalfWidth(char c)
    {
        return c <= '\u007F'
            || (c >= '\uFF61' && c <= '\uFF9F');
    }
}
