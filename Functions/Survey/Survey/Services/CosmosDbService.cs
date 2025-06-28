using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Survey.Models;
using System.Net;

namespace Survey.Services;

/// <summary>
/// Cosmos DB との通信を管理するサービス
/// </summary>
public class CosmosDbService
{
    private readonly CosmosClient _cosmosClient;
    private readonly Container _container;
    private readonly ILogger<CosmosDbService> _logger;
    
    private readonly string _databaseName;
    private readonly string _containerName;

    public CosmosDbService(CosmosClient cosmosClient, ILogger<CosmosDbService> logger)
    {
        _cosmosClient = cosmosClient;
        _logger = logger;
        
        // 環境変数から設定を取得
        _databaseName = Environment.GetEnvironmentVariable("CosmosDbDatabaseName") ?? "vsgh";
        _containerName = Environment.GetEnvironmentVariable("CosmosDbContainerName") ?? "survey";
        
        // コンテナの参照を取得
        _container = _cosmosClient.GetContainer(_databaseName, _containerName);
    }

    /// <summary>
    /// アンケートを登録
    /// </summary>
    public async Task<string> CreateSurveyAsync(Models.Survey survey)
    {
        try
        {
            _logger.LogInformation("アンケートの登録を開始します。ID: {SurveyId}", survey.Id);

            // デバッグ用: Id プロパティの詳細確認
            _logger.LogInformation("デバッグ - Survey.Id の値: '{SurveyId}', 長さ: {Length}, null判定: {IsNull}", 
                survey.Id, survey.Id?.Length ?? 0, survey.Id == null);
            
            // デバッグ用: オブジェクト全体をJSON形式でログ出力
            try
            {
                var jsonString = System.Text.Json.JsonSerializer.Serialize(survey);
                _logger.LogInformation("デバッグ - シリアライズされたJSON: {JsonString}", jsonString);
            }
            catch (Exception jsonEx)
            {
                _logger.LogError(jsonEx, "JSONシリアライゼーションエラー");
            }

            // パーティションキーを設定（eventDate）
            // EventDate が未設定の場合のみ設定する
            if (string.IsNullOrEmpty(survey.EventDate))
            {
                survey.EventDate = DateTime.UtcNow.ToString("yyyy-MM");
            }
            survey.PartitionKey = survey.EventDate; // 互換性のため
            
            var response = await _container.CreateItemAsync(
                survey, 
                new PartitionKey(survey.EventDate)
            );

            _logger.LogInformation("アンケートの登録が完了しました。ID: {SurveyId}, RU: {RequestCharge}", 
                survey.Id, response.RequestCharge);

            return survey.Id;
        }
        catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.Conflict)
        {
            _logger.LogWarning("アンケートID {SurveyId} は既に存在します", survey.Id);
            throw new InvalidOperationException("アンケートは既に登録済みです", ex);
        }
        catch (CosmosException ex)
        {
            _logger.LogError(ex, "Cosmos DB でエラーが発生しました。StatusCode: {StatusCode}", ex.StatusCode);
            throw new Exception("データベースエラーが発生しました", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "予期しないエラーが発生しました");
            throw;
        }
    }

    /// <summary>
    /// 全てのアンケートを取得（集計処理用）
    /// </summary>
    public async Task<List<Models.Survey>> GetAllSurveysAsync()
    {
        try
        {
            _logger.LogInformation("全アンケートの取得を開始します");

            var query = "SELECT * FROM c";
            var queryDefinition = new QueryDefinition(query);
            
            var surveys = new List<Models.Survey>();
            double totalRequestCharge = 0;

            using var feedIterator = _container.GetItemQueryIterator<Models.Survey>(queryDefinition);
            
            while (feedIterator.HasMoreResults)
            {
                var response = await feedIterator.ReadNextAsync();
                totalRequestCharge += response.RequestCharge;
                
                foreach (var survey in response)
                {
                    surveys.Add(survey);
                }
            }

            _logger.LogInformation("全アンケートの取得が完了しました。件数: {Count}, 総RU: {TotalRequestCharge}", 
                surveys.Count, totalRequestCharge);

            return surveys;
        }
        catch (CosmosException ex)
        {
            _logger.LogError(ex, "Cosmos DB でエラーが発生しました。StatusCode: {StatusCode}", ex.StatusCode);
            throw new Exception("データベースエラーが発生しました", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "予期しないエラーが発生しました");
            throw;
        }
    }

    /// <summary>
    /// フィードバックがあるアンケートのみを取得
    /// </summary>
    public async Task<List<FeedbackData>> GetFeedbackAsync()
    {
        try
        {
            _logger.LogInformation("フィードバックの取得を開始します");

            var query = "SELECT c.id, c.feedback, c.createdAt FROM c WHERE c.feedback != ''";
            var queryDefinition = new QueryDefinition(query);
            
            var feedbacks = new List<FeedbackData>();
            double totalRequestCharge = 0;

            using var feedIterator = _container.GetItemQueryIterator<dynamic>(queryDefinition);
            
            while (feedIterator.HasMoreResults)
            {
                var response = await feedIterator.ReadNextAsync();
                totalRequestCharge += response.RequestCharge;
                
                foreach (var item in response)
                {
                    feedbacks.Add(new FeedbackData
                    {
                        Id = item.id,
                        Feedback = item.feedback,
                        Timestamp = item.createdAt
                    });
                }
            }

            _logger.LogInformation("フィードバックの取得が完了しました。件数: {Count}, 総RU: {TotalRequestCharge}", 
                feedbacks.Count, totalRequestCharge);

            return feedbacks;
        }
        catch (CosmosException ex)
        {
            _logger.LogError(ex, "Cosmos DB でエラーが発生しました。StatusCode: {StatusCode}", ex.StatusCode);
            throw new Exception("データベースエラーが発生しました", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "予期しないエラーが発生しました");
            throw;
        }
    }

    /// <summary>
    /// データベースとコンテナの初期化
    /// </summary>
    public async Task InitializeAsync()
    {
        try
        {
            _logger.LogInformation("Cosmos DB の初期化を開始します。Database: {DatabaseName}, Container: {ContainerName}", 
                _databaseName, _containerName);

            // データベースの作成
            var databaseResponse = await _cosmosClient.CreateDatabaseIfNotExistsAsync(
                _databaseName, 
                throughput: 400 // 最小スループット
            );

            // コンテナの作成
            var containerProperties = new ContainerProperties
            {
                Id = _containerName,
                PartitionKeyPath = "/eventDate" // パーティションキーパスを eventDate に変更
            };

            var containerResponse = await databaseResponse.Database.CreateContainerIfNotExistsAsync(
                containerProperties,
                throughput: 400
            );

            _logger.LogInformation("Cosmos DB の初期化が完了しました。Database: {DatabaseName}, Container: {ContainerName}", 
                _databaseName, _containerName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Cosmos DB の初期化でエラーが発生しました");
            throw;
        }
    }
}
