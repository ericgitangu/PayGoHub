using Microsoft.Extensions.Logging;
using PayGoHub.Application.DTOs.Mega;
using PayGoHub.Application.Interfaces;

namespace PayGoHub.Infrastructure.Services;

/// <summary>
/// Mega SMS service for sending SMS messages (token delivery, notifications)
/// Matches Mega Rails API: /send_short_message
/// </summary>
public class MegaSmsService : IMegaSmsService
{
    private readonly ILogger<MegaSmsService> _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    private static readonly Dictionary<string, SmsRecord> _smsStore = new(); // In-memory for demo
    private static int _nextMegaSmsId = 1;

    public MegaSmsService(ILogger<MegaSmsService> logger, IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
    }

    public async Task<SmsResponseDto> SendSmsAsync(SmsRequestDto request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Sending SMS to {Recipient}, instance_sms_id: {InstanceSmsId}, category: {Category}",
            request.Recipient, request.InstanceSmsId, request.Category);

        try
        {
            // Validate request
            if (string.IsNullOrEmpty(request.Recipient))
            {
                return new SmsResponseDto
                {
                    Status = 400,
                    Description = "Recipient phone number is required"
                };
            }

            if (string.IsNullOrEmpty(request.Text))
            {
                return new SmsResponseDto
                {
                    Status = 400,
                    Description = "SMS text is required"
                };
            }

            // Check for duplicate by instance_sms_id
            var existingKey = $"instance_{request.InstanceSmsId}";
            if (request.InstanceSmsId > 0 && _smsStore.ContainsKey(existingKey))
            {
                _logger.LogInformation("Duplicate SMS detected for instance_sms_id: {InstanceSmsId}", request.InstanceSmsId);
                var existing = _smsStore[existingKey];
                return new SmsResponseDto
                {
                    Status = 303, // See Other - duplicate
                    Description = "Duplicate SMS, already queued",
                    MegaSmsId = existing.MegaSmsId
                };
            }

            // Generate Mega SMS ID
            var megaSmsId = Interlocked.Increment(ref _nextMegaSmsId);

            // Store SMS record
            var record = new SmsRecord
            {
                MegaSmsId = megaSmsId,
                InstanceSmsId = request.InstanceSmsId,
                Recipient = request.Recipient,
                Sender = request.Sender,
                Text = request.Text,
                Category = request.Category,
                Status = "queued",
                CreatedAt = DateTime.UtcNow
            };

            _smsStore[$"mega_{megaSmsId}"] = record;
            if (request.InstanceSmsId > 0)
            {
                _smsStore[existingKey] = record;
            }

            _logger.LogInformation("SMS queued with mega_sms_id: {MegaSmsId}", megaSmsId);

            // In production, this would send to actual SMS provider
            // For now, simulate async send by returning queued status
            _ = Task.Run(async () =>
            {
                await Task.Delay(500); // Simulate network latency
                record.Status = "sent";
                record.SentAt = DateTime.UtcNow;
                _logger.LogInformation("SMS {MegaSmsId} marked as sent", megaSmsId);
            }, cancellationToken);

            return new SmsResponseDto
            {
                Status = 200,
                Description = "SMS queued for delivery",
                MegaSmsId = megaSmsId
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send SMS to {Recipient}", request.Recipient);
            return new SmsResponseDto
            {
                Status = 500,
                Description = $"Internal error: {ex.Message}"
            };
        }
    }

    public async Task ProcessDeliveryReportAsync(DeliveryReportDto report, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Processing DLR for mega_sms_id: {MegaSmsId}, status: {Status}",
            report.MegaSmsId, report.Status);

        var key = $"mega_{report.MegaSmsId}";
        if (_smsStore.TryGetValue(key, out var record))
        {
            record.Status = report.Status;
            record.DeliveredAt = report.DeliveredAt;
            record.ErrorCode = report.ErrorCode;

            _logger.LogInformation("SMS {MegaSmsId} status updated to {Status}", report.MegaSmsId, report.Status);
        }
        else
        {
            _logger.LogWarning("DLR for unknown mega_sms_id: {MegaSmsId}", report.MegaSmsId);
        }

        await Task.CompletedTask;
    }

    public async Task<SmsResponseDto?> GetSmsStatusAsync(int megaSmsId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting status for mega_sms_id: {MegaSmsId}", megaSmsId);

        var key = $"mega_{megaSmsId}";
        if (_smsStore.TryGetValue(key, out var record))
        {
            return new SmsResponseDto
            {
                Status = 200,
                Description = record.Status,
                MegaSmsId = megaSmsId
            };
        }

        return await Task.FromResult<SmsResponseDto?>(null);
    }

    private class SmsRecord
    {
        public int MegaSmsId { get; set; }
        public int InstanceSmsId { get; set; }
        public string Recipient { get; set; } = string.Empty;
        public string Sender { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
        public string? Category { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? SentAt { get; set; }
        public DateTime? DeliveredAt { get; set; }
        public string? ErrorCode { get; set; }
    }
}
