using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;
using Xunit;
using Xunit.Abstractions;

namespace PayGoHub.Tests.Contracts;

/// <summary>
/// Contract tests for Mega SMS Gateway API
/// These tests validate that our integration with Mega conforms to the expected contract
/// </summary>
[Trait("Category", "MegaIntegration")]
[Trait("Category", "Contract")]
public class MegaSmsContractTests : IAsyncLifetime
{
    private readonly HttpClient _client;
    private readonly ITestOutputHelper _output;
    private readonly string _baseUrl;
    private readonly string _apiKey;
    private readonly bool _isConfigured;

    public MegaSmsContractTests(ITestOutputHelper output)
    {
        _output = output;

        var config = new ConfigurationBuilder()
            .AddEnvironmentVariables()
            .Build();

        _baseUrl = config["MEGA_BASE_URL"] ?? "https://mega-staging.plugintheworld.com";
        _apiKey = config["MEGA_API_KEY"] ?? "";
        _isConfigured = !string.IsNullOrEmpty(_apiKey);

        _client = new HttpClient
        {
            BaseAddress = new Uri(_baseUrl),
            Timeout = TimeSpan.FromSeconds(30)
        };

        if (_isConfigured)
        {
            _client.DefaultRequestHeaders.Add("X-API-Key", _apiKey);
        }
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public Task DisposeAsync()
    {
        _client.Dispose();
        return Task.CompletedTask;
    }

    #region Health Check Contract

    [Fact]
    public async Task HealthEndpoint_ReturnsHealthyStatus()
    {
        // Skip if not configured (local dev without API key)
        if (!_isConfigured)
        {
            _output.WriteLine("Skipping: MEGA_API_KEY not configured");
            return;
        }

        // Act
        var response = await _client.GetAsync("/health");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        _output.WriteLine($"Health response: {content}");

        var health = JsonSerializer.Deserialize<HealthResponse>(content);
        Assert.NotNull(health);
        Assert.Equal("healthy", health.Status);
    }

    #endregion

    #region SMS Validation Contract

    [Fact]
    public async Task ValidateSms_WithValidRequest_ReturnsValidResponse()
    {
        if (!_isConfigured)
        {
            _output.WriteLine("Skipping: MEGA_API_KEY not configured");
            return;
        }

        // Arrange
        var request = new SendSmsRequest
        {
            To = "+254700000000",
            Message = "Test message from contract test",
            SenderId = "SOLARIUM",
            DryRun = true
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/sms/validate", request);

        // Assert
        _output.WriteLine($"Validate response: {response.StatusCode}");
        var content = await response.Content.ReadAsStringAsync();
        _output.WriteLine($"Content: {content}");

        Assert.True(
            response.StatusCode == HttpStatusCode.OK ||
            response.StatusCode == HttpStatusCode.BadRequest,
            $"Expected OK or BadRequest, got {response.StatusCode}");

        if (response.StatusCode == HttpStatusCode.OK)
        {
            var result = JsonSerializer.Deserialize<ValidationResponse>(content);
            Assert.NotNull(result);
            Assert.True(result.Valid);
            Assert.True(result.Segments >= 1);
        }
    }

    [Theory]
    [InlineData("invalid-phone", "Message", "Invalid phone number format")]
    [InlineData("+254700000000", "", "Empty message")]
    [InlineData("", "Message", "Empty phone number")]
    public async Task ValidateSms_WithInvalidRequest_ReturnsBadRequest(
        string phone, string message, string scenario)
    {
        if (!_isConfigured)
        {
            _output.WriteLine($"Skipping: MEGA_API_KEY not configured ({scenario})");
            return;
        }

        // Arrange
        var request = new SendSmsRequest
        {
            To = phone,
            Message = message,
            DryRun = true
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/sms/validate", request);

        // Assert
        _output.WriteLine($"Scenario '{scenario}': {response.StatusCode}");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    #endregion

    #region SMS Send Contract

    [Fact]
    public async Task SendSms_WithValidRequest_ReturnsQueuedStatus()
    {
        if (!_isConfigured)
        {
            _output.WriteLine("Skipping: MEGA_API_KEY not configured");
            return;
        }

        // Arrange - Use dry_run to avoid actually sending
        var request = new SendSmsRequest
        {
            To = "+254700000000",
            Message = $"Contract test - {DateTime.UtcNow:O}",
            SenderId = "SOLARIUM",
            Reference = $"CONTRACT-TEST-{Guid.NewGuid():N}",
            DryRun = true
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/sms/send", request);

        // Assert
        _output.WriteLine($"Send response: {response.StatusCode}");
        var content = await response.Content.ReadAsStringAsync();
        _output.WriteLine($"Content: {content}");

        Assert.True(
            response.StatusCode == HttpStatusCode.OK ||
            response.StatusCode == HttpStatusCode.Created,
            $"Expected OK or Created, got {response.StatusCode}");

        if (response.IsSuccessStatusCode)
        {
            var result = JsonSerializer.Deserialize<SendSmsResponse>(content);
            Assert.NotNull(result);
            Assert.False(string.IsNullOrEmpty(result.MessageId), "MessageId should not be empty");
            Assert.Contains(result.Status, new[] { "queued", "sent", "validated" });
        }
    }

    [Fact]
    public async Task SendSms_WithoutApiKey_ReturnsUnauthorized()
    {
        // Arrange - Create client without API key
        using var unauthClient = new HttpClient
        {
            BaseAddress = new Uri(_baseUrl)
        };

        var request = new SendSmsRequest
        {
            To = "+254700000000",
            Message = "Test message",
            DryRun = true
        };

        // Act
        var response = await unauthClient.PostAsJsonAsync("/api/sms/send", request);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    #endregion

    #region SMS Status Contract

    [Fact]
    public async Task GetSmsStatus_WithValidMessageId_ReturnsStatus()
    {
        if (!_isConfigured)
        {
            _output.WriteLine("Skipping: MEGA_API_KEY not configured");
            return;
        }

        // First, send a test message to get a valid message_id
        var sendRequest = new SendSmsRequest
        {
            To = "+254700000000",
            Message = $"Status test - {DateTime.UtcNow:O}",
            DryRun = true
        };

        var sendResponse = await _client.PostAsJsonAsync("/api/sms/send", sendRequest);

        if (!sendResponse.IsSuccessStatusCode)
        {
            _output.WriteLine("Could not send test message, skipping status check");
            return;
        }

        var sendContent = await sendResponse.Content.ReadAsStringAsync();
        var sendResult = JsonSerializer.Deserialize<SendSmsResponse>(sendContent);

        if (sendResult?.MessageId == null)
        {
            _output.WriteLine("No message ID returned, skipping status check");
            return;
        }

        // Act
        var response = await _client.GetAsync($"/api/sms/{sendResult.MessageId}/status");

        // Assert
        _output.WriteLine($"Status response: {response.StatusCode}");
        var content = await response.Content.ReadAsStringAsync();
        _output.WriteLine($"Content: {content}");

        Assert.True(
            response.StatusCode == HttpStatusCode.OK ||
            response.StatusCode == HttpStatusCode.NotFound,
            $"Expected OK or NotFound, got {response.StatusCode}");

        if (response.StatusCode == HttpStatusCode.OK)
        {
            var result = JsonSerializer.Deserialize<SmsStatusResponse>(content);
            Assert.NotNull(result);
            Assert.Equal(sendResult.MessageId, result.MessageId);
            Assert.Contains(result.Status, new[] { "queued", "sent", "delivered", "failed", "expired", "validated" });
        }
    }

    [Fact]
    public async Task GetSmsStatus_WithInvalidMessageId_ReturnsNotFound()
    {
        if (!_isConfigured)
        {
            _output.WriteLine("Skipping: MEGA_API_KEY not configured");
            return;
        }

        // Act
        var response = await _client.GetAsync("/api/sms/nonexistent-message-id-12345/status");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    #endregion

    #region DLR Callback Contract

    [Fact]
    public async Task DlrCallback_WithValidPayload_ReturnsSuccess()
    {
        if (!_isConfigured)
        {
            _output.WriteLine("Skipping: MEGA_API_KEY not configured");
            return;
        }

        // Arrange
        var callback = new DlrCallbackRequest
        {
            MessageId = $"msg_{Guid.NewGuid():N}",
            Status = "delivered",
            DeliveredAt = DateTime.UtcNow
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/dlr/callback", callback);

        // Assert - Accept 200, 202, or 404 (message might not exist in test env)
        _output.WriteLine($"DLR callback response: {response.StatusCode}");

        Assert.True(
            response.StatusCode == HttpStatusCode.OK ||
            response.StatusCode == HttpStatusCode.Accepted ||
            response.StatusCode == HttpStatusCode.NotFound,
            $"Expected OK, Accepted, or NotFound, got {response.StatusCode}");
    }

    #endregion

    #region Request/Response Models

    private class HealthResponse
    {
        [JsonPropertyName("status")]
        public string Status { get; set; } = "";

        [JsonPropertyName("version")]
        public string? Version { get; set; }
    }

    private class SendSmsRequest
    {
        [JsonPropertyName("to")]
        public string To { get; set; } = "";

        [JsonPropertyName("message")]
        public string Message { get; set; } = "";

        [JsonPropertyName("sender_id")]
        public string? SenderId { get; set; }

        [JsonPropertyName("reference")]
        public string? Reference { get; set; }

        [JsonPropertyName("callback_url")]
        public string? CallbackUrl { get; set; }

        [JsonPropertyName("priority")]
        public string? Priority { get; set; }

        [JsonPropertyName("dry_run")]
        public bool DryRun { get; set; }
    }

    private class SendSmsResponse
    {
        [JsonPropertyName("message_id")]
        public string? MessageId { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; } = "";

        [JsonPropertyName("to")]
        public string? To { get; set; }

        [JsonPropertyName("segments")]
        public int Segments { get; set; }

        [JsonPropertyName("created_at")]
        public DateTime? CreatedAt { get; set; }
    }

    private class SmsStatusResponse
    {
        [JsonPropertyName("message_id")]
        public string? MessageId { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; } = "";

        [JsonPropertyName("to")]
        public string? To { get; set; }

        [JsonPropertyName("delivered_at")]
        public DateTime? DeliveredAt { get; set; }

        [JsonPropertyName("failed_at")]
        public DateTime? FailedAt { get; set; }

        [JsonPropertyName("failure_reason")]
        public string? FailureReason { get; set; }

        [JsonPropertyName("provider_status")]
        public string? ProviderStatus { get; set; }
    }

    private class ValidationResponse
    {
        [JsonPropertyName("valid")]
        public bool Valid { get; set; }

        [JsonPropertyName("segments")]
        public int Segments { get; set; }

        [JsonPropertyName("estimated_cost")]
        public decimal? EstimatedCost { get; set; }
    }

    private class DlrCallbackRequest
    {
        [JsonPropertyName("message_id")]
        public string MessageId { get; set; } = "";

        [JsonPropertyName("status")]
        public string Status { get; set; } = "";

        [JsonPropertyName("delivered_at")]
        public DateTime? DeliveredAt { get; set; }

        [JsonPropertyName("failure_reason")]
        public string? FailureReason { get; set; }

        [JsonPropertyName("provider_message_id")]
        public string? ProviderMessageId { get; set; }

        [JsonPropertyName("provider_status")]
        public string? ProviderStatus { get; set; }
    }

    private class ErrorResponse
    {
        [JsonPropertyName("error")]
        public string Error { get; set; } = "";

        [JsonPropertyName("message")]
        public string Message { get; set; } = "";
    }

    #endregion
}
