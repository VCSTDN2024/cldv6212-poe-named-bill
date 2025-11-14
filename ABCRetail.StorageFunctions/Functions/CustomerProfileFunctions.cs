using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Text.Json;
using ABCRetail.StorageWeb.Models;
using ABCRetail.StorageWeb.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace ABCRetail.StorageFunctions.Functions;

public sealed class CustomerProfileFunctions
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly CustomerProfileService _customerService;
    private readonly ILogger<CustomerProfileFunctions> _logger;

    public CustomerProfileFunctions(CustomerProfileService customerService, ILogger<CustomerProfileFunctions> logger)
    {
        _customerService = customerService;
        _logger = logger;
    }

    [Function("CustomerProfileUpsert")]
    public async Task<HttpResponseData> UpsertAsync(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "customers")] HttpRequestData request)
    {
        var cancellationToken = request.FunctionContext.CancellationToken;
        var payload = await JsonSerializer.DeserializeAsync<CustomerProfileInputModel>(request.Body, SerializerOptions, cancellationToken);

        if (payload is null)
        {
            return await CreateProblemResponseAsync(request, HttpStatusCode.BadRequest, "Request body was empty or malformed.");
        }

        var validationResults = new List<ValidationResult>();
        if (!Validator.TryValidateObject(payload, new ValidationContext(payload), validationResults, validateAllProperties: true))
        {
            return await CreateProblemResponseAsync(request, HttpStatusCode.BadRequest, string.Join(" ", validationResults.Select(r => r.ErrorMessage).Where(m => !string.IsNullOrWhiteSpace(m))));
        }

        var partitionKey = string.IsNullOrWhiteSpace(payload.Segment) ? "Retail" : payload.Segment.Trim();
        var rowKey = string.IsNullOrWhiteSpace(payload.CustomerId) ? Guid.NewGuid().ToString() : payload.CustomerId.Trim();

        var entity = new CustomerProfileEntity(partitionKey, rowKey)
        {
            FirstName = payload.FirstName,
            LastName = payload.LastName,
            Email = payload.Email,
            LoyaltyTier = payload.LoyaltyTier,
            PhoneNumber = payload.PhoneNumber
        };

        await _customerService.UpsertCustomerAsync(entity, cancellationToken);
        _logger.LogInformation("Customer profile saved for {Partition}/{Row}", partitionKey, rowKey);

        var response = request.CreateResponse(HttpStatusCode.OK);
        response.Headers.Add("Content-Type", "application/json");
        await response.WriteStringAsync(JsonSerializer.Serialize(new
        {
            Message = "Customer profile stored successfully.",
            PartitionKey = partitionKey,
            RowKey = rowKey
        }, SerializerOptions), cancellationToken);

        return response;
    }

    private static async Task<HttpResponseData> CreateProblemResponseAsync(HttpRequestData request, HttpStatusCode statusCode, string message)
    {
        var response = request.CreateResponse(statusCode);
        response.Headers.Add("Content-Type", "application/json");
        await response.WriteStringAsync(JsonSerializer.Serialize(new
        {
            Error = message
        }, SerializerOptions));
        return response;
    }
}
