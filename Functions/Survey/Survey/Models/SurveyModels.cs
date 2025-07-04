using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Survey.Models;

/// <summary>
/// アンケート回答リクエストモデル
/// </summary>
public class SurveyRequest
{
    [JsonPropertyName("communityAffiliation")]
    [Required(ErrorMessage = "コミュニティ所属は必須です")]
    public string[] CommunityAffiliation { get; set; } = Array.Empty<string>();

    [JsonPropertyName("jobRole")]
    [Required(ErrorMessage = "職種は必須です")]
    [MinLength(1, ErrorMessage = "職種は1つ以上選択してください")]
    public string[] JobRole { get; set; } = Array.Empty<string>();

    [JsonPropertyName("jobRoleOther")]
    [MaxLength(100, ErrorMessage = "その他の職種は100文字以内で入力してください")]
    public string? JobRoleOther { get; set; }

    [JsonPropertyName("eventRating")]
    [Required(ErrorMessage = "イベント評価は必須です")]
    [Range(1, 5, ErrorMessage = "イベント評価は1-5の範囲で入力してください")]
    public int EventRating { get; set; }

    [JsonPropertyName("feedback")]
    [MaxLength(1000, ErrorMessage = "フィードバックは1000文字以内で入力してください")]
    public string? Feedback { get; set; }
}

/// <summary>
/// Cosmos DBに保存するアンケートエンティティ
/// </summary>
public class SurveyEntity
{
    [JsonPropertyName("id")]
    public string id { get; set; } = Guid.NewGuid().ToString();

    [JsonPropertyName("communityAffiliation")]
    public string[] CommunityAffiliation { get; set; } = Array.Empty<string>();

    [JsonPropertyName("jobRole")]
    public string[] JobRole { get; set; } = Array.Empty<string>();

    [JsonPropertyName("jobRoleOther")]
    public string? JobRoleOther { get; set; }

    [JsonPropertyName("eventRating")]
    public int EventRating { get; set; }

    [JsonPropertyName("feedback")]
    public string? Feedback { get; set; }

    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [JsonPropertyName("updatedAt")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Cosmos DB のパーティションキー用（eventDate）
    [JsonPropertyName("eventDate")]
    public string EventDate { get; set; } = DateTime.UtcNow.ToString("yyyy-MM-dd");
}

/// <summary>
/// API成功レスポンスモデル
/// </summary>
public class SurveyResponse
{
    [JsonPropertyName("success")]
    public bool Success { get; set; } = true;

    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    [JsonPropertyName("surveyId")]
    public string? SurveyId { get; set; }
}

/// <summary>
/// APIエラーレスポンスモデル
/// </summary>
public class ErrorResponse
{
    [JsonPropertyName("success")]
    public bool Success { get; set; } = false;

    [JsonPropertyName("error")]
    public string Error { get; set; } = string.Empty;

    [JsonPropertyName("code")]
    public string Code { get; set; } = string.Empty;
}

/// <summary>
/// 許可されたコミュニティ一覧
/// </summary>
public static class CommunityConstants
{
    public static readonly string[] AllowedCommunities = 
    {
        "VS Code Meetup",
        "GitHub dockyard"
    };
}

/// <summary>
/// 許可された職種一覧
/// </summary>
public static class JobRoleConstants
{
    public static readonly string[] AllowedJobRoles = 
    {
        "フロントエンドエンジニア",
        "バックエンドエンジニア", 
        "フルスタックエンジニア",
        "DevOpsエンジニア",
        "データエンジニア",
        "モバイルエンジニア",
        "その他"
    };
}
