using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace Survey.Models;

public class ApiResponse<T>
{
    [JsonPropertyName("success")]
    [JsonProperty("success")]
    public bool Success { get; set; }

    [JsonPropertyName("message")]
    [JsonProperty("message")]
    public string? Message { get; set; }

    [JsonPropertyName("error")]
    [JsonProperty("error")]
    public string? Error { get; set; }

    [JsonPropertyName("code")]
    [JsonProperty("code")]
    public string? Code { get; set; }

    [JsonPropertyName("surveyId")]
    [JsonProperty("surveyId")]
    public string? SurveyId { get; set; }

    [JsonPropertyName("data")]
    [JsonProperty("data")]
    public T? Data { get; set; }

    public static ApiResponse<T> SuccessResponse(string message, T? data = default, string? surveyId = null)
    {
        return new ApiResponse<T>
        {
            Success = true,
            Message = message,
            Data = data,
            SurveyId = surveyId
        };
    }

    public static ApiResponse<T> ErrorResponse(string error, string? code = null)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Error = error,
            Code = code
        };
    }
}
