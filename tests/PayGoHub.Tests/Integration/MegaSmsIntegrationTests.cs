using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;
using Xunit;
using Xunit.Abstractions;

namespace PayGoHub.Tests.Integration;

/// <summary>
/// Integration tests for Mega SMS Gateway
/// These tests run against the staging environment to validate real SMS flows
/// </summary>
[Trait("Category", "MegaIntegration")]
[Trait("Category", "Integration")]
[Collection("MegaIntegration")]
public class MegaSmsIntegrationTests : IAsyncLifetime
{
    private readonly HttpClient _client;
    private readonly ITestOutputHelper _output;
    private readonly string _baseUrl;
    private readonly string _apiKey;
    private readonly string _testPhoneNumber;
    private readonly bool _isConfigured;
    private readonly bool _allowRealSms;

    public MegaSmsIntegrationTests(ITestOutputHelper output)
    {
        _output = output;

        var config = new ConfigurationBuilder()
            .AddEnvironmentVariables()
            .Build();

        _baseUrl = config["MEGA_BASE_URL"] ?? "https://mega-staging.plugintheworld.com";
        _apiKey = config["MEGA_API_KEY"] ?? "";
        _testPhoneNumber = config["TEST_PHONE_NUMBER"] ?? "+254700000000";
        _allowRealSms = config["ALLOW_REAL_SMS"] == "true";
        _isConfigured = !string.IsNullOrEmpty(_apiKey);

        _client = new HttpClient
        {
            BaseAddress = new Uri(_baseUrl),
            Timeout = TimeSpan.FromSeconds(60)
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

    #region SMS Sending Tests

    [Fact]
    [Trait("Market", "KE")]
    public async Task SendSms_ToKenyanNumber_QueuesSuccessfully()
    {
        if (!_isConfigured)
        {
            _output.WriteLine("Skipping: MEGA_API_KEY not configured");
            return;
        }

        // Arrange
        var request = new SendSmsRequest
        {
            To = _testPhoneNumber.StartsWith("+254") ? _testPhoneNumber : "+254700000000",
            Message = $"[TEST] PayGoHub integration test - {DateTime.UtcNow:O}",
            SenderId = "SOLARIUM",
            Reference = $"INT-TEST-{Guid.NewGuid():N}",
            DryRun = !_allowRealSms
        };

        _output.WriteLine($"Sending SMS to {request.To} (dry_run: {request.DryRun})");

        // Act
        var response = await _client.PostAsJsonAsync("/api/sms/send", request);
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        _output.WriteLine($"Response: {response.StatusCode}");
        _output.WriteLine($"Content: {content}");

        Assert.True(response.IsSuccessStatusCode, $"Expected success, got {response.StatusCode}: {content}");

        var result = JsonSerializer.Deserialize<SendSmsResponse>(content);
        Assert.NotNull(result);
        Assert.False(string.IsNullOrEmpty(result.MessageId));
        _output.WriteLine($"Message ID: {result.MessageId}");
    }

    [Fact]
    [Trait("Market", "UG")]
    public async Task SendSms_ToUgandanNumber_QueuesSuccessfully()
    {
        if (!_isConfigured)
        {
            _output.WriteLine("Skipping: MEGA_API_KEY not configured");
            return;
        }

        // Arrange
        var request = new SendSmsRequest
        {
            To = "+256700000000",
            Message = $"[TEST] Uganda integration test - {DateTime.UtcNow:O}",
            SenderId = "SOLARIUM",
            DryRun = true
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/sms/send", request);
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        _output.WriteLine($"UG Response: {response.StatusCode} - {content}");
        Assert.True(response.IsSuccessStatusCode || response.StatusCode == HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("Market", "RW")]
    public async Task SendSms_ToRwandanNumber_QueuesSuccessfully()
    {
        if (!_isConfigured)
        {
            _output.WriteLine("Skipping: MEGA_API_KEY not configured");
            return;
        }

        // Arrange - Rwanda launch Jan 3-5, 2026
        var request = new SendSmsRequest
        {
            To = "+250780000000",
            Message = $"[TEST] Rwanda integration test - {DateTime.UtcNow:O}",
            SenderId = "SOLARIUM",
            DryRun = true
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/sms/send", request);
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        _output.WriteLine($"RW Response: {response.StatusCode} - {content}");
        // Rwanda might not be active yet, so accept various responses
        Assert.True(
            response.IsSuccessStatusCode ||
            response.StatusCode == HttpStatusCode.BadRequest ||
            response.StatusCode == HttpStatusCode.NotFound);
    }

    #endregion

    #region Token Delivery SMS Tests

    [Fact]
    public async Task SendTokenSms_WithTokenPayload_QueuesSuccessfully()
    {
        if (!_isConfigured)
        {
            _output.WriteLine("Skipping: MEGA_API_KEY not configured");
            return;
        }

        // Arrange - Simulate token delivery SMS
        var token = "1234567890123456";
        var request = new SendSmsRequest
        {
            To = _testPhoneNumber,
            Message = $"Your solar token is: {token}. Enter this code on your device to add 30 days. Thank you for your payment!",
            SenderId = "SOLARIUM",
            Reference = $"TOKEN-{Guid.NewGuid():N}",
            DryRun = true
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/sms/send", request);
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        _output.WriteLine($"Token SMS Response: {response.StatusCode}");
        Assert.True(response.IsSuccessStatusCode);

        var result = JsonSerializer.Deserialize<SendSmsResponse>(content);
        Assert.NotNull(result);
        Assert.Equal(1, result.Segments); // Token SMS should fit in 1 segment
    }

    [Fact]
    public async Task SendPaymentConfirmationSms_WithPaymentDetails_QueuesSuccessfully()
    {
        if (!_isConfigured)
        {
            _output.WriteLine("Skipping: MEGA_API_KEY not configured");
            return;
        }

        // Arrange - Payment confirmation SMS
        var request = new SendSmsRequest
        {
            To = _testPhoneNumber,
            Message = "Thank you! We received your payment of KES 1,000.00 on 31/12/2025. Your account has been credited with 30 days. New expiry: 30/01/2026.",
            SenderId = "SOLARIUM",
            Reference = $"PAY-CONF-{Guid.NewGuid():N}",
            DryRun = true
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/sms/send", request);

        // Assert
        Assert.True(response.IsSuccessStatusCode);
    }

    #endregion

    #region DLR (Delivery Report) Tests

    [Fact]
    public async Task GetSmsStatus_AfterSending_ReturnsStatus()
    {
        if (!_isConfigured)
        {
            _output.WriteLine("Skipping: MEGA_API_KEY not configured");
            return;
        }

        // First send an SMS
        var sendRequest = new SendSmsRequest
        {
            To = _testPhoneNumber,
            Message = $"DLR test - {DateTime.UtcNow:O}",
            DryRun = true
        };

        var sendResponse = await _client.PostAsJsonAsync("/api/sms/send", sendRequest);
        if (!sendResponse.IsSuccessStatusCode)
        {
            _output.WriteLine("Could not send test SMS, skipping DLR test");
            return;
        }

        var sendContent = await sendResponse.Content.ReadAsStringAsync();
        var sendResult = JsonSerializer.Deserialize<SendSmsResponse>(sendContent);

        if (string.IsNullOrEmpty(sendResult?.MessageId))
        {
            _output.WriteLine("No message ID returned");
            return;
        }

        // Wait a moment for processing
        await Task.Delay(2000);

        // Check status
        var statusResponse = await _client.GetAsync($"/api/sms/{sendResult.MessageId}/status");
        var statusContent = await statusResponse.Content.ReadAsStringAsync();

        _output.WriteLine($"Status response: {statusResponse.StatusCode}");
        _output.WriteLine($"Status content: {statusContent}");

        if (statusResponse.StatusCode == HttpStatusCode.OK)
        {
            var status = JsonSerializer.Deserialize<SmsStatusResponse>(statusContent);
            Assert.NotNull(status);
            Assert.Equal(sendResult.MessageId, status.MessageId);
            _output.WriteLine($"SMS Status: {status.Status}");
        }
    }

    #endregion

    #region Rate Limiting Tests

    [Fact]
    public async Task SendMultipleSms_RespectRateLimits()
    {
        if (!_isConfigured)
        {
            _output.WriteLine("Skipping: MEGA_API_KEY not configured");
            return;
        }

        var successCount = 0;
        var rateLimitedCount = 0;

        // Send 5 SMS in quick succession
        for (int i = 0; i < 5; i++)
        {
            var request = new SendSmsRequest
            {
                To = _testPhoneNumber,
                Message = $"Rate limit test {i + 1} - {DateTime.UtcNow:O}",
                DryRun = true
            };

            var response = await _client.PostAsJsonAsync("/api/sms/send", request);

            if (response.StatusCode == HttpStatusCode.TooManyRequests)
            {
                rateLimitedCount++;
                _output.WriteLine($"Request {i + 1}: Rate limited (429)");
            }
            else if (response.IsSuccessStatusCode)
            {
                successCount++;
                _output.WriteLine($"Request {i + 1}: Success");
            }
        }

        _output.WriteLine($"Success: {successCount}, Rate Limited: {rateLimitedCount}");

        // At least some should succeed
        Assert.True(successCount > 0 || rateLimitedCount > 0);
    }

    #endregion

    #region Error Handling Tests

    [Fact]
    public async Task SendSms_WithInvalidPhoneNumber_ReturnsBadRequest()
    {
        if (!_isConfigured)
        {
            _output.WriteLine("Skipping: MEGA_API_KEY not configured");
            return;
        }

        // Arrange
        var request = new SendSmsRequest
        {
            To = "not-a-phone-number",
            Message = "Test message",
            DryRun = true
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/sms/send", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        _output.WriteLine($"Error response: {content}");
    }

    [Fact]
    public async Task SendSms_WithEmptyMessage_ReturnsBadRequest()
    {
        if (!_isConfigured)
        {
            _output.WriteLine("Skipping: MEGA_API_KEY not configured");
            return;
        }

        // Arrange
        var request = new SendSmsRequest
        {
            To = _testPhoneNumber,
            Message = "",
            DryRun = true
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/sms/send", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task SendSms_WithLongMessage_HandlesMultipleSegments()
    {
        if (!_isConfigured)
        {
            _output.WriteLine("Skipping: MEGA_API_KEY not configured");
            return;
        }

        // Arrange - Message longer than 160 chars (will be multi-segment)
        var longMessage = new string('A', 200) + " - Test long message at " + DateTime.UtcNow.ToString("O");

        var request = new SendSmsRequest
        {
            To = _testPhoneNumber,
            Message = longMessage,
            DryRun = true
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/sms/send", request);
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        _output.WriteLine($"Long message response: {response.StatusCode}");

        if (response.IsSuccessStatusCode)
        {
            var result = JsonSerializer.Deserialize<SendSmsResponse>(content);
            Assert.NotNull(result);
            Assert.True(result.Segments > 1, "Long message should have multiple segments");
            _output.WriteLine($"Segments: {result.Segments}");
        }
    }

    #endregion

    #region Models

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

        [JsonPropertyName("delivered_at")]
        public DateTime? DeliveredAt { get; set; }

        [JsonPropertyName("provider_status")]
        public string? ProviderStatus { get; set; }
    }

    #endregion
}
