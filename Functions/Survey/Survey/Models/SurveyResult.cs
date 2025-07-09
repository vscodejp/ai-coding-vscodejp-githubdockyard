using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace Survey.Models;

public class SurveyResult
{
    [JsonPropertyName("success")]
    [JsonProperty("success")]
    public bool Success { get; set; }

    [JsonPropertyName("data")]
    [JsonProperty("data")]
    public SurveyData? Data { get; set; }
}

public class SurveyData
{
    [JsonPropertyName("totalResponses")]
    [JsonProperty("totalResponses")]
    public int TotalResponses { get; set; }

    [JsonPropertyName("communityAffiliation")]
    [JsonProperty("communityAffiliation")]
    public Dictionary<string, int> CommunityAffiliation { get; set; } = new();

    [JsonPropertyName("jobRole")]
    [JsonProperty("jobRole")]
    public Dictionary<string, int> JobRole { get; set; } = new();

    [JsonPropertyName("eventRating")]
    [JsonProperty("eventRating")]
    public EventRatingData EventRating { get; set; } = new();

    [JsonPropertyName("feedback")]
    [JsonProperty("feedback")]
    public List<FeedbackItem> Feedback { get; set; } = new();
}

public class EventRatingData
{
    [JsonPropertyName("average")]
    [JsonProperty("average")]
    public double Average { get; set; }

    [JsonPropertyName("distribution")]
    [JsonProperty("distribution")]
    public Dictionary<string, int> Distribution { get; set; } = new();
}

public class FeedbackItem
{
    [JsonPropertyName("id")]
    [JsonProperty("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("feedback")]
    [JsonProperty("feedback")]
    public string Feedback { get; set; } = string.Empty;

    [JsonPropertyName("timestamp")]
    [JsonProperty("timestamp")]
    public DateTime Timestamp { get; set; }
}
