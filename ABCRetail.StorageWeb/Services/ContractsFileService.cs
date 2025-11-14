using Azure;
using Azure.Storage.Files.Shares;
using Azure.Storage.Files.Shares.Models;
using ABCRetail.StorageWeb.Options;
using Microsoft.Extensions.Options;

namespace ABCRetail.StorageWeb.Services;

public sealed class ContractsFileService
{
    private readonly ShareClient _shareClient;
    private readonly AzureStorageOptions _options;

    public ContractsFileService(StorageClientFactory factory, IOptions<AzureStorageOptions> options)
    {
        _shareClient = factory.CreateContractsShareClient();
        _options = options.Value;
    }

    public async Task EnsureInitializedAsync(CancellationToken cancellationToken)
    {
        await _shareClient.CreateIfNotExistsAsync(cancellationToken: cancellationToken);
        var directoryClient = _shareClient.GetDirectoryClient(_options.DefaultContractsDirectory);
        await directoryClient.CreateIfNotExistsAsync(cancellationToken: cancellationToken);
    }

    public async Task<IReadOnlyList<ShareFileItem>> GetContractFilesAsync(CancellationToken cancellationToken = default)
    {
        var directoryClient = _shareClient.GetDirectoryClient(_options.DefaultContractsDirectory);
        var results = new List<ShareFileItem>();

        await foreach (var item in directoryClient.GetFilesAndDirectoriesAsync(cancellationToken: cancellationToken))
        {
            results.Add(item);
        }

        return results;
    }

    public async Task UploadContractAsync(string fileName, Stream content, CancellationToken cancellationToken = default)
    {
        var directoryClient = _shareClient.GetDirectoryClient(_options.DefaultContractsDirectory);
        var fileClient = directoryClient.GetFileClient(fileName);
        await fileClient.CreateAsync(content.Length, cancellationToken: cancellationToken);
        content.Position = 0;
        await fileClient.UploadRangeAsync(new HttpRange(0, content.Length), content, cancellationToken: cancellationToken);
    }

    public ShareFileClient GetFileClient(string fileName)
    {
        var directoryClient = _shareClient.GetDirectoryClient(_options.DefaultContractsDirectory);
        return directoryClient.GetFileClient(fileName);
    }

    public async Task DeleteContractAsync(string fileName, CancellationToken cancellationToken = default)
    {
        var fileClient = GetFileClient(fileName);
        await fileClient.DeleteIfExistsAsync(cancellationToken: cancellationToken);
    }
}
