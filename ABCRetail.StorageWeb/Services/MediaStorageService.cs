using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace ABCRetail.StorageWeb.Services;

public sealed class MediaStorageService
{
    private readonly BlobContainerClient _containerClient;

    public MediaStorageService(StorageClientFactory factory)
    {
        _containerClient = factory.CreateMediaContainerClient();
    }

    public async Task EnsureInitializedAsync(CancellationToken cancellationToken)
    {
    await _containerClient.CreateIfNotExistsAsync(PublicAccessType.None, cancellationToken: cancellationToken);
    }

    public async Task<IReadOnlyList<BlobItem>> ListMediaAsync(CancellationToken cancellationToken = default)
    {
        var results = new List<BlobItem>();
        await foreach (var blob in _containerClient.GetBlobsAsync(cancellationToken: cancellationToken))
        {
            results.Add(blob);
        }

        return results;
    }

    public async Task UploadMediaAsync(string blobName, Stream content, string? contentType, CancellationToken cancellationToken = default)
    {
        var blobClient = _containerClient.GetBlobClient(blobName);
        var headers = new BlobHttpHeaders
        {
            ContentType = contentType ?? "application/octet-stream"
        };

        await blobClient.DeleteIfExistsAsync(DeleteSnapshotsOption.IncludeSnapshots, cancellationToken: cancellationToken);
        var options = new BlobUploadOptions
        {
            HttpHeaders = headers
        };
        await blobClient.UploadAsync(content, options, cancellationToken);
    }

    public Uri GetBlobUri(string blobName)
    {
        return _containerClient.GetBlobClient(blobName).Uri;
    }

    public async Task DeleteMediaAsync(string blobName, CancellationToken cancellationToken = default)
    {
        var blobClient = _containerClient.GetBlobClient(blobName);
        await blobClient.DeleteIfExistsAsync(DeleteSnapshotsOption.IncludeSnapshots, cancellationToken: cancellationToken);
    }
}
