using Azure;
using Azure.Data.Tables;

namespace ABCRetail.StorageWeb.Models;

public sealed class CustomerProfileEntity : ITableEntity
{
    public CustomerProfileEntity()
    {
    }

    public CustomerProfileEntity(string partitionKey, string rowKey)
    {
        PartitionKey = partitionKey;
        RowKey = rowKey;
    }

    public string PartitionKey { get; set; } = "Retail";

    public string RowKey { get; set; } = Guid.NewGuid().ToString();

    public string? FirstName { get; set; } = null;

    public string? LastName { get; set; } = null;

    public string? Email { get; set; } = null;

    public string? LoyaltyTier { get; set; } = null;

    public string? PhoneNumber { get; set; } = null;

    public DateTimeOffset? Timestamp { get; set; }

    public ETag ETag { get; set; } = ETag.All;
}
