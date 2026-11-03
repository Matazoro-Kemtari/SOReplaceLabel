using Moq;
using Shouldly;
using Wada.SOReplaceLabel.Application.LabelPrinter;
using Wada.SOReplaceLabel.Wpf.ViewModels;

namespace Wada.SOReplaceLabel.WpfTests.ViewModels;

[TestClass]
public sealed class WebViewPageViewModelTests
{
    [TestMethod]
    public void 正常系_ShopOrderPayloadからShopOrderParamに正しく変換されること()
    {
        // given
        var payload = new WebViewPageViewModel.ShopOrderPayload
        {
            FactoryCode = "FC01",
            SourceAircraftModel = "FP1",
            PartsNumber = "PN-12345",
            IDNo = "ID001",
            BarCode = "BAR987",
            ProductName = "Wing Bracket",
            LeftQuantity = "10",
            RightQuantity = "20",
            Destination = "名古屋",
            RegistrantName = "User1",
            UsingShop = "ShopA",
            ResponsibleShop = "ShopB",
            OrderNumber = "ORD-001",
            Item = "ITEM-01",
            LotNumber = "LOT-99",
            StartUnitNumber = "1",
            EndUnitNumber = "10",
            Status = "Active",
            Area = "AreaX",
            SequenceNumber = "001",
            Shop = "MainShop",
            CompletionFlag = "0",
            SourceShortageSign = "1",
            MND = "2026-11-03",
            AletFlag = "0"
        };

        // when
        var param = payload.ToParam();

        // then
        param.SourceAircraftModel.ShouldBe("FP1");
        param.LeftQuantity.ShouldBe("10");
        param.RightQuantity.ShouldBe("20");
        param.Destination.ShouldBe("名古屋");
        param.SourceShortageSign.ShouldBe("1");
    }

    [TestMethod]
    public async Task 正常系_WebMessageReceivedイベントを受信したとき印刷ユースケースが実行されること()
    {
        // given
        var useCaseMock = new Mock<IPrintLabelUseCase>();
        var serviceMock = new Mock<IWebView2Service>();
        var viewModel = new WebViewPageViewModel(useCaseMock.Object);

        await viewModel.InitializeAsync(serviceMock.Object);

        var json = """
        {
            "factoryCode": "FC01",
            "sourceAircraftModel": "FP1",
            "partsNumber": "P123",
            "idNo": "ID1",
            "barCode": "B1",
            "productName": "ItemName",
            "leftQuantity": "5",
            "rightQuantity": "15",
            "destination": "羽田",
            "registrantName": "山田",
            "usingShop": "S1",
            "responsibleShop": "S2",
            "orderNumber": "O1",
            "item": "I1",
            "lotNumber": "L1",
            "startUnitNumber": "1",
            "endUnitNumber": "2",
            "status": "OK",
            "area": "A1",
            "sequenceNumber": "1",
            "shop": "SH1",
            "completionFlag": "1",
            "sourceShortageSign": "1",
            "mnd": "20261103",
            "aletFlag": "0"
        }
        """;

        // when
        serviceMock.Raise(s => s.WebMessageReceived += null, serviceMock.Object, json);

        // then
        useCaseMock.Verify(
            u => u.ExecuteAsync(It.Is<ShopOrderParam>(p =>
                p.SourceAircraftModel == "FP1" &&
                p.LeftQuantity == "5" &&
                p.RightQuantity == "15" &&
                p.Destination == "羽田" &&
                p.SourceShortageSign == "1")),
            Times.Once);
    }

    [TestMethod]
    [DataRow("")]
    [DataRow(null)]
    public async Task 正常系_空メッセージを受信したときは印刷処理を実行しないこと(string? emptyJson)
    {
        // given
        var useCaseMock = new Mock<IPrintLabelUseCase>();
        var serviceMock = new Mock<IWebView2Service>();
        var viewModel = new WebViewPageViewModel(useCaseMock.Object);

        await viewModel.InitializeAsync(serviceMock.Object);

        // when
        serviceMock.Raise(s => s.WebMessageReceived += null, serviceMock.Object, emptyJson!);

        // then
        useCaseMock.Verify(u => u.ExecuteAsync(It.IsAny<ShopOrderParam>()), Times.Never);
    }

    [TestMethod]
    public async Task 正常系_Destroyが呼ばれたらイベント購読が解除されること()
    {
        // given
        var useCaseMock = new Mock<IPrintLabelUseCase>();
        var serviceMock = new Mock<IWebView2Service>();
        var viewModel = new WebViewPageViewModel(useCaseMock.Object);

        await viewModel.InitializeAsync(serviceMock.Object);

        // when
        viewModel.Destroy();
        serviceMock.Raise(s => s.WebMessageReceived += null, serviceMock.Object, "{}");

        // then
        useCaseMock.Verify(u => u.ExecuteAsync(It.IsAny<ShopOrderParam>()), Times.Never);
    }

    [TestMethod]
    public async Task 正常系_印刷処理で例外が発生してもハンドラ内で捕捉されアプリを落とさないこと()
    {
        // given
        var useCaseMock = new Mock<IPrintLabelUseCase>();
        useCaseMock
            .Setup(u => u.ExecuteAsync(It.IsAny<ShopOrderParam>()))
            .ThrowsAsync(new InvalidOperationException("プリンター接続エラー"));

        var serviceMock = new Mock<IWebView2Service>();
        var viewModel = new WebViewPageViewModel(useCaseMock.Object);

        await viewModel.InitializeAsync(serviceMock.Object);

        // when & then: 例外が外に漏れず例外なく完了すること
        var act = () => serviceMock.Raise(s => s.WebMessageReceived += null, serviceMock.Object, "{\"sourceAircraftModel\":\"FP1\"}");
        Should.NotThrow(act);
    }
}
