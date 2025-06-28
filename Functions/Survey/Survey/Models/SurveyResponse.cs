namespace Survey.Models;

/// <summary>
/// アンケート登録レスポンス用 DTO
/// </summary>
public class SurveyResponse
{
    /// <summary>
    /// 処理の成功/失敗
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// メッセージ
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// 登録されたアンケートのID
    /// </summary>
    public string? SurveyId { get; set; }

    /// <summary>
    /// 成功レスポンスを生成
    /// </summary>
    public static SurveyResponse CreateSuccess(string surveyId)
    {
        return new SurveyResponse
        {
            Success = true,
            Message = "アンケートの登録が完了しました",
            SurveyId = surveyId
        };
    }
}

/// <summary>
/// エラーレスポンス用 DTO
/// </summary>
public class ErrorResponse
{
    /// <summary>
    /// 処理の成功/失敗
    /// </summary>
    public bool Success { get; set; } = false;

    /// <summary>
    /// エラーメッセージ
    /// </summary>
    public string Error { get; set; } = string.Empty;

    /// <summary>
    /// エラーコード
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// バリデーションエラーレスポンスを生成
    /// </summary>
    public static ErrorResponse CreateValidationError(List<string> errors)
    {
        return new ErrorResponse
        {
            Error = string.Join(", ", errors),
            Code = "VALIDATION_ERROR"
        };
    }

    /// <summary>
    /// 内部エラーレスポンスを生成
    /// </summary>
    public static ErrorResponse CreateInternalError(string message = "内部エラーが発生しました")
    {
        return new ErrorResponse
        {
            Error = message,
            Code = "INTERNAL_ERROR"
        };
    }

    /// <summary>
    /// 不正リクエストエラーレスポンスを生成
    /// </summary>
    public static ErrorResponse CreateBadRequestError(string message = "リクエストが不正です")
    {
        return new ErrorResponse
        {
            Error = message,
            Code = "BAD_REQUEST"
        };
    }
}
