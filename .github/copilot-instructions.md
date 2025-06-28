# Copilot Instructions for ai-coding-vscodejp-githubdockyard

## コアコマンド
- **フロントエンド**
  - ビルド: `npm run build`（`frontend/` ディレクトリ、Azure Static Web Apps CI/CD で `build` ディレクトリ生成）
  - デプロイ: プッシュ時に GitHub Actions で自動デプロイ
- **バックエンド**
  - ビルド: `dotnet build --configuration Release`（`Functions/Survey`）
  - パブリッシュ: `dotnet publish --configuration Release --output ./publish`
  - デプロイ: GitHub Actions で `release.zip` を Azure Functions へ自動デプロイ
- **CI/CD**
  - `.github/workflows/azure-static-web-apps-purple-sea-093b8b000.yml`（フロントエンド）
  - `.github/workflows/deploy-function.yml`（バックエンド）

## アーキテクチャ概要
- **フロントエンド**: React（`frontend/`）
- **バックエンド**: .NET 9 Azure Functions（`Functions/Survey`）
- **データベース**: Azure Cosmos DB (NoSQL)
- **API**: REST（`/api/surveys`, `/api/surveys/results`）
- **デプロイ**: Azure Static Web Apps（フロント）、Azure Functions（バック）

## スタイル・コーディング規約
- **TypeScript/JavaScript**
  - 型安全を重視（型注釈推奨）
  - インデントはスペース2 or 4（プロジェクト設定に従う）
  - import順序は標準→外部→内部
  - 命名: キャメルケース（変数/関数）、パスカルケース（型/クラス）
  - エラー処理は明示的に
- **C# (.NET)**
  - 型明示、PascalCase命名
  - 非同期処理は `async`/`await`
  - 例外処理は try-catch で明示
- **共通**
  - コメントは日本語可
  - バリデーション・エラーハンドリングはAPI仕様に準拠

## 重要な仕様・ルール（README要約）
- **アンケートAPI**
  - POST `/surveys`: 回答登録
  - GET `/surveys/results`: 集計取得
  - 必須: `communityAffiliation`（配列, 空配列可）, `jobRole`（配列, 1つ以上）, `eventRating`（1-5）
  - オプション: `jobRoleOther`（"その他"時必須, 100字以内）, `feedback`（1000字以内）
  - レスポンス: `success`, `message`/`error`, `surveyId`/`code`
  - ステータス: 200, 201, 400, 422, 500
- **データモデル**
  - `Survey`型（README参照）

## その他
- 追加のエージェントルールや独自Copilotファイルは現時点で未検出
- 詳細はREADME.md参照
