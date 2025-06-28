namespace Survey.Models;

/// <summary>
/// アンケート集計結果用 DTO
/// </summary>
public class SurveyResultResponse
{
    /// <summary>
    /// 処理の成功/失敗
    /// </summary>
    public bool Success { get; set; } = true;

    /// <summary>
    /// 集計データ
    /// </summary>
    public SurveyResultData Data { get; set; } = new();
}

/// <summary>
/// 集計データ
/// </summary>
public class SurveyResultData
{
    /// <summary>
    /// 総回答数
    /// </summary>
    public int TotalResponses { get; set; }

    /// <summary>
    /// コミュニティ所属の集計
    /// </summary>
    public Dictionary<string, int> CommunityAffiliation { get; set; } = new();

    /// <summary>
    /// 職種の集計
    /// </summary>
    public Dictionary<string, int> JobRole { get; set; } = new();

    /// <summary>
    /// イベント評価の集計
    /// </summary>
    public EventRatingData EventRating { get; set; } = new();

    /// <summary>
    /// フィードバック一覧
    /// </summary>
    public List<FeedbackData> Feedback { get; set; } = new();
}

/// <summary>
/// イベント評価の集計データ
/// </summary>
public class EventRatingData
{
    /// <summary>
    /// 平均評価
    /// </summary>
    public double Average { get; set; }

    /// <summary>
    /// 評価の分布
    /// </summary>
    public Dictionary<string, int> Distribution { get; set; } = new();
}

/// <summary>
/// フィードバックデータ
/// </summary>
public class FeedbackData
{
    /// <summary>
    /// アンケートID
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// フィードバック内容
    /// </summary>
    public string Feedback { get; set; } = string.Empty;

    /// <summary>
    /// タイムスタンプ
    /// </summary>
    public DateTime Timestamp { get; set; }
}
