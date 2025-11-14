using ABCRetail.StorageWeb.Services;
using Azure.Storage.Files.Shares.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ABCRetail.StorageWeb.Pages.Contracts;

public sealed class IndexModel : PageModel
{
    private readonly ContractsFileService _contractsService;

    public IndexModel(ContractsFileService contractsService)
    {
        _contractsService = contractsService;
    }

    public IReadOnlyList<ShareFileItem> ContractFiles { get; private set; } = Array.Empty<ShareFileItem>();

    [BindProperty]
    public IFormFile? Upload { get; set; }

    [TempData]
    public string? StatusMessage { get; set; }

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        ContractFiles = await _contractsService.GetContractFilesAsync(cancellationToken);
    }

    public async Task<IActionResult> OnPostUploadAsync(CancellationToken cancellationToken)
    {
        if (Upload is null || Upload.Length == 0)
        {
            ModelState.AddModelError(string.Empty, "Select a file before uploading.");
            await OnGetAsync(cancellationToken);
            return Page();
        }

        await using var stream = Upload.OpenReadStream();
        await _contractsService.UploadContractAsync(Upload.FileName, stream, cancellationToken);
        StatusMessage = $"Uploaded contract '{Upload.FileName}'.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync(string fileName, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(fileName))
        {
            await _contractsService.DeleteContractAsync(fileName, cancellationToken);
            StatusMessage = $"Deleted contract '{fileName}'.";
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnGetDownloadAsync(string fileName, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            return RedirectToPage();
        }

        var fileClient = _contractsService.GetFileClient(fileName);
        if (!await fileClient.ExistsAsync(cancellationToken))
        {
            TempData["DownloadError"] = $"File '{fileName}' could not be found.";
            return RedirectToPage();
        }

        var downloadInfo = await fileClient.DownloadAsync(cancellationToken: cancellationToken);
        return File(downloadInfo.Value.Content, "application/octet-stream", fileName);
    }
}
