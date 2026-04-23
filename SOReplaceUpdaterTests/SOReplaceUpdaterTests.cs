using System.IO.Compression;
using System.Net;
using System.Net.Http;
using Moq;
using Moq.Protected;
using Shouldly;
using SOReplaceUpdater;

namespace SOReplaceUpdaterTests;

[TestClass]
public class SOReplaceUpdaterTests
{
    private string? _testRoot;
    private string? _installDir;
    private string? _tempDir;

    [TestInitialize]
    public void Setup()
    {
        _testRoot = Path.Combine(Path.GetTempPath(), $"SOReplaceUpdaterTests_{Guid.NewGuid()}");
        _installDir = Path.Combine(_testRoot, "Install");
        _tempDir = Path.Combine(_testRoot, "TempUpdate");

        if (Directory.Exists(_testRoot))
        {
            Directory.Delete(_testRoot, true);
        }

        Directory.CreateDirectory(_installDir);
        Directory.CreateDirectory(_tempDir);
    }

    [TestCleanup]
    public void Cleanup()
    {
        if (_testRoot != null && Directory.Exists(_testRoot))
        {
            try
            {
                Directory.Delete(_testRoot, true);
            }
            catch
            {
                // テスト後のクリーンアップ失敗は無視
            }
        }
    }

    [TestMethod]
    public async Task 正常系_アップデートの一連の処理が成功すること()
    {
        // Arrange
        if (_installDir == null) throw new ArgumentNullException(nameof(_installDir));
        if (_testRoot == null) throw new ArgumentNullException(nameof(_testRoot));
        if (_tempDir == null) throw new ArgumentNullException(nameof(_tempDir));

        File.WriteAllText(Path.Combine(_installDir, "OldFile.txt"), "Old Content");
        File.WriteAllText(Path.Combine(_installDir, "SOReplaceLabel.exe"), "Dummy Exe");

        string sourceDir = Path.Combine(_testRoot, "Source");
        Directory.CreateDirectory(sourceDir);
        File.WriteAllText(Path.Combine(sourceDir, "NewFile.txt"), "New Content");
        File.WriteAllText(Path.Combine(sourceDir, "SOReplaceLabel.exe"), "New Exe");

        string zipPath = Path.Combine(_testRoot, "update.zip");
        ZipFile.CreateFromDirectory(sourceDir, zipPath);
        byte[] zipBytes = File.ReadAllBytes(zipPath);

        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock
           .Protected()
           .Setup<Task<HttpResponseMessage>>(
              "SendAsync",
              ItExpr.IsAny<HttpRequestMessage>(),
              ItExpr.IsAny<CancellationToken>()
           )
           .ReturnsAsync(new HttpResponseMessage()
           {
               StatusCode = HttpStatusCode.OK,
               Content = new ByteArrayContent(zipBytes),
           })
           .Verifiable();

        using var httpClient = new HttpClient(handlerMock.Object);
        using var manager = new UpdateManager(httpClient, _tempDir);

        // Act
        await manager.RunUpdateAsync("http://example.com/update.zip", _installDir, -1, restart: false);

        // Assert
        File.Exists(Path.Combine(_installDir, "NewFile.txt")).ShouldBeTrue();
        File.ReadAllText(Path.Combine(_installDir, "NewFile.txt")).ShouldBe("New Content");

        string backupDir = Path.Combine(_installDir, "backup");
        Directory.Exists(backupDir).ShouldBeTrue();
        File.Exists(Path.Combine(backupDir, "OldFile.txt")).ShouldBeTrue();
        File.ReadAllText(Path.Combine(backupDir, "OldFile.txt")).ShouldBe("Old Content");

        Directory.Exists(Path.Combine(backupDir, "backup")).ShouldBeFalse();
    }

    [TestMethod]
    public void 正常系_除外設定に従って正しくコピーされること()
    {
        // Arrange
        if (_testRoot == null) throw new ArgumentNullException(nameof(_testRoot));

        string source = Path.Combine(_testRoot, "Source_CopyAll");
        string target = Path.Combine(_testRoot, "Target_CopyAll");
        Directory.CreateDirectory(source);
        Directory.CreateDirectory(Path.Combine(source, "SubDir"));
        Directory.CreateDirectory(Path.Combine(source, "ExcludeDir"));

        File.WriteAllText(Path.Combine(source, "Keep.txt"), "Keep");
        File.WriteAllText(Path.Combine(source, "Exclude.exe"), "Exclude");
        File.WriteAllText(Path.Combine(source, "SubDir", "KeepInSub.txt"), "KeepInSub");

        // サブディレクトリ内の同名アイテム（これらは除外されないはず）
        Directory.CreateDirectory(Path.Combine(source, "SubDir", "ExcludeDir"));
        File.WriteAllText(Path.Combine(source, "SubDir", "Exclude.exe"), "Sub Exclude");

        var manager = new UpdateManager();

        // Act
        manager.CopyAll(source, target, excludePatterns: ["ExcludeDir", "Exclude.exe"]);

        // Assert
        File.Exists(Path.Combine(target, "Keep.txt")).ShouldBeTrue();
        File.Exists(Path.Combine(target, "SubDir", "KeepInSub.txt")).ShouldBeTrue();

        // ルートのアイテムは除外されていること
        File.Exists(Path.Combine(target, "Exclude.exe")).ShouldBeFalse();
        Directory.Exists(Path.Combine(target, "ExcludeDir")).ShouldBeFalse();

        // サブディレクトリ内の同名アイテムは除外されずにコピーされていること
        File.Exists(Path.Combine(target, "SubDir", "Exclude.exe")).ShouldBeTrue();
        Directory.Exists(Path.Combine(target, "SubDir", "ExcludeDir")).ShouldBeTrue();
    }
}
