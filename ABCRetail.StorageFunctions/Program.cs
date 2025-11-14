using ABCRetail.StorageWeb.Options;
using ABCRetail.StorageWeb.Services;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = FunctionsApplication.CreateBuilder(args);

builder.Configuration
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

builder.ConfigureFunctionsWebApplication();

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

builder.Build().Run();
