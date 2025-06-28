// Survey型定義（API仕様準拠）

export type Survey = {
  communityAffiliation: string[]; // コミュニティ所属（空配列可）
  jobRole: string[]; // 職種（1つ以上必須）
  eventRating: number; // イベント評価（1-5）
  jobRoleOther?: string; // 職種「その他」時の自由記述（100字以内）
  feedback?: string; // フィードバック（1000字以内）
};

export type SurveyPostResponse = {
  success: boolean;
  message?: string;
  error?: string;
  surveyId?: string;
  code?: string;
};

export type SurveyResult = {
  total: number;
  jobRoleStats: Record<string, number>;
  eventRatingStats: Record<number, number>;
  // 必要に応じて追加
};
