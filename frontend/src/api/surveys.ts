import type { Survey, SurveyPostResponse, SurveyResult } from "../types/survey";

// アンケート送信API
export async function postSurvey(data: Survey): Promise<SurveyPostResponse> {
  const res = await fetch("/api/surveys", {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(data),
  });
  return await res.json();
}

// 集計取得API
export async function getSurveyResults(): Promise<SurveyResult> {
  const res = await fetch("/api/surveys/results");
  return await res.json();
}
