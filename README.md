# AI Codingを極める会 - VS Code Meetup × GitHub dockyard - トライアルリポジトリ

イベントページ: https://vscode.connpass.com/event/358179/

## プロジェクト概要

VS Code MeetupとGitHub dockyardの合同イベント「AI Codingを極める会」の事後アンケートシステムです。
GitHub Copilot Agentを活用して、バックエンドとフロントエンドのチームに分かれて開発します。

## API仕様書

### ベースURL

```
http://localhost:3000/api
```

### エンドポイント一覧

#### 1. アンケート登録API

**POST** `/surveys`

イベント参加者のアンケート回答を登録します。

**リクエストボディ**

```json
{
  "communityAffiliation": ["VS Code Meetup", "GitHub dockyard"],
  "jobRole": ["フロントエンドエンジニア", "バックエンドエンジニア", "フルスタックエンジニア", "DevOpsエンジニア", "データエンジニア", "モバイルエンジニア", "その他"],
  "jobRoleOther": "string",
  "eventRating": 1 | 2 | 3 | 4 | 5,
  "feedback": "string"
}
```

**フィールド説明**

- `communityAffiliation`: 複数選択可能な配列形式。参加者が所属しているコミュニティを全て選択。どちらでもない場合は空配列 `[]` を指定
- `jobRole`: 複数選択可能な配列形式。参加者の職種を全て選択（複数の職種を兼務している場合を考慮）
- `jobRoleOther`: `jobRole`に「その他」が含まれている場合に具体的な職種を入力（最大100文字）
- `eventRating`: 1-5の整数（1=非常に不満、5=非常に満足）
- `feedback`: 任意のフィードバック（最大1000文字）

**レスポンス**

```json
{
  "success": true,
  "message": "アンケートの登録が完了しました",
  "surveyId": "string"
}
```

**エラーレスポンス**

```json
{
  "success": false,
  "error": "エラーメッセージ",
  "code": "ERROR_CODE"
}
```

#### 2. 集計結果取得API

**GET** `/surveys/results`

アンケートの集計結果を取得します。

**レスポンス**

```json
{
  "success": true,
  "data": {
    "totalResponses": 50,
    "communityAffiliation": {
      "VS Code Meetup": 20,
      "GitHub dockyard": 15,
      "どちらでもない": 5
    },
    "jobRole": {
      "フロントエンドエンジニア": 15,
      "バックエンドエンジニア": 12,
      "フルスタックエンジニア": 8,
      "DevOpsエンジニア": 5,
      "データエンジニア": 4,
      "モバイルエンジニア": 3,
      "その他": 3
    },
    "eventRating": {
      "average": 4.2,
      "distribution": {
        "1": 1,
        "2": 2,
        "3": 8,
        "4": 20,
        "5": 19
      }
    },
    "feedback": [
      {
        "id": "string",
        "feedback": "string",
        "timestamp": "2025-06-24T10:00:00Z"
      }
    ]
  }
}
```

## データスキーマ

### Survey Model

```typescript
interface Survey {
  id: string;
  communityAffiliation: ("VS Code Meetup" | "GitHub dockyard")[];
  jobRole: (
    | "フロントエンドエンジニア"
    | "バックエンドエンジニア"
    | "フルスタックエンジニア"
    | "DevOpsエンジニア"
    | "データエンジニア"
    | "モバイルエンジニア"
    | "その他"
  )[];
  jobRoleOther?: string;
  eventRating: 1 | 2 | 3 | 4 | 5;
  feedback: string;
  createdAt: Date;
  updatedAt: Date;
}
```

## バリデーションルール

### 必須フィールド

- `communityAffiliation`: 必須（配列形式。どちらのコミュニティにも所属していない場合は空配列 `[]` を指定）
- `jobRole`: 必須（配列形式、1つ以上選択）
- `eventRating`: 必須（1-5の整数）

### オプショナルフィールド

- `jobRoleOther`: 任意（`jobRole`に「その他」が含まれている場合は必須、最大100文字）
- `feedback`: 任意（最大1000文字）

## HTTPステータスコード

- `200 OK`: 正常処理
- `201 Created`: リソース作成成功
- `400 Bad Request`: リクエストが不正
- `422 Unprocessable Entity`: バリデーションエラー
- `500 Internal Server Error`: サーバーエラー

## 開発環境

- **フロントエンド**: React 
- **バックエンド**: .NET 9 Azure Functions
- **データベース**: Azure Cosmos DB (NoSQL)

