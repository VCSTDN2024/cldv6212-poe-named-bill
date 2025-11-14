using ABCRetail.StorageWeb.Models.Commerce;
using ABCRetail.StorageWeb.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ABCRetail.StorageWeb.Pages.Orders;

[Authorize(Roles = "Administrator")]
public sealed class AdminModel : PageModel
{
    private readonly OrderService _orderService;

    public AdminModel(OrderService orderService)
    {
        _orderService = orderService;
    }

    public IReadOnlyList<Order> Orders { get; private set; } = Array.Empty<Order>();

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        Orders = await _orderService.GetPendingOrdersAsync(cancellationToken);
    }

    public async Task<IActionResult> OnPostProcessAsync(int orderId, CancellationToken cancellationToken)
    {
        await _orderService.UpdateOrderStatusAsync(orderId, OrderStatus.Processed, cancellationToken);
        TempData["StatusMessage"] = $"Order #{orderId} marked as processed.";
        return RedirectToPage();
    }
}
