using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace Survey.Models;

public class Survey
{
    [JsonPropertyName("id")]
    [JsonProperty("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [JsonPropertyName("eventDate")]
    [JsonProperty("eventDate")]
    public string EventDate { get; set; } = "2025-07-09"; // パーティションキー

    [JsonPropertyName("communityAffiliation")]
    [JsonProperty("communityAffiliation")]
    [Required]
    public string[] CommunityAffiliation { get; set; } = Array.Empty<string>();

    [JsonPropertyName("jobRole")]
    [JsonProperty("jobRole")]
    [Required]
    [MinLength(1, ErrorMessage = "職種は1つ以上選択してください")]
    public string[] JobRole { get; set; } = Array.Empty<string>();

    [JsonPropertyName("jobRoleOther")]
    [JsonProperty("jobRoleOther")]
    [MaxLength(100, ErrorMessage = "その他の職種は100文字以内で入力してください")]
    public string? JobRoleOther { get; set; }

    [JsonPropertyName("eventRating")]
    [JsonProperty("eventRating")]
    [Required]
    [Range(1, 5, ErrorMessage = "イベント評価は1-5の範囲で入力してください")]
    public int EventRating { get; set; }

    [JsonPropertyName("feedback")]
    [JsonProperty("feedback")]
    [MaxLength(1000, ErrorMessage = "フィードバックは1000文字以内で入力してください")]
    public string? Feedback { get; set; }

    [JsonPropertyName("createdAt")]
    [JsonProperty("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [JsonPropertyName("updatedAt")]
    [JsonProperty("updatedAt")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // バリデーションメソッド
    public bool IsValid(out List<string> errors)
    {
        errors = new List<string>();

        if (CommunityAffiliation == null)
        {
            errors.Add("コミュニティ所属は必須です");
        }

        if (JobRole == null || JobRole.Length == 0)
        {
            errors.Add("職種は1つ以上選択してください");
        }

        if (JobRole?.Contains("その他") == true && string.IsNullOrWhiteSpace(JobRoleOther))
        {
            errors.Add("「その他」を選択した場合は具体的な職種を入力してください");
        }

        if (EventRating < 1 || EventRating > 5)
        {
            errors.Add("イベント評価は1-5の範囲で入力してください");
        }

        return errors.Count == 0;
    }
}
