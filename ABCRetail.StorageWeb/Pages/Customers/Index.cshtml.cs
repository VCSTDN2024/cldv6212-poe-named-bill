using ABCRetail.StorageWeb.Models;
using ABCRetail.StorageWeb.Options;
using ABCRetail.StorageWeb.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;

namespace ABCRetail.StorageWeb.Pages.Customers;

public sealed class IndexModel : PageModel
{
    private readonly CustomerProfileService _customerService;
    private readonly AzureStorageOptions _options;

    public IndexModel(CustomerProfileService customerService, IOptions<AzureStorageOptions> options)
    {
        _customerService = customerService;
        _options = options.Value;
    }

    public IReadOnlyList<CustomerProfileEntity> Customers { get; private set; } = Array.Empty<CustomerProfileEntity>();

    [BindProperty]
    public CustomerProfileInputModel Input { get; set; } = new();

    [TempData]
    public string? StatusMessage { get; set; }

    public string StudentNumber => _options.PlaceholderStudentNumber;

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        Customers = await _customerService.GetCustomersAsync(50, cancellationToken);
    }

    public async Task<IActionResult> OnPostUpsertAsync(CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            await OnGetAsync(cancellationToken);
            return Page();
        }

        var partitionKey = string.IsNullOrWhiteSpace(Input.Segment) ? "Retail" : Input.Segment.Trim();
        var rowKey = string.IsNullOrWhiteSpace(Input.CustomerId) ? Guid.NewGuid().ToString() : Input.CustomerId.Trim();
        var entity = new CustomerProfileEntity(partitionKey, rowKey)
        {
            FirstName = Input.FirstName,
            LastName = Input.LastName,
            Email = Input.Email,
            LoyaltyTier = Input.LoyaltyTier,
            PhoneNumber = Input.PhoneNumber
        };

        await _customerService.UpsertCustomerAsync(entity, cancellationToken);
        StatusMessage = $"Saved customer profile for {entity.FirstName} {entity.LastName}.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync(string partitionKey, string rowKey, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(partitionKey) && !string.IsNullOrWhiteSpace(rowKey))
        {
            await _customerService.DeleteCustomerAsync(partitionKey, rowKey, cancellationToken);
            StatusMessage = $"Deleted customer {rowKey}.";
        }

        return RedirectToPage();
    }
}
