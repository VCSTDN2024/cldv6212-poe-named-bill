using ABCRetail.StorageWeb.Data;
using ABCRetail.StorageWeb.Models;
using ABCRetail.StorageWeb.Models.Commerce;
using Microsoft.EntityFrameworkCore;

namespace ABCRetail.StorageWeb.Services;

public sealed class CartService
{
    private readonly ApplicationDbContext _dbContext;

    public CartService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Cart> GetOrCreateCartAsync(string userId, CancellationToken cancellationToken = default)
    {
        var cart = await _dbContext.Carts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.UserId == userId, cancellationToken);

        if (cart is not null)
        {
            return cart;
        }

        cart = new Cart { UserId = userId };
        _dbContext.Carts.Add(cart);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return cart;
    }

    public async Task AddItemAsync(string userId, ProductEntity product, int quantity, CancellationToken cancellationToken = default)
    {
        var cart = await GetOrCreateCartAsync(userId, cancellationToken);
        var existingItem = cart.Items.FirstOrDefault(i =>
            i.ProductPartitionKey == product.PartitionKey &&
            i.ProductRowKey == product.RowKey);

        if (existingItem is not null)
        {
            existingItem.Quantity += quantity;
        }
        else
        {
            cart.Items.Add(new CartItem
            {
                ProductPartitionKey = product.PartitionKey,
                ProductRowKey = product.RowKey,
                ProductName = product.Name ?? product.RowKey,
                UnitPrice = product.Price,
                Quantity = quantity,
                AddedAtUtc = DateTime.UtcNow
            });
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task RemoveItemAsync(string userId, int cartItemId, CancellationToken cancellationToken = default)
    {
        var cart = await GetOrCreateCartAsync(userId, cancellationToken);
        var item = cart.Items.FirstOrDefault(i => i.Id == cartItemId);
        if (item is null)
        {
            return;
        }

        cart.Items.Remove(item);
        _dbContext.CartItems.Remove(item);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task ClearCartAsync(string userId, CancellationToken cancellationToken = default)
    {
        var cart = await GetOrCreateCartAsync(userId, cancellationToken);
        if (cart.Items.Count == 0)
        {
            return;
        }

        _dbContext.CartItems.RemoveRange(cart.Items);
        cart.Items.Clear();
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
