using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Survey.Models;
using Survey.Services;
using System.Text.Json;

namespace Survey;

/// <summary>
/// アンケート登録API
/// Azure Functions v4 (.NET 9) でRESTful APIを提供
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
    /// <param name="req">HTTPリクエスト</param>
    /// <param name="cancellationToken">キャンセレーショントークン</param>
    /// <returns>登録結果</returns>
    [Function("CreateSurvey")]
    public async Task<IActionResult> CreateSurvey(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "surveys")] HttpRequest req,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Survey creation request received at {Timestamp}", DateTime.UtcNow);

        try
        {
            // リクエストボディの読み取り
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync(cancellationToken);
            
            if (string.IsNullOrWhiteSpace(requestBody))
            {
                _logger.LogWarning("Empty request body received");
                return new BadRequestObjectResult(new ErrorResponse
                {
                    Error = "リクエストボディが空です",
                    Code = "EMPTY_REQUEST_BODY"
                });
            }

            // JSONデシリアライゼーション
            SurveyRequest? surveyRequest;
            try
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };
                surveyRequest = JsonSerializer.Deserialize<SurveyRequest>(requestBody, options);
            }
            catch (JsonException ex)
            {
                _logger.LogWarning(ex, "Invalid JSON format in request");
                return new BadRequestObjectResult(new ErrorResponse
                {
                    Error = "無効なJSON形式です",
                    Code = "INVALID_JSON"
                });
            }

            if (surveyRequest == null)
            {
                _logger.LogWarning("Failed to deserialize survey request");
                return new BadRequestObjectResult(new ErrorResponse
                {
                    Error = "リクエストの解析に失敗しました",
                    Code = "PARSE_ERROR"
                });
            }

            // バリデーション実行
            var validationErrors = SurveyValidationService.ValidateRequest(surveyRequest);
            if (validationErrors.Any())
            {
                _logger.LogWarning("Validation failed: {Errors}", string.Join(", ", validationErrors));
                return new UnprocessableEntityObjectResult(new ErrorResponse
                {
                    Error = string.Join("; ", validationErrors),
                    Code = "VALIDATION_ERROR"
                });
            }

            // エンティティに変換
            var surveyEntity = SurveyValidationService.ConvertToEntity(surveyRequest);

            // Cosmos DBに保存
            var surveyId = await _cosmosDbService.CreateSurveyAsync(surveyEntity, cancellationToken);

            _logger.LogInformation("Survey created successfully with ID: {SurveyId}", surveyId);

            // 成功レスポンス
            return new ObjectResult(new SurveyResponse
            {
                Success = true,
                Message = "アンケートの登録が完了しました",
                SurveyId = surveyId
            })
            {
                StatusCode = StatusCodes.Status201Created
            };
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Business logic error occurred during survey creation");
            return new ObjectResult(new ErrorResponse
            {
                Error = ex.Message,
                Code = "BUSINESS_ERROR"
            })
            {
                StatusCode = StatusCodes.Status422UnprocessableEntity
            };
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Survey creation was cancelled");
            return new ObjectResult(new ErrorResponse
            {
                Error = "リクエストがキャンセルされました",
                Code = "REQUEST_CANCELLED"
            })
            {
                StatusCode = StatusCodes.Status408RequestTimeout
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred during survey creation");
            return new ObjectResult(new ErrorResponse
            {
                Error = "サーバー内部エラーが発生しました",
                Code = "INTERNAL_SERVER_ERROR"
            })
            {
                StatusCode = StatusCodes.Status500InternalServerError
            };
        }
    }

    /// <summary>
    /// ヘルスチェックエンドポイント
    /// GET /api/health
    /// </summary>
    [Function("HealthCheck")]
    public IActionResult HealthCheck([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "health")] HttpRequest req)
    {
        _logger.LogInformation("Health check requested");
        return new OkObjectResult(new { status = "healthy", timestamp = DateTime.UtcNow });
    }
}