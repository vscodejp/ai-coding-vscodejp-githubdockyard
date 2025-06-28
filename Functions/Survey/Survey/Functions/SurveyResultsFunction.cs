using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Survey.Services;

namespace Survey.Functions;

/// <summary>
/// アンケート集計結果取得 API
/// </summary>
public class SurveyResultsFunction
{
    private readonly ILogger<SurveyResultsFunction> _logger;
    private readonly SurveyAggregationService _aggregationService;

    public SurveyResultsFunction(ILogger<SurveyResultsFunction> logger, SurveyAggregationService aggregationService)
    {
        _logger = logger;
        _aggregationService = aggregationService;
    }

    /// <summary>
    /// アンケート集計結果取得エンドポイント
    /// GET /api/surveys/results
    /// </summary>
    [Function("GetSurveyResults")]
    public async Task<IActionResult> GetSurveyResults(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "surveys/results")] HttpRequest req)
    {
        try
        {
            _logger.LogInformation("アンケート集計結果API が呼び出されました");

            // 集計処理実行
            var results = await _aggregationService.GetAggregatedResultsAsync();

            _logger.LogInformation("アンケート集計結果を正常に取得しました。総回答数: {TotalResponses}", 
                results.Data.TotalResponses);

            return new OkObjectResult(results);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "アンケート集計結果の取得中にエラーが発生しました");
            
            return new ObjectResult(new
            {
                success = false,
                error = "集計結果の取得中にエラーが発生しました",
                code = "AGGREGATION_ERROR"
            })
            {
                StatusCode = 500
            };
        }
    }
}
