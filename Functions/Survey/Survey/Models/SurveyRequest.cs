using System.ComponentModel.DataAnnotations;

namespace Survey.Models;

/// <summary>
/// アンケート登録リクエスト用 DTO
/// </summary>
public class SurveyRequest
{
    /// <summary>
    /// コミュニティ所属
    /// </summary>
    [Required(ErrorMessage = "コミュニティ所属は必須です")]
    public List<string> CommunityAffiliation { get; set; } = new();

    /// <summary>
    /// 職種
    /// </summary>
    [Required(ErrorMessage = "職種は必須です")]
    [MinLength(1, ErrorMessage = "職種は1つ以上選択してください")]
    public List<string> JobRole { get; set; } = new();

    /// <summary>
    /// その他の職種
    /// </summary>
    [MaxLength(100, ErrorMessage = "その他の職種は100文字以内で入力してください")]
    public string? JobRoleOther { get; set; }

    /// <summary>
    /// イベント評価（1-5）
    /// </summary>
    [Required(ErrorMessage = "イベント評価は必須です")]
    [Range(1, 5, ErrorMessage = "イベント評価は1から5の間で選択してください")]
    public int EventRating { get; set; }

    /// <summary>
    /// フィードバック
    /// </summary>
    [MaxLength(1000, ErrorMessage = "フィードバックは1000文字以内で入力してください")]
    public string Feedback { get; set; } = string.Empty;

    /// <summary>
    /// リクエストの妥当性をチェック
    /// </summary>
    public bool IsValid(out List<string> errors)
    {
        errors = new List<string>();

        // コミュニティ所属の値チェック
        var validCommunities = new[] { "VS Code Meetup", "GitHub dockyard" };
        if (CommunityAffiliation.Any(c => !string.IsNullOrEmpty(c) && !validCommunities.Contains(c)))
        {
            errors.Add("無効なコミュニティが選択されています");
        }

        // 職種の値チェック
        var validJobRoles = new[]
        {
            "フロントエンドエンジニア", "バックエンドエンジニア", "フルスタックエンジニア",
            "DevOpsエンジニア", "データエンジニア", "モバイルエンジニア", "その他"
        };
        if (JobRole.Any(jr => !validJobRoles.Contains(jr)))
        {
            errors.Add("無効な職種が選択されています");
        }

        // 「その他」が選択されている場合のチェック
        if (JobRole.Contains("その他") && string.IsNullOrWhiteSpace(JobRoleOther))
        {
            errors.Add("職種で「その他」を選択した場合は、具体的な職種を入力してください");
        }

        return !errors.Any();
    }

    /// <summary>
    /// Survey エンティティに変換
    /// </summary>
    public Models.Survey ToEntity()
    {
        return new Models.Survey
        {
            Id = Guid.NewGuid().ToString(), // 明示的に ID を設定
            CommunityAffiliation = CommunityAffiliation,
            JobRole = JobRole,
            JobRoleOther = JobRoleOther,
            EventRating = EventRating,
            Feedback = Feedback,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }
}
