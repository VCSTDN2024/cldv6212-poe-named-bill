using System.Security.Claims;
using ABCRetail.StorageWeb.Models.Commerce;
using ABCRetail.StorageWeb.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ABCRetail.StorageWeb.Pages.Orders;

[Authorize]
public sealed class IndexModel : PageModel
{
    private readonly OrderService _orderService;

    public IndexModel(OrderService orderService)
    {
        _orderService = orderService;
    }

    public IReadOnlyList<Order> Orders { get; private set; } = Array.Empty<Order>();

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        Orders = await _orderService.GetOrdersForUserAsync(userId, cancellationToken);
    }
}
