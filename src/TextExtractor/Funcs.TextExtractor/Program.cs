using Funcs.TextExtractor.Configuration;
using Funcs.TextExtractor.ImagesStorage;
using Funcs.TextExtractor.OCR;
using Funcs.TextExtractor.Repositories;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureServices(services =>
    {
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();

        var config = new ConfigurationBuilder()
            .SetBasePath(Environment.CurrentDirectory)
            .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables()
            .Build();

        services.Configure<FunctionSettings>(config.GetSection("FunctionSettings"));

        // Register CosmosClient
        services.AddSingleton(serviceProvider =>
        {
            var cosmosClientOptions = new CosmosClientOptions
            {
                ConnectionMode = ConnectionMode.Direct,
                AllowBulkExecution = true,
                SerializerOptions = new CosmosSerializationOptions
                {
                    PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase
                }
            };

            var functionSettings = serviceProvider.GetRequiredService<IOptions<FunctionSettings>>().Value;
            string cosmosDbConnectionString = functionSettings.CosmosDbConnectionString;

            return new CosmosClient(cosmosDbConnectionString, cosmosClientOptions);
        });

        // Add HttpClient and IHttpClientFactory registration
        services.AddHttpClient();

        services.AddTransient<IImagesStorageService, ImagesStorageService>();

        services.AddTransient<IOCRService, AzureOCRService>();

        services.AddTransient<IImageProcessingTaskRepository, CosmosDbImageProcessingTaskRepository>();

        // Application Insights telemetry configuration
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();
    })
        .Build();

    host.Run();
