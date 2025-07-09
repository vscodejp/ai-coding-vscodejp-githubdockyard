using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Azure.Cosmos;
using Survey.Services;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

builder.Services
    .AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights();

// Cosmos DB クライアントの設定
builder.Services.AddSingleton<CosmosClient>(serviceProvider =>
{
    var connectionString = Environment.GetEnvironmentVariable("CosmosDbConnectionString");
    if (string.IsNullOrEmpty(connectionString))
    {
        throw new InvalidOperationException("CosmosDbConnectionString environment variable is not set");
    }

    var cosmosClientOptions = new CosmosClientOptions
    {
        RequestTimeout = TimeSpan.FromSeconds(30),
        OpenTcpConnectionTimeout = TimeSpan.FromSeconds(30),
        GatewayModeMaxConnectionLimit = 50,
        MaxRetryAttemptsOnRateLimitedRequests = 3,
        MaxRetryWaitTimeOnRateLimitedRequests = TimeSpan.FromSeconds(30)
    };

    return new CosmosClient(connectionString, cosmosClientOptions);
});

// Cosmos DB サービスの登録
builder.Services.AddScoped<ICosmosDbService, CosmosDbService>();

builder.Build().Run();
