using ABCRetail.StorageWeb.Models;
using ABCRetail.StorageWeb.Options;
using ABCRetail.StorageWeb.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;

namespace ABCRetail.StorageWeb.Pages.Products;

public sealed class IndexModel : PageModel
{
    private readonly ProductCatalogService _productService;
    private readonly AzureStorageOptions _options;

    public IndexModel(ProductCatalogService productService, IOptions<AzureStorageOptions> options)
    {
        _productService = productService;
        _options = options.Value;
    }

    public IReadOnlyList<ProductEntity> Products { get; private set; } = Array.Empty<ProductEntity>();

    [BindProperty]
    public ProductInputModel Input { get; set; } = new();

    [TempData]
    public string? StatusMessage { get; set; }

    public string ModuleCode => _options.PlaceholderModuleCode;

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        Products = await _productService.GetProductsAsync(50, cancellationToken);
    }

    public async Task<IActionResult> OnPostUpsertAsync(CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            await OnGetAsync(cancellationToken);
            return Page();
        }

        var partitionKey = string.IsNullOrWhiteSpace(Input.Category) ? "General" : Input.Category.Trim();
        var rowKey = string.IsNullOrWhiteSpace(Input.ProductSku) ? Guid.NewGuid().ToString() : Input.ProductSku.Trim();

        var entity = new ProductEntity(partitionKey, rowKey)
        {
            Name = Input.Name,
            Description = Input.Description,
            Price = Input.Price,
            StockQuantity = Input.StockQuantity,
            IsSeasonal = Input.IsSeasonal
        };

        await _productService.UpsertProductAsync(entity, cancellationToken);
        StatusMessage = $"Saved product {entity.Name}.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync(string partitionKey, string rowKey, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(partitionKey) && !string.IsNullOrWhiteSpace(rowKey))
        {
            await _productService.DeleteProductAsync(partitionKey, rowKey, cancellationToken);
            StatusMessage = $"Deleted product {rowKey}.";
        }

        return RedirectToPage();
    }
}
