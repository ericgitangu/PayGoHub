using PayGoHub.Application.DTOs.Mega;

namespace PayGoHub.Application.Interfaces;

/// <summary>
/// Mega SMS service interface for sending SMS messages
/// </summary>
public interface IMegaSmsService
{
    /// <summary>
    /// Send an SMS message via Mega
    /// </summary>
    Task<SmsResponseDto> SendSmsAsync(SmsRequestDto request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Process delivery report callback from Mega
    /// </summary>
    Task ProcessDeliveryReportAsync(DeliveryReportDto report, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get SMS status by Mega SMS ID
    /// </summary>
    Task<SmsResponseDto?> GetSmsStatusAsync(int megaSmsId, CancellationToken cancellationToken = default);
}
