using Microsoft.Extensions.Logging;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Survey.Services;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

// Application Insights の設定
builder.Services
    .AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights();

// Cosmos DB サービスの依存性注入
builder.Services.AddSingleton<CosmosDbService>(serviceProvider =>
{
    var logger = serviceProvider.GetRequiredService<ILogger<CosmosDbService>>();
    // 環境変数からCosmos DBの接続文字列を取得
    var cosmosConnectionString = Environment.GetEnvironmentVariable("COSMOS_DB_CONNECTION_STRING")
        ?? throw new InvalidOperationException("COSMOS_DB_CONNECTION_STRING environment variable is not set");
    return new CosmosDbService(logger, cosmosConnectionString);
});

// CORS設定（フロントエンドとの連携用）
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://localhost:3000") // Viteのデフォルトポート
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

builder.Build().Run();
