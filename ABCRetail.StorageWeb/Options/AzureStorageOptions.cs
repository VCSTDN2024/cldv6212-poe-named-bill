namespace ABCRetail.StorageWeb.Options;

public sealed class AzureStorageOptions
{
    public const string SectionName = "AzureStorage";

    public string AccountConnectionString { get; set; } = string.Empty;

    public string CustomersTableName { get; set; } = "customers";

    public string ProductsTableName { get; set; } = "products";

    public string MediaContainerName { get; set; } = "mediacontent";

    public string OperationsQueueName { get; set; } = "operations-queue";

    public string ContractsFileShareName { get; set; } = "contracts";

    public string DefaultContractsDirectory { get; set; } = "contracts";

    public string PlaceholderStudentNumber { get; set; } = "ST10445399";

    public string PlaceholderModuleCode { get; set; } = "CLDV6212";

    public bool UseAzuriteForLocalDevelopment { get; set; } = false;
}
