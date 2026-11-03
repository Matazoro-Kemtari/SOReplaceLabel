using System;
using System.IO;
using System.Reactive.Disposables;
using System.Text.Json;
using Prism.Mvvm;

namespace Wada.SOReplaceLabel.Wpf.ViewModels;

public class WebViewPageViewModel : BindableBase, IDestructible
{
    private IWebView2Service? _webViewService;

    /// <summary>
    /// レガシーな IE 向けシステムにおける ActiveX (Scripting.FileSystemObject) のファイル書き込み処理を模擬（Shim）するスクリプト。
    /// WebView2 (Chromium) では未サポートの FileSystemObject によるファイル出力をインターセプトし、
    /// 内容をキャメルケースのキー名付き JSON オブジェクトに変換して window.chrome.webview.postMessage 経由で C# 側へ送信します。
    /// </summary>
    private const string ActiveXShimScript = @"
(function() {
    if (window.__activeXShimInstalled) {
        return;
    }
    window.__activeXShimInstalled = true;

    const KEYS = [
        'factoryCode', 'sourceAircraftModel', 'partsNumber', 'idNo', 'barCode', 'productName',
        'leftQuantity', 'rightQuantity', 'destination', 'registrantName', 'usingShop', 'responsibleShop',
        'orderNumber', 'item', 'lotNumber', 'startUnitNumber', 'endUnitNumber', 'status',
        'area', 'sequenceNumber', 'shop', 'completionFlag', 'sourceShortageSign', 'mnd', 'aletFlag'
    ];

    window.ActiveXObject = function(progId) {
        if (progId !== 'Scripting.FileSystemObject') {
            throw new Error('ActiveXObject not supported in this environment: ' + progId);
        }
        return {
            DeleteFile: function() {
                // ファイル削除は不要なため何もしない
            },
            OpenTextFile: function() {
                var rawText = '';
                return {
                    Write: function(text) {
                        rawText += text;
                    },
                    WriteLine: function(text) {
                        rawText += text + '\n';
                    },
                    Close: function() {
                        try {
                            var values = rawText.split('\t');
                            var data = {};
                            for (var i = 0; i < KEYS.length; i++) {
                                data[KEYS[i]] = values[i] !== undefined ? values[i].trim() : '';
                            }
                            window.chrome.webview.postMessage(JSON.stringify(data));
                        } catch (e) {
                            console.error('postMessage failed:', e);
                        }
                    }
                };
            }
        };
    };
})();";

    public async Task InitializeAsync(IWebView2Service webView2Service)
    {
        _webViewService = webView2Service;
        _webViewService.WebMessageReceived += OnWebMessageReceived;

        var userDataFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Wada.SOReplaceLabel", "WebView2Data");

        await _webViewService.InitializeAsync(userDataFolder);

        // スクリプト注入
        _webViewService.AddScriptOnDocumentCreated(ActiveXShimScript);

        // ナビゲート
        _webViewService.Navigate("https://ghsam095mgs.mhi.co.jp/KIT/Transit/Transit_PT.asp");
    }

    private void OnWebMessageReceived(object sender, string json)
    {
        if (string.IsNullOrEmpty(json)) return;

        try
        {
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                PropertyNameCaseInsensitive = true
            };
            var payload = JsonSerializer.Deserialize<ShopOrderPayload>(json, options);
            if (payload == null) return;

            // 印刷処理
            //ProcessAndPrint(payload);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error processing web message: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Disposeが必要なReactivePropertyやReactiveCommandを集約させるための仕掛け
    /// </summary>
    private CompositeDisposable Disposables { get; } = [];

    public void Destroy()
    {
        if (_webViewService != null)
        {
            _webViewService.WebMessageReceived -= OnWebMessageReceived;
        }
        Disposables.Dispose();
    }

    public class ShopOrderPayload
    {
        /// <summary>
        /// 工場コード
        /// </summary>
        public string FactoryCode { get; set; } = string.Empty;

        /// <summary>
        /// 機種
        /// </summary>
        public string SourceAircraftModel { get; set; } = string.Empty;

        /// <summary>
        /// 部品番号
        /// </summary>
        public string PartsNumber { get; set; } = string.Empty;

        /// <summary>
        /// IDNo
        /// </summary>
        public string IDNo { get; set; } = string.Empty;

        /// <summary>
        /// バーコード
        /// </summary>
        public string BarCode { get; set; } = string.Empty;

        /// <summary>
        /// 名称
        /// </summary>
        public string ProductName { get; set; } = string.Empty;

        /// <summary>
        /// 左個数
        /// </summary>
        public string LeftQuantity { get; set; } = string.Empty;

        /// <summary>
        /// 右個数
        /// </summary>
        public string RightQuantity { get; set; } = string.Empty;

        /// <summary>
        /// 送り先
        /// </summary>
        public string Destination { get; set; } = string.Empty;

        /// <summary>
        /// 登録者ID
        /// </summary>
        public string RegistrantName { get; set; } = string.Empty;

        /// <summary>
        /// 使用ショップ
        /// </summary>
        public string UsingShop { get; set; } = string.Empty;

        /// <summary>
        /// 責任ショップ
        /// </summary>
        public string ResponsibleShop { get; set; } = string.Empty;

        /// <summary>
        /// オーダー
        /// </summary>
        public string OrderNumber { get; set; } = string.Empty;

        /// <summary>
        /// アイテム
        /// </summary>
        public string Item { get; set; } = string.Empty;

        /// <summary>
        /// ロット
        /// </summary>
        public string LotNumber { get; set; } = string.Empty;

        /// <summary>
        /// 開始号機
        /// </summary>
        public string StartUnitNumber { get; set; } = string.Empty;

        /// <summary>
        /// 終了号機
        /// </summary>
        public string EndUnitNumber { get; set; } = string.Empty;

        /// <summary>
        /// ステータス
        /// </summary>
        public string Status { get; set; } = string.Empty;

        /// <summary>
        /// エリア
        /// </summary>
        public string Area { get; set; } = string.Empty;

        /// <summary>
        /// 連番
        /// </summary>
        public string SequenceNumber { get; set; } = string.Empty;

        /// <summary>
        /// ショップ
        /// </summary>
        public string Shop { get; set; } = string.Empty;

        /// <summary>
        /// 完了
        /// </summary>
        public string CompletionFlag { get; set; } = string.Empty;

        /// <summary>
        /// 欠品サイン
        /// </summary>
        public string SourceShortageSign { get; set; } = string.Empty;

        /// <summary>
        /// MND
        /// </summary>
        public string MND { get; set; } = string.Empty;

        /// <summary>
        /// アラートフラグ
        /// </summary>
        public string AletFlag { get; set; } = string.Empty;
    }
}
