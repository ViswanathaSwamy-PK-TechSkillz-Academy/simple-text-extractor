namespace FuncApp_TextExtractor.Configuration;

public class FunctionSettings
{
    public string? BlobConnectionString { get; set; }

    public string? CosmosDbEndpoint { get; set; }

    public string? CosmosDbKey { get; set; }

    public string? CosmosDbDatabaseId { get; set; }

    public string? CosmosDbContainerId { get; set; }
}
