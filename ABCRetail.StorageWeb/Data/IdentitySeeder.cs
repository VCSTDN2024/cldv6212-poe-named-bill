using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;

namespace ABCRetail.StorageWeb.Data;

public sealed class IdentitySeeder
{
    private readonly IConfiguration _configuration;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public IdentitySeeder(
        IConfiguration configuration,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager)
    {
        _configuration = configuration;
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        await EnsureRoleAsync("Administrator", cancellationToken);
        await EnsureRoleAsync("Customer", cancellationToken);

        await EnsureUserAsync(
            email: _configuration["Seed:AdminEmail"] ?? "admin@example.com",
            password: _configuration["Seed:AdminPassword"] ?? "Admin!234",
            role: "Administrator");

        await EnsureUserAsync(
            email: _configuration["Seed:CustomerEmail"] ?? "customer@example.com",
            password: _configuration["Seed:CustomerPassword"] ?? "Customer!234",
            role: "Customer");
    }

    private async Task EnsureRoleAsync(string roleName, CancellationToken cancellationToken)
    {
        if (await _roleManager.RoleExistsAsync(roleName))
        {
            return;
        }

        await _roleManager.CreateAsync(new IdentityRole(roleName));
    }

    private async Task EnsureUserAsync(string email, string password, string role)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user is null)
        {
            user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, password);
            if (!result.Succeeded)
            {
                throw new InvalidOperationException($"Failed to create seed user {email}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }
        }

        if (!await _userManager.IsInRoleAsync(user, role))
        {
            await _userManager.AddToRoleAsync(user, role);
        }
    }
}
