using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Text.Json;
using ABCRetail.StorageWeb.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace ABCRetail.StorageFunctions.Functions;

public sealed class OperationsQueueFunctions
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly OperationsQueueService _queueService;
    private readonly ILogger<OperationsQueueFunctions> _logger;

    public OperationsQueueFunctions(OperationsQueueService queueService, ILogger<OperationsQueueFunctions> logger)
    {
        _queueService = queueService;
        _logger = logger;
    }

    [Function("OperationsEnqueue")]
    public async Task<HttpResponseData> EnqueueAsync(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "operations")]
        HttpRequestData request)
    {
        var cancellationToken = request.FunctionContext.CancellationToken;
        var payload = await JsonSerializer.DeserializeAsync<QueueMessageRequest>(request.Body, SerializerOptions, cancellationToken);

        if (payload is null)
        {
            return await CreateProblemResponseAsync(request, HttpStatusCode.BadRequest, "Request body was empty or malformed.");
        }

        var validationResults = new List<ValidationResult>();
        if (!Validator.TryValidateObject(payload, new ValidationContext(payload), validationResults, validateAllProperties: true))
        {
            return await CreateProblemResponseAsync(request, HttpStatusCode.BadRequest, string.Join(" ", validationResults.Select(r => r.ErrorMessage).Where(m => !string.IsNullOrWhiteSpace(m))));
        }

        await _queueService.EnqueueMessageAsync(payload.Message.Trim(), cancellationToken);
        _logger.LogInformation("Queued operations message of length {Length}", payload.Message.Length);

        var response = request.CreateResponse(HttpStatusCode.Accepted);
        response.Headers.Add("Content-Type", "application/json");
        await response.WriteStringAsync(JsonSerializer.Serialize(new
        {
            Message = "Operations message enqueued successfully."
        }, SerializerOptions), cancellationToken);

        return response;
    }

    [Function("OperationsPeek")]
    public async Task<HttpResponseData> PeekAsync(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "operations")]
        HttpRequestData request)
    {
        var cancellationToken = request.FunctionContext.CancellationToken;
        var peekCount = 16;
        var messages = await _queueService.PeekMessagesAsync(peekCount, cancellationToken);

        var response = request.CreateResponse(HttpStatusCode.OK);
        response.Headers.Add("Content-Type", "application/json");
        await response.WriteStringAsync(JsonSerializer.Serialize(new
        {
            Count = messages.Count,
            Messages = messages.Select(message => new
            {
                message.MessageText,
                message.InsertedOn,
                message.ExpiresOn
            })
        }, SerializerOptions), cancellationToken);

        return response;
    }

    private static async Task<HttpResponseData> CreateProblemResponseAsync(HttpRequestData request, HttpStatusCode statusCode, string message)
    {
        var response = request.CreateResponse(statusCode);
        response.Headers.Add("Content-Type", "application/json");
        await response.WriteStringAsync(JsonSerializer.Serialize(new { Error = message }, SerializerOptions));
        return response;
    }

    private sealed class QueueMessageRequest
    {
        [Required(ErrorMessage = "Please provide a queue message body.")]
        [StringLength(7680, ErrorMessage = "Queue messages must be smaller than 7.5KB.")]
        public string Message { get; set; } = string.Empty;
    }
}
