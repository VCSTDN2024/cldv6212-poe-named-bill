using Azure.Data.Tables;
using Azure.Storage.Blobs;
using Azure.Storage.Files.Shares;
using Azure.Storage.Queues;
using ABCRetail.StorageWeb.Options;
using Microsoft.Extensions.Options;

namespace ABCRetail.StorageWeb.Services;

public sealed class StorageClientFactory
{
    private readonly AzureStorageOptions _options;

    public StorageClientFactory(IOptions<AzureStorageOptions> options)
    {
        _options = options.Value;
    }

    public TableClient CreateCustomersTableClient()
    {
        var client = new TableClient(_options.AccountConnectionString, _options.CustomersTableName);
        return client;
    }

    public TableClient CreateProductsTableClient()
    {
        var client = new TableClient(_options.AccountConnectionString, _options.ProductsTableName);
        return client;
    }

    public BlobContainerClient CreateMediaContainerClient()
    {
        var client = new BlobContainerClient(_options.AccountConnectionString, _options.MediaContainerName);
        return client;
    }

    public QueueClient CreateOperationsQueueClient()
    {
        var client = new QueueClient(_options.AccountConnectionString, _options.OperationsQueueName);
        return client;
    }

    public ShareClient CreateContractsShareClient()
    {
        var client = new ShareClient(_options.AccountConnectionString, _options.ContractsFileShareName);
        return client;
    }
}
