using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Survey.Models;
using Survey.Services;
using System.Text.Json;

namespace Survey.Functions;

/// <summary>
/// アンケート登録 API
/// </summary>
public class SurveyFunction
{
    private readonly ILogger<SurveyFunction> _logger;
    private readonly CosmosDbService _cosmosDbService;

    public SurveyFunction(ILogger<SurveyFunction> logger, CosmosDbService cosmosDbService)
    {
        _logger = logger;
        _cosmosDbService = cosmosDbService;
    }

    /// <summary>
    /// アンケート登録エンドポイント
    /// POST /api/surveys
    /// </summary>
    [Function("CreateSurvey")]
    public async Task<IActionResult> CreateSurvey(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "surveys")] HttpRequest req)
    {
        try
        {
            _logger.LogInformation("アンケート登録API が呼び出されました");

            // リクエストボディの読み取り
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            
            if (string.IsNullOrWhiteSpace(requestBody))
            {
                _logger.LogWarning("リクエストボディが空です");
                return new BadRequestObjectResult(ErrorResponse.CreateBadRequestError("リクエストボディが必要です"));
            }

            // JSON デシリアライズ
            SurveyRequest? surveyRequest;
            try
            {
                var jsonOptions = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                surveyRequest = JsonSerializer.Deserialize<SurveyRequest>(requestBody, jsonOptions);
            }
            catch (JsonException ex)
            {
                _logger.LogWarning(ex, "JSON の形式が正しくありません");
                return new BadRequestObjectResult(ErrorResponse.CreateBadRequestError("JSON の形式が正しくありません"));
            }

            if (surveyRequest == null)
            {
                _logger.LogWarning("デシリアライズに失敗しました");
                return new BadRequestObjectResult(ErrorResponse.CreateBadRequestError("リクエストの形式が正しくありません"));
            }

            // バリデーション
            if (!surveyRequest.IsValid(out var validationErrors))
            {
                _logger.LogWarning("バリデーションエラー: {Errors}", string.Join(", ", validationErrors));
                return new UnprocessableEntityObjectResult(ErrorResponse.CreateValidationError(validationErrors));
            }

            // Survey エンティティに変換
            var survey = surveyRequest.ToEntity();
            
            // デバッグ用ログ追加
            _logger.LogInformation("Survey オブジェクト作成完了: ID={SurveyId}, CommunityAffiliation={CommunityAffiliation}", 
                survey.Id, string.Join(", ", survey.CommunityAffiliation));

            // Cosmos DB に保存
            var surveyId = await _cosmosDbService.CreateSurveyAsync(survey);

            _logger.LogInformation("アンケートが正常に登録されました。ID: {SurveyId}", surveyId);

            // レスポンス作成
            var response = SurveyResponse.CreateSuccess(surveyId);
            return new CreatedResult($"/api/surveys/{surveyId}", response);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "処理中にエラーが発生しました");
            return new ConflictObjectResult(ErrorResponse.CreateBadRequestError(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "予期しないエラーが発生しました");
            return new ObjectResult(ErrorResponse.CreateInternalError())
            {
                StatusCode = 500
            };
        }
    }

    /// <summary>
    /// ヘルスチェックエンドポイント
    /// GET /api/surveys/health
    /// </summary>
    [Function("HealthCheck")]
    public IActionResult HealthCheck(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "surveys/health")] HttpRequest req)
    {
        _logger.LogInformation("ヘルスチェックAPI が呼び出されました");

        return new OkObjectResult(new
        {
            status = "healthy",
            timestamp = DateTime.UtcNow,
            version = "1.0.0"
        });
    }
}
