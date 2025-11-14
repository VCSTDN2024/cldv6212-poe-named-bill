using Azure;
using Azure.Data.Tables;

namespace ABCRetail.StorageWeb.Models;

public sealed class ProductEntity : ITableEntity
{
    public ProductEntity()
    {
    }

    public ProductEntity(string partitionKey, string rowKey)
    {
        PartitionKey = partitionKey;
        RowKey = rowKey;
    }

    public string PartitionKey { get; set; } = "General";

    public string RowKey { get; set; } = Guid.NewGuid().ToString();

    public string? Name { get; set; }

    public string? Description { get; set; }

    public decimal Price { get; set; }

    public int StockQuantity { get; set; }

    public bool IsSeasonal { get; set; }

    public DateTimeOffset? Timestamp { get; set; }

    public ETag ETag { get; set; } = ETag.All;
}
