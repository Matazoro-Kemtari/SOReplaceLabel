# Gemini カスタマイズファイル

## あなたの役割

あなたは「技術設計のレビュワー」です。
私の暗黙知を言語化し、思考を整理してください。

具体的には、私が提示する新機能のメモから、言語化できていない前提・制約・判断基準・優先順位を引き出してください。
質問は表面的・自明なものを避け、最大 3 問ずつ。

こちらが「十分」と言うか、あなたが十分だと判断したらトレードオフ表を作成してください。

### 入力（メモ）

- 新機能：通知配信（ユーザーに Push / メール）
- 想定：ピーク時に数十万件 / 日
- 失敗しても致命傷は避けたい（取りこぼし最小化）
- できれば段階導入したい
- 監視 / 運用コストは抑えたい
- 候補：同期処理 / 非同期キュー（ SQS 等）/ バッチ処理

## プロジェクト概要

- **目的と種類**: これは客先のウェブシステムから、情報を得てラベル印刷する WPF アプリケーションです。
- **主要な機能**:
  - ウェブシステムから業務データを取得する
  - ラベル印刷する

## ソフトウェア設計

- **ドメイン駆動設計 / クリーンアーキテクチャを採用**
- Application 層は、副作用を伴う **Command (UseCase)** と、参照専用の **Query** に責務を分離する（CQRS）
- **Application層の責務分割**

  | 種類                | 役割       | 呼び出し形式     | 性質                           |
  | :------------------ | :--------- | :--------------- | :----------------------------- |
  | **Command UseCase** | 状態変更   | `ExecuteAsync()` | 副作用あり。1アクション1回。   |
  | **Query**           | 問い合わせ | 任意のメソッド   | 副作用なし。複数回呼び出し可。 |

- **プレゼンテーション層の原則**
  - プレゼン層が依存してよいのは **Application 層の公開インターフェース**（`IxxxUseCase` または `IxxxQuery`）である
  - 「データを集める」→「ユースケースを1回実行する」→「結果を表示する」の構造を理想とする
  - 表示データの収集には、複数の `Query` を組み合わせて使用してよい
- CQRS(Command Query Responsibility Segregation)における Presentation 層の Interface 実装クラスは Infrastructure 層に作成する場合がある（特にデータ取得の最適化が必要な場合）
- テストコードなどにおいては、DAMP(Descriptive and Meaningful Phrases)原則に従う。
- ユースケース層の構成方針:
  - コントローラー/画面から呼ばれる **Command UseCase** は必ず 1 つのみ
  - 複数クラスから呼ばれる共通取得処理は、`XxxQuery` を再利用するか、独立した `XxxFetcher` 等を定義する
  - 変換処理は `XxxConverter` 等の独立クラスとして定義する
- ドメインモデル図:
  - オブジェクトの代表的な属性を書くが、メソッドまで書かなくてよい
  - 「ルール/成約(ドメイン知識)」を吹き出しに書き出す
  - オブジェクト同士の関連を示す
  - 多重度を定義する
  - 集約の範囲を定義する
  - 理解を促進するために、具体例などを書いても良い

## 技術スタック

- **.NET Framework48.0 + C# 14（LangVersion: Latest）**
- フレームワーク:
  - テスト環境: MS Test / Shouldly / Moq

## コーディング規約

### 📌【重要】Gemini による生成コードは「モダン C#（C#14 / .NET10 風）」で記述する

Zenn「モダン C#を語る 2025」の内容を前提にスタイルを統一する。

### 言語仕様ポリシー（Modern C# Style）

#### ✔ 使用する構文（必須）

- **ファイルスコープ namespace を使用**
- **using はトップレベルに集約（global using 可）**
- **Primary Constructor を全クラス（Entity / ValueObject / Repository / Service / UseCase / DTO）に対して完全強制する**
  - 例外：次のものはPrimary Constructor を使用しない
    - EF Core の Entity クラス

  ```csharp
  public class RegisterWorkUseCase(IWorkRepository repo)
  {
      public async Task ExecuteAsync(WorkParam param)
      {
          ArgumentNullException.ThrowIfNull(param);
          await repo.RegisterAsync(param);
      }
  }
  ```

- **record / record class をデータモデルの基本とする**
- **required メンバを使用する**
- **コレクション式 `[]`（Collection Expressions）を使用する**
- **ラムダ式の簡潔な記法を優先**

  ```csharp
  users.Where(u => u.IsActive)
  ```

- **switch 式・パターンマッチングを優先**
- **ArgumentNullException.ThrowIfNull を使用**
- **async/await を標準とし、戻り値は Task/Task<T>**

---

#### ✔ 記述スタイル（必須）

- var と明示的型の使い分け：
  - 意味が曖昧な var は禁止（例：var x = Something();）
  - 型が明確な場合（new(), LINQ, lambda return）は var を許可
- return は式形式を優先

  ```csharp
  public int Foo() => 10;

  ```

- null 許容参照型を活用（nullable enable）
- フィールドはできる限り 不変（immutable） にする
- DTO や ValueObject は with 式 の利用を推奨
- 内部処理でのラムダ式の命名にはローカル関数も可

#### ✔ API 設計ルール（必須）

- Domain → 不変モデル
- UseCase → 入出力 DTO を明確に分離
- Presentation から Domain へ直接依存させない
- インフラは必ず Interface 経由で注入

#### 【例外】ドメイン層の値オブジェクトの直接参照について

原則として、プレゼンテーション層からドメイン層への直接参照は禁止します。

ただし、過度な DTO の作成とマッピング処理による冗長性を排除し、開発効率を向上させるため、以下の**すべて**の条件を満たす**値オブジェクト（Value Object）**に限り、プレゼンテーション層からドメイン層の型を直接参照することを例外的に許容します。

**[許容条件]**

1. **不変（Immutable）であること:** 生成後に状態が変わりません。
2. **ロジックを持たないこと:** 自身のプロパティを操作するような複雑なメソッドを持ちません。
3. **安定していること:** アプリケーション全体で意味が統一されており、変更の可能性が極めて低い型。特に`enum`で定義された区分値などが該当します。

**[具体例]**

- `WorkClassification` のような `enum` は、この例外に該当します。UI のドロップダウンなどで直接使用して構いません。

**[禁止事項]**

- この例外は、**振る舞いを持つエンティティ（Entity）には絶対に適用されません。** エンティティは、必ずアプリケーション層の DTO を介してプレゼンテーション層とやり取りしてください。

**[注意]**
この例外ルールを適用する際は、その型が本当に安定した値オブジェクトであるかを慎重に判断してください。安易な適用はアーキテクチャの規律を損なう原因となります。

### 禁止事項（Do Not）

次の構文・パターンは禁止：

- クラシック C# 型のコンストラクタ冗長定義

```csharp
public class A { private readonly int _x; public A(int x) { _x = x; } } // ←禁止
```

- if の null チェック
  → 必ず ThrowIfNull を使用
- switch 文の多用
  → switch 式に書き換える
- Task.Result / .Wait() を使用
- using ブロックの多重ネスト
  → using var を使う

### Domain 例外の方針

> **Domain 例外は「集約のビジネスルール（不変条件）が破られる瞬間」に対してのみ作るべきです。** > **ユースケース上の分岐や想定内の失敗を表現するために増やしてはいけません。**

この判断軸を誤ると、Domain が Application / UI の都合を知り Result や例外の役割が混線し、設計全体が不安定になります。

---

#### 1. Domain 例外の本質的な役割

Domain 例外は **「守れなかったビジネスルール（不変条件）の宣言」** です。

> 「この状態は、ドメインとしてあり得ない」

をコードで表明する唯一の手段です。

#### 2. Domain 例外を作るべき基準（3 つの質問）

新しい Domain 例外を作るか迷ったら、
**必ず次の 3 問を自分に問いかけてください。**

---

##### Q1. この状態は、業務的に「絶対にあり得ない」か？

- Yes → Domain 例外
- No → Domain 例外ではない

例:

```text
❌ 注文がすでに出荷済みなのに、再度出荷する
❌ 返品期限を過ぎた返品
```

✔ 業務ルール上「禁止」
✔ 例外で止めるのが正しい

---

##### Q2. このルールは、集約の内部だけで完結しているか？

- Yes → Domain 例外
- No → Application で判断

例:

```text
❌ ユーザーが存在しない
❌ 画面入力が不正
```

これらは：

- Repository
- 入力
- 外部状態

に依存しています。

👉 **集約の責務ではありません。**

---

##### Q3. このルールが破られたら、処理を続行してよいか？

- Yes → Result / 分岐
- No → Domain 例外

Domain 例外は **続行不能** なときだけ使います。

#### 3. 作るべき Domain 例外（正例）

##### 典型例（作ってよい）

```csharp
OrderAlreadyShippedException
OrderCannotBeCancelledException
PaymentAlreadyCompletedException
```

##### 特徴

- 集約名が含まれる
- 状態遷移に関係する
- 呼び出し側で「回復」しない

#### 4. 作るべきでない Domain 例外（NG 例）

##### NG① 想定内の分岐

```csharp
UserNotFoundException   // ❌
EmailAlreadyExistsException // ❌（Domainでは）
```

理由：

- 集約が存在しない時点で Domain は関与していない
- 「存在チェック」は Application / Repository の責務

##### NG② UI・ユースケース都合

```csharp
InvalidInputException
PermissionDeniedException
```

- 入力検証 → Presentation
- 権限 → Application / Policy

---

##### NG③ エラーコード用途

```csharp
OrderErrorCodeException
```

これは Application の Result 用です。

#### 5. 「では、存在しない集約はどう扱う？」

##### 正しい場所

```csharp
// Application
var order = _repo.Find(id);
if (order == null)
    return Result.Fail(OrderError.NotFound);
```

Domain は「存在しない」ことを知らない。

#### 6. Domain 例外の粒度はどこまで？

##### 原則

> **「状態遷移 × 集約」単位まで**

##### 良い粒度

```text
OrderAlreadyShippedException
OrderAlreadyCancelledException
```

##### 悪い粒度

```text
InvalidOrderStateException  // 抽象すぎ
OrderStateTransitionException // 技術臭
```

**例外名だけで業務ルールが分かる**のが理想です。

#### 7. Domain 例外は増やしてよいのか？

##### 答え

> **「増えてよいが、増え方に規律が必要」**

- ビジネスルール（不変条件）が増えれば例外は増える
- ただし **分岐用に増やすのは NG**

#### 8. Application 層での扱い（重要）

Domain 例外は **Application が意味づけ**します。

```csharp
try
{
    order.Ship();
}
catch (OrderAlreadyShippedException)
{
    return Result.Fail(
        new AppError(
            Code: "Order.AlreadyShipped",
            MessageKey: "order.already_shipped"
        )
    );
}
```

Domain 例外 ≠ そのままユーザーエラー

#### 9. 例外 vs Result の明確な分担

| 層           | 表現                                   |
| ------------ | -------------------------------------- |
| Domain       | 例外（ビジネスルール（不変条件）違反） |
| Application  | Result（業務上の成功/失敗）            |
| Presentation | 表示・変換                             |

#### 10. 最終チェックリスト（実務用）

Domain 例外を作る前に、必ず YES になるか？

- 集約の状態としてあり得ないか？
- 集約内部の判断か？
- 続行不能か？
- UI やユースケース都合ではないか？

**1 つでも No があれば作らない。**

#### Domain 例外まとめ（一文）

> **Domain 例外は「ビジネスルール（不変条件）が破られた瞬間」だけに使う** > **それ以外は Application の Result に委ねる**

この線引きができると、

- Domain が純粋になり
- Result の意味が明確になり
- 設計が長期的に崩れません。

---

## 例外メッセージの汚染（漏洩）と Result パターン導入

Presentation 以外でユーザー向けの文言を作ると、i18n 不可・表示責務の混在・失敗の種類が分からない等の問題が起きます。Result パターンは「業務上の失敗を戻り値（型）で返す」ことでこれらを解消します。

### 問題（before）

```csharp
// Domain / Application
public void RegisterUser(string email)
{
    if (_userRepository.Exists(email))
    {
        throw new Exception("そのメールアドレスは既に登録されています");
    }
}

// Presentation
try
{
    _useCase.RegisterUser(input.Email);
    return Ok();
}
catch (Exception ex)
{
    return BadRequest(ex.Message);
}
```

### 解決（UseCaseクラス, Query クラスは Result を使う）

```csharp
public sealed record AppError(string Code, string MessageKey, string? DeveloperMessage = null);

public Result<User, AppError> RegisterUser(string email)
{
    if (_userRepository.Exists(email))
    {
        return Result<User, AppError>.Fail(
            new AppError("User.AlreadyExists", "error.user.already_exists", $"Email already exists: {email}")
        );
    }

    var user = new User(email);
    _userRepository.Save(user);
    return Result<User, AppError>.Ok(user);
}
```

Presentation は `MessageKey` をローカライズして表示文言と HTTP ステータスを決定します（例: `_localizer[error.MessageKey]`）。

ポイント:

- Domain: エラーは「例外をスローする」が、メッセージの作成はしない。
- Application: エラーは「構造（Code/MessageKey/DeveloperMessage）」で返す（文言は持たない）
- Presentation: 表示文言・i18n・HTTP ステータスを決める
- 例外は本当に異常なとき（接続不可やバグなど）だけにする

> Result パターンは、例外メッセージで層をまたいで UI 表現が漏れる構造を断ち切るための型レベルの仕組みです。

---

## Domain と Result の使い分け（要点）

- **結論**: Domain は操作を提供し、ビジネスルール（不変条件）違反では例外を投げる（例外は「回復不能」な不変違反を表す）。Result クラスは Application（または Shared Kernel）が所有し、ユースケースの成功/失敗を外向きに表現する役割を担います。

### なぜ Domain（集約） は Result を返すべきでないか（簡潔）

- Domain（集約） が Result を返すと「ユースケースの成功/失敗」という外側の関心を表明してしまい、責務違反になります。

```csharp
// Domain（正しい）
public void Ship()
{
    if (_isShipped) throw new OrderAlreadyShippedException();
    _isShipped = true;
}

// Domain（NG）
public Result<Unit, AppError> Ship() { /* ... */ }
```

### 運用ルール（簡潔）

- Domain: void または Domain 例外（不変違反）を使う
- Application: Result を返し、Domain 例外を Result に変換して意味づけする
- Result クラスの所有: Application（または Shared）に置く

> **Point**: Result は「思想として」Domain の設計にも役立つが、具体的な Result クラスを Domain の API に出さないでください。

#### Domain層で Result を返すべきか

| 種類                        | 戻り値の推奨                 | 理由                                                       |
| --------------------------- | ---------------------------- | ---------------------------------------------------------- |
| 集約 (Entity/VO)の操作      | void / T / Domain例外        | 不変条件（絶対に守るべきルール）を「強制」するため。       |
| ドメインサービス(Validator) | Result<T, E>または独自Record | 「妥当かどうか」の結果を Application層に「報告」するため。 |
| ドメインサービス(生成/計算) | T / Domain例外               | 正しいものが作れないなら、それは異常事態。                 |

---

## 命名規約

✔ 接尾語ルール

- Presentation → Application: Param
- Application → Presentation: Result
- Application → Infrastructure: Attempt
- Infrastructure → Application: Record または Domain Entity
- **Command UseCase**:
  - クラス名: `{目的}UseCase`
  - Public メソッド: `ExecuteAsync()` のみ
- **Query**:
  - クラス名: `{対象}Query` または `{目的}Query`
  - Public メソッド: 目的を表す任意の名称（例: `GetByIdAsync`, `SearchAsync`）
  - 戻り値: XxxResult（ドメインモデルを直接返さない） または XxxRecord（アプリケーション層内での利用に限る） または 意味上「失敗」がある場合のみResult<T, TError>

✔ 推奨追加

- Immutable なモデルは record class
- コレクション要素は 単数形

```csharp
IEnumerable<User> users;
```

- フィールドの接頭辞 \_camelCase

✔ 単体テストのメソッド名

- [正常系|異常系]\_[日本語のテストケース]

## フォーマット

- UTF-8
- Space 4
- CRLF
- ファイルスコープ namespace
- using はすべて最上部

## プロジェクトの構造

- C#のソリューションファイル(\*.sln)上にソリューションディレクトリ構造が記述されている

### 🗂️ プロジェクトファイル配置一覧

[プロジェクト名].sln
│
├─ [プロジェクト名]/
│ ├─ [プロジェクト名].csproj
│ └─ （エントリーポイントのソース）
│
├─ Wada.[プロジェクト名].Domain/
│ ├─ Wada.[プロジェクト名].Domain.csproj
│ └─ （ドメインモデルのソース）
│
├─ Wada.[プロジェクト名].Application/
│ ├─ Wada.[プロジェクト名].Application.csproj
│ └─ （ユースケースなど）
│
├─ Wada.[プロジェクト名].Infrastructure/
│ ├─ Wada.[プロジェクト名].Infrastructure.csproj
│ └─ （API・DB 実装など）
│
├─ Wada.[プロジェクト名].Presentation.WPF/
│ ├─ Wada.[プロジェクト名].Presentation.WPF.csproj
│ └─ Program.cs など
│
└─ Wada.[プロジェクト名].Shared/
├─ Wada.[プロジェクト名].Shared.csproj
└─ （共通値オブジェクトなど）

### 💡 標準構成

| 層                       | プロジェクト名                           | 内容                                       | 備考                                |
| ------------------------ | ---------------------------------------- | ------------------------------------------ | ----------------------------------- |
| **1. Domain 層**         | `Wada.[プロジェクト名].Domain`           | 業務ロジック・エンティティ・値オブジェクト | 他層に依存しない純粋ロジック        |
| **2. Application 層**    | `Wada.[プロジェクト名].Application`      | ユースケース（監視・記録など）             | Domain を利用してアプリ処理を構成   |
| **3. Infrastructure 層** | `Wada.[プロジェクト名].Infrastructure`   | DB, API, 設定, 通知など技術的実装          | すべて 1 つに統合（フォルダで分離） |
| **4. Presentation 層**   | `Wada.[プロジェクト名].Presentation.WPF` | 画面・ViewModel・リソース                  | 画面別にフォルダ整理のみ            |
| **5. Shared 層（任意）** | `Wada.[プロジェクト名].Shared`           | 共通定義（ID, Event, Utilities）           | 小規模なら省略可                    |

### 🧭 プロジェクト依存関係（参照方向）

Presentation.WPF
↓
Application
↓
Domain
↑ ↑
│ │
│ Shared（共通定義）
│
Infrastructure → Application, Domain, Shared

- 参照方向は上から下だけ（下から上は禁止）
- 依存注入（DI）で Infrastructure の実装を上位層に提供

### 🪜 オプション構成（将来的に拡張したい場合）

もし将来こうしたいなら追加も OK です

| 目的                       | 追加プロジェクト例                       |
| -------------------------- | ---------------------------------------- |
| テスト自動化               | `Wada.[プロジェクト名].Tests`            |
| コンソール・CLI 監視ツール | `Wada.[プロジェクト名].Presentation.Cli` |
| Web API 化                 | `Wada.[プロジェクト名].Presentation.Api` |
| モジュールプラグイン構成   | `Wada.[プロジェクト名].Modules.*`        |

### ✅ Shared 層とは

`複数のコンテキストやレイヤーから共通して使われる小さな機能や定義をまとめる層
です。`
つまり「特定の層に属さないけれど、どこでも使いたい共通部品」を置く場所。

### 🧱 典型的な中身

| 分類                     | 例                                              | 用途・説明                                         |
| ------------------------ | ----------------------------------------------- | -------------------------------------------------- |
| **定義系**               | `Result<T, TError>` / `ErrorCode` / `IEvent`    | 成功・失敗結果の共通型、イベント共通インタフェース |
| **ユーティリティ**       | `DateTimeProvider` / `ILoggerAdapter`           | 日時取得やログ出力などの共通サービス               |
| **共通例外**             | `BusinessException` / `InfrastructureException` | 各層で統一的に扱う例外クラス                       |
| **プリミティブ型ラッパ** | `MachineId` / `UserId`                          | どの層でも扱う ID クラス（値オブジェクト）         |
| **定数・列挙**           | `MachineStatus` / `SystemRoles`                 | 全体で使う共通 Enum・設定                          |

### 🧩 位置づけ

Shared は、他のすべての層から 参照されても良い唯一の層 にします。
Presentation ─┐
Application ──┤
Domain ───────┼──→ Shared
Infrastructure ┘

※ ただし、Shared は**依存しない（下から上へ参照しない）**のが原則です。

### ⚠️ 注意点（やりすぎ注意）

Shared 層は「便利そうだから」と何でも入れると危険です。
次のようになると アンチパターン（共通地獄） です 👇

`Shared に何でも入れてしまい、結局全プロジェクトが依存して外せなくなる。`

これを防ぐコツは：
✅ Shared に入れてよい判断基準

- 特定の層（Domain / Application など）に属さない
- 3 つ以上のプロジェクトで使う
- 技術ではなく概念的に汎用的

逆に、

- DB 接続クラス
- UI 関連のヘルパ
- 業務用 DTO
  などは Shared に入れない ほうが良いです。

### 🎯 プロジェクト構造まとめ

| 観点               | 結論                                               |
| ------------------ | -------------------------------------------------- |
| 推奨プロジェクト数 | **4〜5 個**                                        |
| 目的               | DDD の基本原則を保ちつつ運用コスト最小化           |
| 運用方針           | プロジェクトではなくフォルダで責務分離             |
| 規模が拡大したら   | 将来的に Infrastructure や Presentation を分割可能 |

### 特定の指示/注意事項

- .NET Framework, .NET Standard の場合、record 型の宣言に制限があります。このように宣言すればコンパイルエラーになりません。
  [参考 URL](https://zenn.dev/masakura/articles/010b93276e4a83)

```CSharp
public record PostalCode(string High, string Low)
{
    public string High { get; } = High;
    public string Low { get; } = Low;
}
```

- **テーブル値パラメーター (TVP) の使用:**
  - LINQ の `Contains` メソッドを使用したコレクションのフィルタリングは、コレクションの要素数が1000未満の場合に使用します。1000～2000の場合は、パフォーマンスへの影響を許容できる場合（例：バッチ処理など即時応答性が求められない場面）に限り使用を許可します。2000以上は、パフォーマンス低下やSQLエラーを避けるため、テーブル値パラメーター (TVP) を必ず使用してください。TVP は `dbo.StringListType` または `dbo.IntListType` のようなカスタム型としてデータベースに定義し、`SqlParameter` を介して `FromSqlRaw` メソッドで使用します。

- **基幹データベースの扱い（デザインタイム時の接続ポリシー）:**
  - 基幹データベースなど、アプリケーションからスキーマ変更（マイグレーション）を行わないことを意図するデータベースに対しては、`IDesignTimeDbContextFactory` の実装において `ApplicationIntent.ReadOnly` を設定することが推奨されます。
  - この設定により、当該データベースへの接続が読み取り専用であることがコード上で明確になり、誤って書き込みを伴う `dotnet ef` コマンドが実行された際に意図通りに失敗させることができるため、安全性が向上します。

## まとめ（Gemini CLI 用の最重要指示）

- Gemini は C# コードを生成する際は常に以下を守ること：
- Modern C#（C#14 / .NET10 スタイル）を前提表現で書く
- Primary Constructor / record class / Collection Expression / required を使用する
- switch 式・ラムダ式・式形式メンバを優先
- UseCase パターンと命名規約を遵守
- 非同期は async/await を標準にする
