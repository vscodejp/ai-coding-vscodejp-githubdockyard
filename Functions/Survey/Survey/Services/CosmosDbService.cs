using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Survey.Models;
using Azure.Identity;

namespace Survey.Services;

/// <summary>
/// Cosmos DBとのデータ操作を行うサービス
/// Azure のベストプラクティスに従い、Managed Identity を使用した認証を実装
/// </summary>
public class CosmosDbService
{
    private readonly CosmosClient _cosmosClient;
    private readonly Database _database;
    private readonly Container _container;
    private readonly ILogger<CosmosDbService> _logger;

    private const string DatabaseId = "vsgh";
    private const string ContainerId = "survey";
    private const string PartitionKey = "/eventDate";

    public CosmosDbService(ILogger<CosmosDbService> logger, string cosmosConnectionString)
    {
        _logger = logger;
        try
        {
            // ConnectionStringでローカル・本番どちらも接続できるように
            var cosmosClientOptions = new CosmosClientOptions
            {
                ConnectionMode = ConnectionMode.Direct,
                MaxRetryAttemptsOnRateLimitedRequests = 3,
                MaxRetryWaitTimeOnRateLimitedRequests = TimeSpan.FromSeconds(30),
                RequestTimeout = TimeSpan.FromSeconds(30)
            };
            _cosmosClient = new CosmosClient(cosmosConnectionString, cosmosClientOptions);
            _database = _cosmosClient.GetDatabase(DatabaseId);
            _container = _database.GetContainer(ContainerId);
            _logger.LogInformation("Cosmos DB service initialized successfully (ConnectionString)");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize Cosmos DB service");
            throw;
        }
    }

    /// <summary>
    /// アンケートデータをCosmos DBに保存
    /// エラーハンドリングとリトライロジックを含む
    /// </summary>
    /// <param name="surveyEntity">保存するアンケートエンティティ</param>
    /// <param name="cancellationToken">キャンセレーショントークン</param>
    /// <returns>保存されたエンティティのID</returns>
    public async Task<string> CreateSurveyAsync(SurveyEntity surveyEntity, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Creating survey with ID: {SurveyId}", surveyEntity.id);

            var response = await _container.CreateItemAsync(
                surveyEntity,
                new PartitionKey(surveyEntity.EventDate),
                cancellationToken: cancellationToken);

            _logger.LogInformation("Survey created successfully. ID: {SurveyId}, RU consumed: {RU}", 
                surveyEntity.id, response.RequestCharge);

            return surveyEntity.id;
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Conflict)
        {
            _logger.LogWarning("Survey with ID {SurveyId} already exists", surveyEntity.id);
            throw new InvalidOperationException($"アンケートID {surveyEntity.id} は既に存在します", ex);
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
        {
            _logger.LogWarning("Rate limit exceeded for survey creation. ID: {SurveyId}", surveyEntity.id);
            throw new InvalidOperationException("一時的にリクエストが集中しています。しばらくお待ちください", ex);
        }
        catch (CosmosException ex)
        {
            _logger.LogError(ex, "Cosmos DB error occurred while creating survey. ID: {SurveyId}, StatusCode: {StatusCode}", 
                surveyEntity.id, ex.StatusCode);
            throw new InvalidOperationException("データベースエラーが発生しました", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while creating survey. ID: {SurveyId}", surveyEntity.id);
            throw new InvalidOperationException("予期しないエラーが発生しました", ex);
        }
    }

    /// <summary>
    /// アンケートデータを取得（将来の集計機能用）
    /// </summary>
    /// <param name="surveyId">取得するアンケートのID</param>
    /// <param name="cancellationToken">キャンセレーショントークン</param>
    /// <returns>アンケートエンティティ</returns>
    public async Task<SurveyEntity?> GetSurveyAsync(string surveyId, string eventDate, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Retrieving survey with ID: {SurveyId}", surveyId);

            var response = await _container.ReadItemAsync<SurveyEntity>(
                surveyId,
                new PartitionKey(eventDate),
                cancellationToken: cancellationToken);

            _logger.LogInformation("Survey retrieved successfully. ID: {SurveyId}, RU consumed: {RU}", 
                surveyId, response.RequestCharge);

            return response.Resource;
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            _logger.LogWarning("Survey with ID {SurveyId} not found", surveyId);
            return null;
        }
        catch (CosmosException ex)
        {
            _logger.LogError(ex, "Cosmos DB error occurred while retrieving survey. ID: {SurveyId}, StatusCode: {StatusCode}", 
                surveyId, ex.StatusCode);
            throw new InvalidOperationException("データベースエラーが発生しました", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while retrieving survey. ID: {SurveyId}", surveyId);
            throw new InvalidOperationException("予期しないエラーが発生しました", ex);
        }
    }

    /// <summary>
    /// リソースのクリーンアップ
    /// </summary>
    public void Dispose()
    {
        _cosmosClient?.Dispose();
    }
}
