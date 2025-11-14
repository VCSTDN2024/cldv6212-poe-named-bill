using ABCRetail.StorageWeb.Services;
using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ABCRetail.StorageWeb.Pages.Media;

public sealed class IndexModel : PageModel
{
    private readonly MediaStorageService _mediaStorageService;

    public IndexModel(MediaStorageService mediaStorageService)
    {
        _mediaStorageService = mediaStorageService;
    }

    public IReadOnlyList<MediaEntry> MediaFiles { get; private set; } = Array.Empty<MediaEntry>();

    [BindProperty]
    public IFormFile? Upload { get; set; }

    [TempData]
    public string? StatusMessage { get; set; }

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        var blobs = await _mediaStorageService.ListMediaAsync(cancellationToken);
        MediaFiles = blobs.Select(blob => new MediaEntry(blob, _mediaStorageService.GetBlobUri(blob.Name))).ToList();
    }

    public async Task<IActionResult> OnPostUploadAsync(CancellationToken cancellationToken)
    {
        if (Upload is null || Upload.Length == 0)
        {
            ModelState.AddModelError(string.Empty, "Please select a media file to upload.");
            await OnGetAsync(cancellationToken);
            return Page();
        }

        var blobName = Upload.FileName;
        await using var stream = Upload.OpenReadStream();
        await _mediaStorageService.UploadMediaAsync(blobName, stream, Upload.ContentType, cancellationToken);
        StatusMessage = $"Uploaded media file '{blobName}'.";

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync(string blobName, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(blobName))
        {
            await _mediaStorageService.DeleteMediaAsync(blobName, cancellationToken);
            StatusMessage = $"Deleted media file '{blobName}'.";
        }

        return RedirectToPage();
    }
}

public sealed record MediaEntry(BlobItem Blob, Uri Uri);
