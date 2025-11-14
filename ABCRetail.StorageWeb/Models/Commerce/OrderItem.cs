using System.ComponentModel.DataAnnotations;

namespace ABCRetail.StorageWeb.Models.Commerce;

public sealed class OrderItem
{
    [Key]
    public int Id { get; set; }

    public int OrderId { get; set; }
    public Order? Order { get; set; }

    [Required]
    [MaxLength(256)]
    public string ProductName { get; set; } = string.Empty;

    [Range(0.01, 100000)]
    public decimal UnitPrice { get; set; }

    [Range(1, 1000)]
    public int Quantity { get; set; }

    [MaxLength(128)]
    public string? ProductPartitionKey { get; set; }

    [MaxLength(128)]
    public string? ProductRowKey { get; set; }
}
