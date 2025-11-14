using Azure;
using Azure.Data.Tables;
using ABCRetail.StorageWeb.Models;

namespace ABCRetail.StorageWeb.Services;

public sealed class CustomerProfileService
{
    private readonly TableClient _tableClient;

    public CustomerProfileService(StorageClientFactory storageClientFactory)
    {
        _tableClient = storageClientFactory.CreateCustomersTableClient();
    }

    public async Task EnsureInitializedAsync(CancellationToken cancellationToken)
    {
        await _tableClient.CreateIfNotExistsAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<CustomerProfileEntity>> GetCustomersAsync(int take = 25, CancellationToken cancellationToken = default)
    {
        var results = new List<CustomerProfileEntity>();

        await foreach (var entity in _tableClient.QueryAsync<CustomerProfileEntity>(maxPerPage: take, cancellationToken: cancellationToken))
        {
            results.Add(entity);
            if (results.Count >= take)
            {
                break;
            }
        }

        return results;
    }

    public async Task UpsertCustomerAsync(CustomerProfileEntity entity, CancellationToken cancellationToken = default)
    {
        await _tableClient.UpsertEntityAsync(entity, TableUpdateMode.Merge, cancellationToken);
    }

    public async Task DeleteCustomerAsync(string partitionKey, string rowKey, CancellationToken cancellationToken = default)
    {
        await _tableClient.DeleteEntityAsync(partitionKey, rowKey, ETag.All, cancellationToken);
    }
}
