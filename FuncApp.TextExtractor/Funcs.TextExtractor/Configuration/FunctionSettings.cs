namespace Funcs.TextExtractor.Configuration;

public class FunctionSettings
{
    public string BlobConnectionString { get; set; } = string.Empty;

    public string CosmosDbEndpoint { get; set; } = string.Empty;

    public string CosmosDbKey { get; set; } = string.Empty;

    public string CosmosDbDatabaseId { get; set; } = string.Empty;

    public string CosmosDbContainerId { get; set; } = string.Empty;

    public string AzAiServicesEndpoint { get; set; } = string.Empty;

    public string AzAiServicesApiKey { get; set; } = string.Empty;
}
