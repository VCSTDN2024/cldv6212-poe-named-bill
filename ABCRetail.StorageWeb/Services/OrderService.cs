using ABCRetail.StorageWeb.Data;
using ABCRetail.StorageWeb.Models.Commerce;
using Microsoft.EntityFrameworkCore;

namespace ABCRetail.StorageWeb.Services;

public sealed class OrderService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly CartService _cartService;

    public OrderService(ApplicationDbContext dbContext, CartService cartService)
    {
        _dbContext = dbContext;
        _cartService = cartService;
    }

    public async Task<IReadOnlyList<Order>> GetOrdersForUserAsync(string userId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Orders
            .Include(o => o.Items)
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Order>> GetPendingOrdersAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Orders
            .Include(o => o.Items)
            .Include(o => o.User)
            .OrderByDescending(o => o.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<Order?> GetOrderAsync(int orderId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == orderId, cancellationToken);
    }

    public async Task<Order?> PlaceOrderAsync(string userId, CancellationToken cancellationToken = default)
    {
        var cart = await _cartService.GetOrCreateCartAsync(userId, cancellationToken);
        if (!cart.Items.Any())
        {
            return null;
        }

        var order = new Order
        {
            UserId = userId,
            Status = OrderStatus.Placed,
            CreatedAtUtc = DateTime.UtcNow,
            Items = cart.Items.Select(item => new OrderItem
            {
                ProductPartitionKey = item.ProductPartitionKey,
                ProductRowKey = item.ProductRowKey,
                ProductName = item.ProductName,
                UnitPrice = item.UnitPrice,
                Quantity = item.Quantity
            }).ToList()
        };

        _dbContext.Orders.Add(order);
        _dbContext.CartItems.RemoveRange(cart.Items);
        cart.Items.Clear();

        await _dbContext.SaveChangesAsync(cancellationToken);
        return order;
    }

    public async Task UpdateOrderStatusAsync(int orderId, OrderStatus status, CancellationToken cancellationToken = default)
    {
        var order = await GetOrderAsync(orderId, cancellationToken);
        if (order is null)
        {
            return;
        }

        order.Status = status;
        if (status == OrderStatus.Processed)
        {
            order.ProcessedAtUtc = DateTime.UtcNow;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
