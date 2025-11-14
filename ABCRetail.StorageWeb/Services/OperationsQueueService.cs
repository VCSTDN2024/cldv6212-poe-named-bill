using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;

namespace ABCRetail.StorageWeb.Services;

public sealed class OperationsQueueService
{
    private readonly QueueClient _queueClient;

    public OperationsQueueService(StorageClientFactory factory)
    {
        _queueClient = factory.CreateOperationsQueueClient();
    }

    public async Task EnsureInitializedAsync(CancellationToken cancellationToken)
    {
        await _queueClient.CreateIfNotExistsAsync(cancellationToken: cancellationToken);
    }

    public async Task EnqueueMessageAsync(string message, CancellationToken cancellationToken = default)
    {
    await _queueClient.SendMessageAsync(message, cancellationToken);
    }

    public async Task<IReadOnlyList<PeekedMessage>> PeekMessagesAsync(int maxMessages = 10, CancellationToken cancellationToken = default)
    {
        var response = await _queueClient.PeekMessagesAsync(maxMessages, cancellationToken: cancellationToken);
        return response.Value;
    }

    public async Task ClearAsync(CancellationToken cancellationToken = default)
    {
        await _queueClient.ClearMessagesAsync(cancellationToken);
    }
}
