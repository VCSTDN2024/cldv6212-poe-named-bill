using System.ComponentModel.DataAnnotations;

namespace ABCRetail.StorageWeb.Models.Commerce;

public sealed class CartItem
{
    [Key]
    public int Id { get; set; }

    public int CartId { get; set; }
    public Cart? Cart { get; set; }

    [Required]
    [MaxLength(128)]
    public string ProductPartitionKey { get; set; } = string.Empty;

    [Required]
    [MaxLength(128)]
    public string ProductRowKey { get; set; } = string.Empty;

    [Required]
    [MaxLength(256)]
    public string ProductName { get; set; } = string.Empty;

    [Range(0.01, 100000)]
    public decimal UnitPrice { get; set; }

    [Range(1, 1000)]
    public int Quantity { get; set; }

    public DateTime AddedAtUtc { get; set; } = DateTime.UtcNow;
}
