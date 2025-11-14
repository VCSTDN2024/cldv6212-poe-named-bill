using ABCRetail.StorageWeb.Models.Commerce;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ABCRetail.StorageWeb.Data;

public sealed class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<Cart> Carts => Set<Cart>();
    public DbSet<CartItem> CartItems => Set<CartItem>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Cart>(entity =>
        {
            entity.HasIndex(c => c.UserId).IsUnique();
            entity.Property(c => c.UserId).IsRequired();
        });

        builder.Entity<CartItem>(entity =>
        {
            entity.Property(i => i.ProductName).HasMaxLength(256);
            entity.Property(i => i.ProductPartitionKey).HasMaxLength(128);
            entity.Property(i => i.ProductRowKey).HasMaxLength(128);
            entity.Property(i => i.UnitPrice).HasPrecision(18, 2);
        });

        builder.Entity<Order>(entity =>
        {
            entity.Property(o => o.Status).HasConversion<int>();
            entity.Property(o => o.UserId).IsRequired();
        });

        builder.Entity<OrderItem>(entity =>
        {
            entity.Property(i => i.ProductName).HasMaxLength(256);
            entity.Property(i => i.ProductPartitionKey).HasMaxLength(128);
            entity.Property(i => i.ProductRowKey).HasMaxLength(128);
            entity.Property(i => i.UnitPrice).HasPrecision(18, 2);
        });
    }
}
