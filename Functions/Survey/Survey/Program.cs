using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Azure.Cosmos;
using Survey.Services;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

// Application Insights の設定
builder.Services
    .AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights();

// Cosmos DB クライアントの設定
builder.Services.AddSingleton<CosmosClient>(serviceProvider =>
{
    var connectionString = Environment.GetEnvironmentVariable("CosmosDbConnectionString");
    if (string.IsNullOrEmpty(connectionString))
    {
        throw new InvalidOperationException("Cosmos DB の接続文字列が設定されていません");
    }

    var cosmosClientOptions = new CosmosClientOptions
    {
        ConnectionMode = ConnectionMode.Direct,
        MaxRequestsPerTcpConnection = 20,
        MaxTcpConnectionsPerEndpoint = 32,
        ConsistencyLevel = ConsistencyLevel.Session,
        RequestTimeout = TimeSpan.FromSeconds(30)
    };

    return new CosmosClient(connectionString, cosmosClientOptions);
});

// サービスの登録
builder.Services.AddScoped<CosmosDbService>();
builder.Services.AddScoped<SurveyAggregationService>();

var app = builder.Build();

// Cosmos DB の初期化
using (var scope = app.Services.CreateScope())
{
    try
    {
        var cosmosDbService = scope.ServiceProvider.GetRequiredService<CosmosDbService>();
        await cosmosDbService.InitializeAsync();
        Console.WriteLine("✅ Cosmos DB の初期化が完了しました");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Cosmos DB の初期化に失敗しました: {ex.Message}");
        // 初期化失敗時は警告のみ出力（アプリケーションは継続）
    }
}

app.Run();
