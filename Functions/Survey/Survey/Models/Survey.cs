using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace Survey.Models;

/// <summary>
/// Survey エンティティクラス - Cosmos DB に保存されるデータ構造
/// </summary>
public class Survey
{
    /// <summary>
    /// Cosmos DB のドキュメント ID
    /// </summary>
    [JsonPropertyName("id")]
    [JsonProperty("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// コミュニティ所属
    /// </summary>
    [JsonPropertyName("communityAffiliation")]
    [Required(ErrorMessage = "コミュニティ所属は必須です")]
    public List<string> CommunityAffiliation { get; set; } = new();

    /// <summary>
    /// 職種
    /// </summary>
    [JsonPropertyName("jobRole")]
    [Required(ErrorMessage = "職種は必須です")]
    [MinLength(1, ErrorMessage = "職種は1つ以上選択してください")]
    public List<string> JobRole { get; set; } = new();

    /// <summary>
    /// その他の職種（jobRoleに「その他」が含まれている場合に入力）
    /// </summary>
    [JsonPropertyName("jobRoleOther")]
    [MaxLength(100, ErrorMessage = "その他の職種は100文字以内で入力してください")]
    public string? JobRoleOther { get; set; }

    /// <summary>
    /// イベント評価（1-5）
    /// </summary>
    [JsonPropertyName("eventRating")]
    [Required(ErrorMessage = "イベント評価は必須です")]
    [Range(1, 5, ErrorMessage = "イベント評価は1から5の間で選択してください")]
    public int EventRating { get; set; }

    /// <summary>
    /// フィードバック
    /// </summary>
    [JsonPropertyName("feedback")]
    [MaxLength(1000, ErrorMessage = "フィードバックは1000文字以内で入力してください")]
    public string Feedback { get; set; } = string.Empty;

    /// <summary>
    /// 作成日時
    /// </summary>
    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 更新日時
    /// </summary>
    [JsonPropertyName("updatedAt")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// イベント日付（パーティションキー用）
    /// </summary>
    [JsonPropertyName("eventDate")]
    public string EventDate { get; set; } = DateTime.UtcNow.ToString("yyyy-MM");

    /// <summary>
    /// Cosmos DB パーティションキー（互換性のため残存、削除予定）
    /// </summary>
    [JsonPropertyName("partitionKey")]
    public string PartitionKey { get; set; } = DateTime.UtcNow.ToString("yyyy-MM");
}
