using ABCRetail.StorageWeb.Data;
using ABCRetail.StorageWeb.Options;
using ABCRetail.StorageWeb.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;
        options.Password.RequireDigit = true;
        options.Password.RequireUppercase = false;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequiredLength = 8;
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddOptions<AzureStorageOptions>()
    .Bind(builder.Configuration.GetSection(AzureStorageOptions.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddSingleton<StorageClientFactory>();
builder.Services.AddSingleton<CustomerProfileService>();
builder.Services.AddSingleton<ProductCatalogService>();
builder.Services.AddSingleton<MediaStorageService>();
builder.Services.AddSingleton<OperationsQueueService>();
builder.Services.AddSingleton<ContractsFileService>();
builder.Services.AddHostedService<StorageInitializationHostedService>();
builder.Services.AddScoped<CartService>();
builder.Services.AddScoped<OrderService>();
builder.Services.AddScoped<IdentitySeeder>();

// Razor Pages render the storage demo screens.
builder.Services.AddRazorPages();

var app = builder.Build();

await DatabaseInitializer.InitializeAsync(app.Services);

// Keep the HTTP pipeline production focused while still supporting local development.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // Retain the default 30-day HSTS window so browsers keep enforcing HTTPS after first contact.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

app.Run();
