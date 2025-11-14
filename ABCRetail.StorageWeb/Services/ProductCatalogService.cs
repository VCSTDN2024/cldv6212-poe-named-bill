using Azure;
using Azure.Data.Tables;
using ABCRetail.StorageWeb.Models;

namespace ABCRetail.StorageWeb.Services;

public sealed class ProductCatalogService
{
    private readonly TableClient _tableClient;

    public ProductCatalogService(StorageClientFactory storageClientFactory)
    {
        _tableClient = storageClientFactory.CreateProductsTableClient();
    }

    public async Task EnsureInitializedAsync(CancellationToken cancellationToken)
    {
        await _tableClient.CreateIfNotExistsAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ProductEntity>> GetProductsAsync(int take = 25, CancellationToken cancellationToken = default)
    {
        var results = new List<ProductEntity>();

        await foreach (var entity in _tableClient.QueryAsync<ProductEntity>(maxPerPage: take, cancellationToken: cancellationToken))
        {
            results.Add(entity);
            if (results.Count >= take)
            {
                break;
            }
        }

        return results;
    }

    public async Task UpsertProductAsync(ProductEntity entity, CancellationToken cancellationToken = default)
    {
        await _tableClient.UpsertEntityAsync(entity, TableUpdateMode.Merge, cancellationToken);
    }

    public async Task DeleteProductAsync(string partitionKey, string rowKey, CancellationToken cancellationToken = default)
    {
        await _tableClient.DeleteEntityAsync(partitionKey, rowKey, ETag.All, cancellationToken);
    }

    public async Task<ProductEntity?> GetProductAsync(string partitionKey, string rowKey, CancellationToken cancellationToken = default)
    {
        var response = await _tableClient.GetEntityIfExistsAsync<ProductEntity>(partitionKey, rowKey, cancellationToken: cancellationToken);
        return response.HasValue ? response.Value : null;
    }
}
