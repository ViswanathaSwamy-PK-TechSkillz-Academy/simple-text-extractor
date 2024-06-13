using Funcs.TextExtractor.Configuration;
using Funcs.TextExtractor.Data.Repositories;
using Funcs.TextExtractor.ImagesStorage;
using Funcs.TextExtractor.OCR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
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

        // Add HttpClient and IHttpClientFactory registration
        services.AddHttpClient();

        services.AddTransient<IImagesStorageService, ImagesStorageService>();

        services.AddTransient<IOCRService, AzureOCRService>();

        services.AddTransient<IImageProcessingTaskRepository, CosmosDbImageProcessingTaskRepository>();
    })
    .Build();

host.Run();