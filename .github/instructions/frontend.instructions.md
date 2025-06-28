---
applyTo: 'frontend/**'
---

アンケートアプリ フロントエンド実装指針
1. 画面・機能要件
トップページ

アンケート回答フォーム
必須: communityAffiliation（配列, 空配列可）, jobRole（配列, 1つ以上）, eventRating（1-5）
オプション: jobRoleOther（"その他"時必須, 100字以内）, feedback（1000字以内）
入力バリデーション（zod＋react-hook-form）
送信時にAPI（POST /api/surveys）へリクエスト
送信後は完了画面を表示
集計結果ページ

API（GET /api/surveys/results）から集計データ取得
回答集計をグラフやリストで表示
共通

APIエラーやバリデーションエラーの明示的な表示
2. 技術・設計方針
UI: React + TypeScript + MUI（@mui/material）
フォーム: react-hook-form + zod
API通信・状態管理: @tanstack/react-query
ルーティング: 必要に応じてreact-router-dom
型定義: Survey型をAPI仕様に準拠して定義
Lint/Format: ESLint, Prettier
3. ディレクトリ構成例
4. 実装手順
Survey型・APIクライアントの型定義
SurveyFormコンポーネント作成（バリデーション含む）
API連携（送信・取得）
集計結果表示コンポーネント作成
ルーティング・UI調整
エラー・ローディング対応
ESLint/Prettier整備
