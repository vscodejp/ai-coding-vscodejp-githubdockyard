using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Survey.Models;
using System.Net;

namespace Survey.Services;

public interface ICosmosDbService
{
    Task<string> CreateSurveyAsync(Models.Survey survey);
    Task<SurveyData> GetSurveyResultsAsync();
}

public class CosmosDbService : ICosmosDbService
{
    private readonly CosmosClient _cosmosClient;
    private readonly Container _container;
    private readonly ILogger<CosmosDbService> _logger;

    private const string DatabaseName = "vsgh";
    private const string ContainerName = "survey";

    public CosmosDbService(CosmosClient cosmosClient, ILogger<CosmosDbService> logger)
    {
        _cosmosClient = cosmosClient;
        _logger = logger;
        _container = _cosmosClient.GetContainer(DatabaseName, ContainerName);
    }

    public async Task<string> CreateSurveyAsync(Models.Survey survey)
    {
        try
        {
            _logger.LogInformation("アンケートを登録します: {SurveyId}", survey.Id);

            var response = await _container.CreateItemAsync(
                survey,
                new PartitionKey(survey.EventDate)
            );

            _logger.LogInformation("アンケートが正常に登録されました: {SurveyId}, RU消費: {RequestCharge}", 
                survey.Id, response.RequestCharge);

            return survey.Id;
        }
        catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.Conflict)
        {
            _logger.LogWarning("アンケートID {SurveyId} は既に存在します", survey.Id);
            throw new InvalidOperationException("同じIDのアンケートが既に存在します", ex);
        }
        catch (CosmosException ex)
        {
            _logger.LogError(ex, "Cosmos DBエラーが発生しました: {StatusCode} - {Message}", 
                ex.StatusCode, ex.Message);
            throw new Exception($"データベースエラーが発生しました: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "アンケート登録中に予期しないエラーが発生しました");
            throw;
        }
    }

    public async Task<SurveyData> GetSurveyResultsAsync()
    {
        try
        {
            _logger.LogInformation("アンケート集計結果を取得します");

            // 全てのアンケートを取得
            var query = new QueryDefinition("SELECT * FROM c");
            var iterator = _container.GetItemQueryIterator<Models.Survey>(query);

            var surveys = new List<Models.Survey>();
            double totalRU = 0;

            while (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync();
                surveys.AddRange(response);
                totalRU += response.RequestCharge;
            }

            _logger.LogInformation("アンケートデータを {Count} 件取得しました。RU消費: {RequestCharge}", 
                surveys.Count, totalRU);

            // 集計処理
            var result = new SurveyData
            {
                TotalResponses = surveys.Count
            };

            // コミュニティ所属の集計
            foreach (var survey in surveys)
            {
                if (survey.CommunityAffiliation.Length == 0)
                {
                    result.CommunityAffiliation["どちらでもない"] = 
                        result.CommunityAffiliation.GetValueOrDefault("どちらでもない", 0) + 1;
                }
                else
                {
                    foreach (var community in survey.CommunityAffiliation)
                    {
                        result.CommunityAffiliation[community] = 
                            result.CommunityAffiliation.GetValueOrDefault(community, 0) + 1;
                    }
                }
            }

            // 職種の集計
            foreach (var survey in surveys)
            {
                foreach (var job in survey.JobRole)
                {
                    result.JobRole[job] = result.JobRole.GetValueOrDefault(job, 0) + 1;
                }
            }

            // イベント評価の集計
            if (surveys.Count > 0)
            {
                result.EventRating.Average = surveys.Average(s => s.EventRating);
                
                for (int i = 1; i <= 5; i++)
                {
                    var count = surveys.Count(s => s.EventRating == i);
                    result.EventRating.Distribution[i.ToString()] = count;
                }
            }

            // フィードバックの取得（空でないもののみ）
            result.Feedback = surveys
                .Where(s => !string.IsNullOrWhiteSpace(s.Feedback))
                .Select(s => new FeedbackItem
                {
                    Id = s.Id,
                    Feedback = s.Feedback!,
                    Timestamp = s.CreatedAt
                })
                .OrderByDescending(f => f.Timestamp)
                .ToList();

            _logger.LogInformation("集計結果の生成が完了しました");
            return result;
        }
        catch (CosmosException ex)
        {
            _logger.LogError(ex, "Cosmos DBエラーが発生しました: {StatusCode} - {Message}", 
                ex.StatusCode, ex.Message);
            throw new Exception($"データベースエラーが発生しました: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "集計結果取得中に予期しないエラーが発生しました");
            throw;
        }
    }
}
