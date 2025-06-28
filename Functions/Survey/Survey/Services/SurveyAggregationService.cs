using Survey.Models;
using Survey.Services;
using Microsoft.Extensions.Logging;

namespace Survey.Services;

/// <summary>
/// アンケート集計処理を行うサービス
/// </summary>
public class SurveyAggregationService
{
    private readonly CosmosDbService _cosmosDbService;
    private readonly ILogger<SurveyAggregationService> _logger;

    public SurveyAggregationService(CosmosDbService cosmosDbService, ILogger<SurveyAggregationService> logger)
    {
        _cosmosDbService = cosmosDbService;
        _logger = logger;
    }

    /// <summary>
    /// アンケート結果を集計
    /// </summary>
    public async Task<SurveyResultResponse> GetAggregatedResultsAsync()
    {
        try
        {
            _logger.LogInformation("アンケート集計処理を開始します");

            // 全てのアンケートを取得
            var surveys = await _cosmosDbService.GetAllSurveysAsync();
            
            // フィードバックを個別に取得（効率化）
            var feedbacks = await _cosmosDbService.GetFeedbackAsync();

            var result = new SurveyResultResponse
            {
                Success = true,
                Data = new SurveyResultData
                {
                    TotalResponses = surveys.Count,
                    CommunityAffiliation = AggregateCommunityAffiliation(surveys),
                    JobRole = AggregateJobRole(surveys),
                    EventRating = AggregateEventRating(surveys),
                    Feedback = feedbacks.OrderByDescending(f => f.Timestamp).ToList()
                }
            };

            _logger.LogInformation("アンケート集計処理が完了しました。総回答数: {TotalResponses}", result.Data.TotalResponses);
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "アンケート集計処理でエラーが発生しました");
            throw;
        }
    }

    /// <summary>
    /// コミュニティ所属を集計
    /// </summary>
    private Dictionary<string, int> AggregateCommunityAffiliation(List<Models.Survey> surveys)
    {
        var aggregation = new Dictionary<string, int>
        {
            ["VS Code Meetup"] = 0,
            ["GitHub dockyard"] = 0,
            ["どちらでもない"] = 0
        };

        foreach (var survey in surveys)
        {
            if (survey.CommunityAffiliation.Any())
            {
                foreach (var affiliation in survey.CommunityAffiliation)
                {
                    if (aggregation.ContainsKey(affiliation))
                    {
                        aggregation[affiliation]++;
                    }
                }
            }
            else
            {
                // 空配列の場合は「どちらでもない」にカウント
                aggregation["どちらでもない"]++;
            }
        }

        return aggregation;
    }

    /// <summary>
    /// 職種を集計
    /// </summary>
    private Dictionary<string, int> AggregateJobRole(List<Models.Survey> surveys)
    {
        var aggregation = new Dictionary<string, int>
        {
            ["フロントエンドエンジニア"] = 0,
            ["バックエンドエンジニア"] = 0,
            ["フルスタックエンジニア"] = 0,
            ["DevOpsエンジニア"] = 0,
            ["データエンジニア"] = 0,
            ["モバイルエンジニア"] = 0,
            ["その他"] = 0
        };

        foreach (var survey in surveys)
        {
            foreach (var jobRole in survey.JobRole)
            {
                if (aggregation.ContainsKey(jobRole))
                {
                    aggregation[jobRole]++;
                }
            }
        }

        return aggregation;
    }

    /// <summary>
    /// イベント評価を集計
    /// </summary>
    private EventRatingData AggregateEventRating(List<Models.Survey> surveys)
    {
        if (!surveys.Any())
        {
            return new EventRatingData
            {
                Average = 0,
                Distribution = new Dictionary<string, int>
                {
                    ["1"] = 0, ["2"] = 0, ["3"] = 0, ["4"] = 0, ["5"] = 0
                }
            };
        }

        var distribution = new Dictionary<string, int>
        {
            ["1"] = 0, ["2"] = 0, ["3"] = 0, ["4"] = 0, ["5"] = 0
        };

        var totalRating = 0.0;
        foreach (var survey in surveys)
        {
            var rating = survey.EventRating.ToString();
            if (distribution.ContainsKey(rating))
            {
                distribution[rating]++;
            }
            totalRating += survey.EventRating;
        }

        var average = Math.Round(totalRating / surveys.Count, 1);

        return new EventRatingData
        {
            Average = average,
            Distribution = distribution
        };
    }
}
