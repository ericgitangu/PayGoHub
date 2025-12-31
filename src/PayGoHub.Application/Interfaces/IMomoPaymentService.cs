using PayGoHub.Application.DTOs.MoMo;

namespace PayGoHub.Application.Interfaces;

/// <summary>
/// MoMo payment validation and confirmation service
/// </summary>
public interface IMomoPaymentService
{
    /// <summary>
    /// Validate a customer account for payment
    /// </summary>
    Task<ValidationResponseDto> ValidateAsync(ValidationRequestDto request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Confirm a payment transaction
    /// </summary>
    Task<ConfirmationResponseDto> ConfirmAsync(ConfirmationRequestDto request, CancellationToken cancellationToken = default);
}
