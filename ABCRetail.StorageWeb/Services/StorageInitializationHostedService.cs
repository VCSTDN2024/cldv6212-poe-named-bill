namespace ABCRetail.StorageWeb.Services;

public sealed class StorageInitializationHostedService : IHostedService
{
    private readonly IServiceProvider _serviceProvider;

    public StorageInitializationHostedService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var customerService = scope.ServiceProvider.GetRequiredService<CustomerProfileService>();
        var productService = scope.ServiceProvider.GetRequiredService<ProductCatalogService>();
        var mediaService = scope.ServiceProvider.GetRequiredService<MediaStorageService>();
        var queueService = scope.ServiceProvider.GetRequiredService<OperationsQueueService>();
        var contractService = scope.ServiceProvider.GetRequiredService<ContractsFileService>();

        await Task.WhenAll(
            customerService.EnsureInitializedAsync(cancellationToken),
            productService.EnsureInitializedAsync(cancellationToken),
            mediaService.EnsureInitializedAsync(cancellationToken),
            queueService.EnsureInitializedAsync(cancellationToken),
            contractService.EnsureInitializedAsync(cancellationToken));
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
