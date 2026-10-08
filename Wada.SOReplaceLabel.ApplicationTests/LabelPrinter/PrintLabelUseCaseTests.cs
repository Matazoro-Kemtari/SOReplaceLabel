using Moq;
using Shouldly;
using Wada.SOReplaceLabel.Application.LabelPrinter;
using Wada.SOReplaceLabel.Domain;
using Wada.SOReplaceLabel.Domain.ShopOrderAggregation;

namespace Wada.SOReplaceLabel.ApplicationTests.LabelPrinter;

[TestClass]
public sealed class PrintLabelUseCaseTests
{
    [TestMethod]
    public async Task 正常系_変換したラベル内容をプリンターへ渡すこと()
    {
        var labelPrinter = new Mock<ILabelPrinter>();
        var useCase = new PrintLabelUseCase(labelPrinter.Object);
        var content = string.Join(
            "\t",
            [
                "factory",
                "FP1",
                "parts",
                "id",
                "barcode",
                "product",
                "12",
                "34",
                "Tokyo",
                "registrant",
                "using-shop",
                "responsible-shop",
                "order",
                "item",
                "lot",
                "start",
                "end",
                "status",
                "area",
                "sequence",
                "shop",
                "completion",
                "1",
                "mnd",
                "alert",
            ]);

        await useCase.ExecuteAsync(content);

        labelPrinter.Verify(
            printer => printer.PrintAsync(It.Is<LabelPrintContent>(printContent =>
                printContent.AircraftModel.PrintValue == "777" &&
                printContent.LeftQuantity == "12" &&
                printContent.RightQuantity == "34" &&
                printContent.Destination == "Tokyo" &&
                printContent.Shortage.PrintValue == "欠品")),
            Times.Once);
    }

    [TestMethod]
    public async Task 異常系_contentがnullの場合はArgumentNullExceptionを送出し印刷しないこと()
    {
        // given
        var labelPrinter = new Mock<ILabelPrinter>();
        var useCase = new PrintLabelUseCase(labelPrinter.Object);

        // when
        var action = () => useCase.ExecuteAsync(null!);

        // then
        var ex = await action.ShouldThrowAsync<ArgumentNullException>();
        ex.Message.ShouldContain("値を Null にすることはできません。");

        labelPrinter.Verify(printer => printer.PrintAsync(It.IsAny<LabelPrintContent>()), Times.Never);
    }
}
