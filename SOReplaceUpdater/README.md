# SOReplaceUpdater — アプリのアップデート準備手順

`SOReplaceUpdater` は、本体アプリ（`SOReplaceLabel`）のファイルを ZIP で差し替えるための外部アップデーターです。
本体が起動中に自身を上書きできないため、本体が終了したあとに本 exe が更新を実行します。

このドキュメントでは、**新しいバージョンを配布するための準備手順**と、アップデーターの動作概要をまとめます。

---

## 1. 全体の流れ（概要）

```text
[準備（開発者）]
  1. バージョン番号を上げる
  2. Release ビルドする
  3. 配布用 ZIP を作成する
  4. ZIP を公開 URL に置く
  5. version.json を更新する

[実行時（クライアント）]
  1. 本体起動時に version.json を取得
  2. 新しいバージョンがあればユーザーに確認
  3. SOReplaceUpdater.exe を起動し、本体を終了
  4. アップデーターが ZIP を取得・展開・置換
  5. 本体を再起動
```

---

## 2. アップデート準備の手順

### 手順 1. バージョン番号を上げる

本体のアセンブリバージョンを、配布する新しいバージョンに合わせて更新します。

- 変更ファイル: `SOReplaceLabel/SOReplaceLabel.csproj`
- プロパティ: `<Version>`

例:

```xml
<Version>1.3.2.0</Version>
```

起動時の更新チェックは、実行中アセンブリの `Version` と `version.json` の `latestVersion` を比較します。
`latestVersion` は先頭の `v` を除いて比較されるため、`1.3.2.0` と `v1.3.2.0` のどちらでも問題ありません。

### 手順 2. Release ビルドする

ソリューションを **Release** でビルドします。

```powershell
dotnet build SOReplaceLabel.sln -c Release
```

ビルド後、本体の出力先にアップデーターもコピーされます（`SOReplaceLabel.csproj` の `CopyUpdater` ターゲット）。

- 本体出力例: `SOReplaceLabel/bin/Release/net48/`
- 含まれる主なファイル:
  - `SOReplaceLabel.exe`
  - 依存 DLL（`SOReplaceLabelLib.dll`, `WpfMvvm.dll` など）
  - `SOReplaceUpdater.exe`（ビルド後にコピー）
  - 設定ファイルなど

### 手順 3. 配布用 ZIP を作成する

`SOReplaceLabel/bin/Release/net48/` 配下の**配布に必要なファイル**を ZIP にまとめます。

#### ZIP の構造（重要）

ZIP の**直下**に、インストール先へ展開したいファイルが来るようにします。

```text
update.zip
├── SOReplaceLabel.exe
├── SOReplaceLabel.exe.config
├── SOReplaceLabelLib.dll
├── WpfMvvm.dll
├── （その他の依存 DLL / リソース）
└── …
```

アップデーターは次の順で処理します。

1. ZIP を一時フォルダに展開
2. 展開先の内容をインストールディレクトリへ上書きコピー

そのため、次のような**余分な親フォルダ**を ZIP 内に含めないでください。

```text
# NG 例（展開後に net48 フォルダができてしまう）
update.zip
└── net48/
    ├── SOReplaceLabel.exe
    └── …
```

#### ZIP に含めない／注意する項目

| 対象                       | 扱い                                                                                       |
| -------------------------- | ------------------------------------------------------------------------------------------ |
| `SOReplaceUpdater.exe`     | 更新時に**上書き対象外**（インストール済みのものを使い続ける）。ZIP に入れても置換されない |
| `backup` フォルダ          | 更新時にバックアップ対象外。ZIP にも含めない                                               |
| `*.pdb` などデバッグ用     | 本番配布では通常不要                                                                       |
| ユーザー固有の設定・データ | 上書きされる可能性があるため、原則含めない                                                 |

> **補足:** アップデーター自身（`SOReplaceUpdater.exe`）を更新したい場合は、本仕組みでは置換されないため、別途手動配置や初回インストールパッケージでの差し替えが必要です。

### 手順 4. ZIP を公開する

作成した ZIP を、クライアントから HTTP(S) で取得できる場所に配置します。

例:

```text
http://www.wadass.com/release-publisher/app-release-data/soreplacelabel-update/SOReplaceLabel-1.3.2.0.zip
```

この URL が、次の `version.json` の `downloadUrl` になります。

### 手順 5. `version.json` を更新する

本体は起動時に次の URL からバージョン情報を取得します（`Settings.UpdateVersionUrl`）。

```text
http://www.wadass.com/release-publisher/app-release-data/soreplacelabel-update/version.json
```

JSON 形式:

```json
{
  "latestVersion": "1.3.2.0",
  "downloadUrl": "http://www.wadass.com/release-publisher/app-release-data/soreplacelabel-update/SOReplaceLabel-1.3.2.0.zip"
}
```

| フィールド      | 説明                                                                 |
| --------------- | -------------------------------------------------------------------- |
| `latestVersion` | 最新バージョン。本体のアセンブリバージョンより大きい場合に更新を促す |
| `downloadUrl`   | 手順 4 で公開した ZIP の URL                                         |

#### 公開時の注意

1. **先に ZIP を公開**してから `version.json` を更新する（逆だと、ダウンロード失敗で更新が失敗する）
2. `latestVersion` は、手順 1 で設定した `<Version>` と一致させる
3. `downloadUrl` はクライアントから到達可能であること（ファイアウォール・認証なしで GET できること）

---

## 3. クライアント側の更新動作（参考）

### 3.1 本体アプリの起動時チェック

`SOReplaceLabel` 起動時（`App.xaml.cs`）:

1. 現在のアセンブリバージョンを取得
2. `UpdateVersionUrl` の `version.json` を取得
3. `latestVersion` が新しければ確認ダイアログを表示
4. ユーザーが「はい」を選ぶと `SOReplaceUpdater.exe` を起動し、本体を終了

起動コマンド例:

```text
SOReplaceUpdater.exe "<downloadUrl>" "<installDir>" "<parentPid>"
```

| 引数          | 内容                                                        |
| ------------- | ----------------------------------------------------------- |
| `downloadUrl` | `version.json` の `downloadUrl`                             |
| `installDir`  | 本体のインストールディレクトリ（通常は exe と同じフォルダ） |
| `parentPid`   | 本体プロセスの PID（終了待ちに使用）                        |

### 3.2 アップデーターの処理（`Program.cs`）

`SOReplaceUpdater` は次の順で更新します。

1. **親プロセス終了待ち**
   本体（`parentPid`）が終了するまで待機する（すでに終了済みならスキップ）

2. **ダウンロード**
   `downloadUrl` から ZIP を取得し、一時フォルダ
   `%TEMP%\SOReplaceLabel_Update\update.zip` に保存

3. **展開**
   同一時フォルダの `extract` に展開

4. **バックアップ作成**
   現在のインストール内容を一時 `backup` にコピー
   （ルートの `backup` フォルダと `SOReplaceUpdater.exe` は除外）

5. **ファイル置換**
   展開内容をインストールディレクトリへ上書き
   （`SOReplaceUpdater.exe` は除外）

6. **永続バックアップ**
   1 世代前をインストール先の `backup` フォルダに残す

7. **失敗時のロールバック**
   置換中に失敗した場合、一時バックアップから復元して例外を再送出

8. **再起動**
   インストール先の `SOReplaceLabel.exe` を起動

---

## 4. チェックリスト

公開前に次を確認してください。

- [ ] `SOReplaceLabel.csproj` の `<Version>` を上げた
- [ ] Release ビルドが成功した
- [ ] ZIP 直下に `SOReplaceLabel.exe` などがある（余計な親フォルダなし）
- [ ] ZIP を公開 URL に配置し、ブラウザ等でダウンロードできる
- [ ] `version.json` の `latestVersion` と `downloadUrl` が正しい
- [ ] `version.json` を公開した（ZIP 公開の**後**）
- [ ] 旧バージョンのクライアントで起動し、更新ダイアログが出ることを確認した（可能なら）

---

## 5. トラブルシュート

| 症状                           | 確認ポイント                                                                 |
| ------------------------------ | ---------------------------------------------------------------------------- |
| 更新ダイアログが出ない         | `version.json` の URL・JSON 形式、`latestVersion` が現在より新しいか         |
| ダウンロード失敗               | `downloadUrl` の到達性、HTTP ステータス、ZIP の配置                          |
| 更新後に起動しない             | ZIP 内に `SOReplaceLabel.exe` があるか、依存 DLL が揃っているか              |
| ファイルが想定と違う場所に入る | ZIP 内に余分な親フォルダがないか                                             |
| アップデーターが見つからない   | インストール先に `SOReplaceUpdater.exe` があるか（初回インストールに含める） |
| 更新失敗後に古い状態に戻った   | ロールバックが動作した可能性。コンソールの ERROR ログを確認                  |
