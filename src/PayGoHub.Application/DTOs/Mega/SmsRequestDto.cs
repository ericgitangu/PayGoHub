namespace PayGoHub.Application.DTOs.Mega;

/// <summary>
/// SMS sending request DTO matching Mega API format
/// POST /send_short_message
/// </summary>
public class SmsRequestDto
{
    /// <summary>
    /// SMS text (max 160 chars)
    /// </summary>
    public string Text { get; set; } = string.Empty;

    /// <summary>
    /// Recipient phone number (no leading '+')
    /// </summary>
    public string Recipient { get; set; } = string.Empty;

    /// <summary>
    /// Sender phone number (no leading '+')
    /// </summary>
    public string Sender { get; set; } = string.Empty;

    /// <summary>
    /// SMS ID on SolarHub (for DLR updates, deduplication, debugging)
    /// </summary>
    public int InstanceSmsId { get; set; }

    /// <summary>
    /// Category for filtering (e.g., "template-name-category-name")
    /// </summary>
    public string? Category { get; set; }
}

/// <summary>
/// SMS response DTO
/// </summary>
public class SmsResponseDto
{
    /// <summary>
    /// Status code (200 = OK, 303 = Duplicate, 400 = Invalid, 401 = Auth failed)
    /// </summary>
    public int Status { get; set; }

    /// <summary>
    /// Description message
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Mega SMS ID (returned on success)
    /// </summary>
    public int? MegaSmsId { get; set; }
}

/// <summary>
/// Delivery report callback DTO
/// </summary>
public class DeliveryReportDto
{
    public int MegaSmsId { get; set; }
    public int InstanceSmsId { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? ErrorCode { get; set; }
    public DateTime? DeliveredAt { get; set; }
}
