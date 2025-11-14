using System.ComponentModel.DataAnnotations;

namespace ABCRetail.StorageWeb.Models;

public sealed class ProductInputModel
{
    [Display(Name = "Category / Partition Key")]
    [StringLength(64, ErrorMessage = "Please keep the category under 64 characters.")]
    public string Category { get; set; } = string.Empty;

    [Display(Name = "SKU (Row Key)")]
    [StringLength(128, ErrorMessage = "SKUs must be under 128 characters.")]
    public string? ProductSku { get; set; }

    [Required(ErrorMessage = "Please capture the product name.")]
    [StringLength(150, ErrorMessage = "Product names must stay under 150 characters.")]
    public string? Name { get; set; }

    [StringLength(500, ErrorMessage = "Descriptions should be under 500 characters.")]
    public string? Description { get; set; }

    [Range(0.01, 1000000, ErrorMessage = "Please capture a price greater than zero.")]
    public decimal Price { get; set; }

    [Display(Name = "Stock Quantity")]
    [Range(0, 1000000, ErrorMessage = "Stock can't be negative.")]
    public int StockQuantity { get; set; }

    public bool IsSeasonal { get; set; }
}
