namespace FuncApp_TextExtractor.Configuration;

public class FunctionSettings
{
    public string BlobConnectionString { get; set; } = string.Empty;

    public string CosmosDbEndpoint { get; set; } = string.Empty;

    public string CosmosDbKey { get; set; } = string.Empty;

    public string CosmosDbDatabaseId { get; set; } = string.Empty;

    public string CosmosDbContainerId { get; set; } = string.Empty;
}
