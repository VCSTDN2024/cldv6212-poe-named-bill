using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using ABCRetail.StorageWeb.Models;
using ABCRetail.StorageWeb.Models.Commerce;
using ABCRetail.StorageWeb.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ABCRetail.StorageWeb.Pages.Cart;

[Authorize]
public sealed class IndexModel : PageModel
{
    private readonly CartService _cartService;
    private readonly ProductCatalogService _productCatalogService;
    private readonly OrderService _orderService;

    public IndexModel(CartService cartService, ProductCatalogService productCatalogService, OrderService orderService)
    {
        _cartService = cartService;
        _productCatalogService = productCatalogService;
        _orderService = orderService;
    }

    public IReadOnlyList<CartItem> Items { get; private set; } = Array.Empty<CartItem>();
    public IReadOnlyList<ProductEntity> Products { get; private set; } = Array.Empty<ProductEntity>();
    public decimal CartTotal => Items.Sum(i => i.UnitPrice * i.Quantity);

    [BindProperty]
    [Range(1, 25, ErrorMessage = "Quantity must be between 1 and 25.")]
    public int Quantity { get; set; } = 1;

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        await LoadPageModelAsync(cancellationToken);
    }

    public async Task<IActionResult> OnPostAddAsync(string partitionKey, string rowKey, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(partitionKey) || string.IsNullOrWhiteSpace(rowKey))
        {
            ModelState.AddModelError(string.Empty, "Select a product to add.");
            await LoadPageModelAsync(cancellationToken);
            return Page();
        }

        if (!ModelState.IsValid)
        {
            await LoadPageModelAsync(cancellationToken);
            return Page();
        }

        var product = await _productCatalogService.GetProductAsync(partitionKey, rowKey, cancellationToken);
        if (product is null)
        {
            ModelState.AddModelError(string.Empty, "The selected product was not found.");
            await LoadPageModelAsync(cancellationToken);
            return Page();
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        await _cartService.AddItemAsync(userId, product, Quantity, cancellationToken);
        TempData["StatusMessage"] = $"Added {Quantity} × {product.Name ?? product.RowKey} to your cart.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostRemoveAsync(int itemId, CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        await _cartService.RemoveItemAsync(userId, itemId, cancellationToken);
        TempData["StatusMessage"] = "Item removed from your cart.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostCheckoutAsync(CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var order = await _orderService.PlaceOrderAsync(userId, cancellationToken);
        if (order is null)
        {
            TempData["StatusMessage"] = "Add at least one item to your cart before checking out.";
            return RedirectToPage();
        }

        TempData["StatusMessage"] = $"Order #{order.Id} placed successfully.";
        return RedirectToPage("/Orders/Index");
    }

    private async Task LoadPageModelAsync(CancellationToken cancellationToken)
    {
        Products = await _productCatalogService.GetProductsAsync(50, cancellationToken);
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var cart = await _cartService.GetOrCreateCartAsync(userId, cancellationToken);
        Items = cart.Items.ToList();
    }
}
