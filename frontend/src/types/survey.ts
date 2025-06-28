// Survey型定義（API仕様準拠）

export type Survey = {
  id?: string;
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
  feedback?: string;
  createdAt?: Date;
  updatedAt?: Date;
};

export type SurveyPostResponse = {
  success: boolean;
  message?: string;
  error?: string;
  surveyId?: string;
  code?: string;
};

// 集計APIのレスポンス構造に合わせて型を定義
export type SurveyResult = {
  success: boolean;
  data: {
    totalResponses: number;
    communityAffiliation: Record<string, number>;
    jobRole: Record<string, number>;
    eventRating: {
      average: number;
      distribution: Record<string, number>;
    };
    feedback: Array<{
      id: string;
      feedback: string;
      timestamp: string;
    }>;
  };
};
