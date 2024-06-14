using Funcs.TextExtractor.Configuration;
using Funcs.TextExtractor.Data.Entities;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;

namespace Funcs.TextExtractor.Data.Repositories;

public class CosmosDbImageProcessingTaskRepository : IImageProcessingTaskRepository
{
    private readonly Microsoft.Azure.Cosmos.Container _container;

    public CosmosDbImageProcessingTaskRepository(CosmosClient cosmosClient, IOptions<FunctionSettings> options)
    {
        var databaseId = options.Value.CosmosDbDatabaseId;
        var containerId = options.Value.CosmosDbContainerId;

        _container = cosmosClient.GetContainer(databaseId, containerId);
    }

    public async Task CreateAsync(ImageProcessingTask task)
    {
        var requestOptions = new ItemRequestOptions { EnableContentResponseOnWrite = false };
        var partitionKey = new PartitionKey(task.RequestId);
        //await _container.CreateItemAsync(task, new PartitionKey(task.RequestId));
        await _container.CreateItemAsync(task, partitionKey, requestOptions);
    }

    public async Task UpdateAsync(ImageProcessingTask task)
    {

        await _container.UpsertItemAsync(task, new PartitionKey(task.RequestId));
    }

    public async Task<ImageProcessingTask> GetByIdAsync(string id)
    {
        try
        {
            ItemResponse<ImageProcessingTask> response = await _container.ReadItemAsync<ImageProcessingTask>(id, new PartitionKey(id));
            return response.Resource;
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task<IEnumerable<ImageProcessingTask>> GetAllAsync()
    {
        var query = new QueryDefinition("SELECT * FROM c");
        var tasks = new List<ImageProcessingTask>();

        var iterator = _container.GetItemQueryIterator<ImageProcessingTask>(query);
        while (iterator.HasMoreResults)
        {
            var response = await iterator.ReadNextAsync();
            tasks.AddRange(response);
        }

        return tasks;
    }

}