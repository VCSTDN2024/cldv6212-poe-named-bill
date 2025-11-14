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

public sealed class MediaFunctions
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly MediaStorageService _mediaStorageService;
    private readonly ILogger<MediaFunctions> _logger;

    public MediaFunctions(MediaStorageService mediaStorageService, ILogger<MediaFunctions> logger)
    {
        _mediaStorageService = mediaStorageService;
        _logger = logger;
    }

    [Function("MediaUpload")]
    public async Task<HttpResponseData> UploadAsync(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "media" )] HttpRequestData request)
    {
        var cancellationToken = request.FunctionContext.CancellationToken;
        var payload = await JsonSerializer.DeserializeAsync<MediaUploadRequest>(request.Body, SerializerOptions, cancellationToken);

        if (payload is null)
        {
            return await CreateProblemResponseAsync(request, HttpStatusCode.BadRequest, "Request body was empty or malformed.");
        }

        var validationResults = new List<ValidationResult>();
        if (!Validator.TryValidateObject(payload, new ValidationContext(payload), validationResults, validateAllProperties: true))
        {
            return await CreateProblemResponseAsync(request, HttpStatusCode.BadRequest, string.Join(" ", validationResults.Select(r => r.ErrorMessage).Where(m => !string.IsNullOrWhiteSpace(m))));
        }

        byte[] bytes;
        try
        {
            bytes = Convert.FromBase64String(payload.Base64Data);
        }
        catch (FormatException ex)
        {
            _logger.LogWarning(ex, "Failed to decode base64 media payload for {FileName}", payload.FileName);
            return await CreateProblemResponseAsync(request, HttpStatusCode.BadRequest, "The provided Base64 data could not be decoded.");
        }

        await using var stream = new MemoryStream(bytes);
        var contentType = string.IsNullOrWhiteSpace(payload.ContentType) ? "application/octet-stream" : payload.ContentType;
        await _mediaStorageService.UploadMediaAsync(payload.FileName, stream, contentType, cancellationToken);

        _logger.LogInformation("Media file {FileName} uploaded with {Length} bytes", payload.FileName, bytes.Length);

        var response = request.CreateResponse(HttpStatusCode.OK);
        response.Headers.Add("Content-Type", "application/json");
        await response.WriteStringAsync(JsonSerializer.Serialize(new
        {
            Message = "Media file uploaded successfully.",
            payload.FileName,
            BlobUri = _mediaStorageService.GetBlobUri(payload.FileName)
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

    private sealed class MediaUploadRequest
    {
        [Required]
        [StringLength(256, ErrorMessage = "File names must be under 256 characters.")]
        public string FileName { get; set; } = string.Empty;

        [Required]
        [StringLength(128, ErrorMessage = "Content type must be under 128 characters.")]
        public string ContentType { get; set; } = string.Empty;

        [Required]
        public string Base64Data { get; set; } = string.Empty;
    }
}
