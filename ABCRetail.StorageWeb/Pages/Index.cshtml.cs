using ABCRetail.StorageWeb.Options;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;

namespace ABCRetail.StorageWeb.Pages;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    private readonly AzureStorageOptions _options;

    public IndexModel(ILogger<IndexModel> logger, IOptions<AzureStorageOptions> options)
    {
        _logger = logger;
        _options = options.Value;
    }

    public AzureStorageOptions StorageOptions => _options;

    public void OnGet()
    {
        _logger.LogInformation("Dashboard viewed at {Time}", DateTimeOffset.UtcNow);
    }
}
