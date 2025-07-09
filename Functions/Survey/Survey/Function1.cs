using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Survey.Models;
using Survey.Services;
using System.Text.Json;

namespace Survey;

public class SurveyFunctions
{
    private readonly ILogger<SurveyFunctions> _logger;
    private readonly ICosmosDbService _cosmosDbService;

    public SurveyFunctions(ILogger<SurveyFunctions> logger, ICosmosDbService cosmosDbService)
    {
        _logger = logger;
        _cosmosDbService = cosmosDbService;
    }

    [Function("CreateSurvey")]
    public async Task<IActionResult> CreateSurvey(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "surveys")] HttpRequest req)
    {
        _logger.LogInformation("アンケート登録APIが呼ばれました");

        try
        {
            // リクエストボディを読み取り
            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            
            if (string.IsNullOrWhiteSpace(requestBody))
            {
                _logger.LogWarning("リクエストボディが空です");
                return new BadRequestObjectResult(
                    ApiResponse<object>.ErrorResponse("リクエストボディが必要です", "EMPTY_BODY"));
            }

            // JSONデシリアライズ
            var survey = JsonSerializer.Deserialize<Models.Survey>(requestBody, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (survey == null)
            {
                _logger.LogWarning("JSONの解析に失敗しました");
                return new BadRequestObjectResult(
                    ApiResponse<object>.ErrorResponse("無効なJSONフォーマットです", "INVALID_JSON"));
            }

            // バリデーション
            if (!survey.IsValid(out var errors))
            {
                _logger.LogWarning("バリデーションエラー: {Errors}", string.Join(", ", errors));
                return new BadRequestObjectResult(
                    ApiResponse<object>.ErrorResponse(string.Join(", ", errors), "VALIDATION_ERROR"));
            }

            // 新しいIDと日時を設定
            survey.Id = Guid.NewGuid().ToString();
            survey.EventDate = "2025-07-09"; // イベント日付
            survey.CreatedAt = DateTime.UtcNow;
            survey.UpdatedAt = DateTime.UtcNow;

            // Cosmos DBに保存
            var surveyId = await _cosmosDbService.CreateSurveyAsync(survey);

            _logger.LogInformation("アンケートが正常に登録されました: {SurveyId}", surveyId);

            var response = ApiResponse<object>.SuccessResponse(
                "アンケートの登録が完了しました", 
                null, 
                surveyId);

            return new OkObjectResult(response);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "JSONの解析中にエラーが発生しました");
            return new BadRequestObjectResult(
                ApiResponse<object>.ErrorResponse("無効なJSONフォーマットです", "JSON_PARSE_ERROR"));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "無効な操作が実行されました");
            return new UnprocessableEntityObjectResult(
                ApiResponse<object>.ErrorResponse(ex.Message, "INVALID_OPERATION"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "アンケート登録中に予期しないエラーが発生しました");
            return new ObjectResult(
                ApiResponse<object>.ErrorResponse("内部サーバーエラーが発生しました", "INTERNAL_ERROR"))
            {
                StatusCode = 500
            };
        }
    }

    [Function("GetSurveyResults")]
    public async Task<IActionResult> GetSurveyResults(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "surveys/results")] HttpRequest req)
    {
        _logger.LogInformation("集計結果取得APIが呼ばれました");

        try
        {
            var results = await _cosmosDbService.GetSurveyResultsAsync();

            _logger.LogInformation("集計結果を正常に取得しました");

            var response = ApiResponse<SurveyData>.SuccessResponse(
                "集計結果を取得しました", 
                results);

            return new OkObjectResult(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "集計結果取得中に予期しないエラーが発生しました");
            return new ObjectResult(
                ApiResponse<object>.ErrorResponse("内部サーバーエラーが発生しました", "INTERNAL_ERROR"))
            {
                StatusCode = 500
            };
        }
    }
}