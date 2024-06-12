using FuncApp_TextExtractor.Configuration;
using FuncApp_TextExtractor.ImagesBlobStorage;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(FuncApp_TextExtractor.Startup))]
namespace FuncApp_TextExtractor;

public class Startup : FunctionsStartup
{
    public override void Configure(IFunctionsHostBuilder builder)
    {
        var config = new ConfigurationBuilder()
            .SetBasePath(Environment.CurrentDirectory)
            .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables()
            .Build();

        builder.Services.AddSingleton<IImagesBlobStorageService, ImagesBlobStorageService>();

        builder.Services.Configure<FunctionSettings>(config.GetSection("FunctionSettings"));
    }
}
