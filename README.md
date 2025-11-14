# ABC Retail Azure Storage Solution (Parts 1 & 2)

> Student: **ST10445399** · Module: **CLDV6212** · Storage account: **st10445399**

## 📦 Overview

This solution now includes the original ASP.NET Core Razor Pages dashboard **and** an Azure Functions project so ABC Retail can orchestrate storage workflows from serverless endpoints. Together they demonstrate how to modernise the storage foundation with:

- **Azure Table Storage** for customer profiles and product catalogue data.
- **Azure Blob Storage** for media assets.
- **Azure Queue Storage** for lightweight order and inventory events.
- **Azure File Shares** for storing contract documents.
- **Azure Functions (Part 2)** to trigger the same storage services from HTTP endpoints suitable for integrations, automation, or mobile clients.

The dashboards and data entry pages remain optimised for Part 1 evidence, while the Functions project and discussion content satisfy Part 2 deliverables.

## 🚀 Getting Started

### Prerequisites

- .NET 8 SDK
- Azure subscription (or Azurite for local emulation)
- Visual Studio 2022 / VS Code
- Azure Storage account with the following resources:
  - Tables: `customers`, `products`
  - Blob container: `mediacontent`
  - Queue: `operations-queue`
  - File share: `contracts` (includes a `contracts` directory for uploads)

> Copy the provided `appsettings.Template.json` files to `appsettings.json` (web + functions projects) and fill in your own connection strings before running locally.

### Configuration

All configuration lives under the `AzureStorage` section in `appsettings.json`:

```json
{
  "AzureStorage": {
    "AccountConnectionString": "<REPLACE_WITH_STORAGE_CONNECTION_STRING>",
    "CustomersTableName": "customers",
    "ProductsTableName": "products",
    "MediaContainerName": "mediacontent",
    "OperationsQueueName": "operations-queue",
    "ContractsFileShareName": "contracts",
    "DefaultContractsDirectory": "contracts",
    "PlaceholderStudentNumber": "ST10445399",
    "PlaceholderModuleCode": "CLDV6212",
    "UseAzuriteForLocalDevelopment": false
  }
}
```

- For local development, keep secrets out of source control by using `appsettings.Development.json`, environment variables, or [`dotnet user-secrets`](https://learn.microsoft.com/aspnet/core/security/app-secrets).
- Azurite does not currently emulate Azure File Shares; continue using the real storage account for the Contracts feature.

### Running the app locally

```powershell
cd "c:\Users\Kiibzz Bill\Desktop\testt2\ABCRetail.StorageWeb"
dotnet run
```

Then browse to `https://localhost:5001` (or the HTTP port indicated by the console output).

### Running the Azure Functions locally (Part 2)

Install the [Azure Functions Core Tools](https://learn.microsoft.com/azure/azure-functions/functions-run-local) if you have not already.

```powershell
cd "c:\Users\Kiibzz Bill\Desktop\testt2\ABCRetail.StorageFunctions"
func start
```

All functions are HTTP-triggered and secured with the default *Function* key. Use the sample payloads below with tools such as *curl*, *Postman*, or the built-in Azure Portal tester.

| Function | Route | Purpose | Sample Payload |
|----------|-------|---------|----------------|
| `CustomerProfileUpsert` | `POST /api/customers` | Stores/updates a customer record in the `customers` table. | `{ "segment": "Retail", "customerId": "CUST-001", "firstName": "Ada", "lastName": "Lovelace", "email": "ada@example.com", "loyaltyTier": "Gold", "phoneNumber": "+27 00 000 0000" }` |
| `MediaUpload` | `POST /api/media` | Uploads a Base64-encoded blob to `mediacontent`. | `{ "fileName": "hero-banner.png", "contentType": "image/png", "base64Data": "<BASE64 PAYLOAD>" }` |
| `OperationsEnqueue` | `POST /api/operations` | Enqueues a transaction message to `operations-queue`. | `{ "message": "Processed order ORD-2045" }` |
| `OperationsPeek` | `GET /api/operations` | Returns up to 16 messages currently in the queue. | *(no body)* |
| `ContractsUpload` | `POST /api/contracts` | Saves a Base64-encoded document to the `contracts` file share. | `{ "fileName": "agreement.pdf", "base64Data": "<BASE64 PAYLOAD>" }` |

All endpoints reuse the same storage services created for Part 1, ensuring consistency between the web UI and the serverless layer.

## 🧪 Validation

- `dotnet build` – ensure the solution compiles.
- `dotnet test` – confirm unit tests and function compilation succeed.
- `dotnet run` – smoke test all Razor Pages locally.
- `func start` – verify each Azure Function responds with `200/202` using the sample payloads.
- Upload a minimum of five entities/files per storage type to capture the required screenshots for both the web UI and serverless flows.

## ☁️ Deployment Notes

1. Create an Azure App Service targeting .NET 8.
2. Configure the same `AzureStorage` settings in App Service → **Configuration**.
3. Deploy the project via GitHub Actions, Visual Studio publish, or `az webapp deploy`.
4. Record the live URL in your submission document. Format: `http://student_number.azurewebsites.net`.
5. Provision an Azure Functions App (Consumption or Premium plan) targeting .NET 8, configure identical `AzureStorage` settings as application settings, and deploy `ABCRetail.StorageFunctions` via `func azure functionapp publish`, GitHub Actions, or Visual Studio.

## 📝 Submission Checklist

- [ ] Capture at least five entries per storage service and take screenshots.
- [ ] Publish the Razor Pages app **and** the Azure Function App, capturing deployed screenshots.
- [ ] Include the student number, module code, production URLs, and GitHub repo in the MS Word document (placeholders are already present if details change).
- [ ] Paste HTTP request/response screenshots for each function to prove Part 2 integration works end-to-end.
- [ ] Answer the Part 2 discussion prompts (see summary below) and expand them in the submission document.

## 📄 Licensing

The project uses only Microsoft and Azure SDK dependencies; see `ABCRetail.StorageWeb.csproj` for automatic notices.

## 💬 Discussion Highlights (Part 2)

- **Azure Event Hubs:** Ideal for streaming telemetry such as website clickstreams or in-store sensor readings. Integrating Event Hubs would let ABC Retail capture real-time customer behaviour, feed analytics/AI pipelines, and surface personalised recommendations or alerts without overwhelming transactional systems.
- **Azure Service Bus (Event Bus):** Enables reliable, ordered messaging between the web app, inventory systems, and third-party services. By decoupling publishers and subscribers, Service Bus supports features like delayed delivery, dead-letter queues, and topic subscriptions—perfect for notifying customers about order milestones or syncing stock levels across channels.

Expand these talking points in the Word submission, referencing how the store’s customer experience improves (real-time updates, personalised messaging, resilient workflows).
