namespace PayGoHub.Domain.Enums;

/// <summary>
/// Type of token generation mode
/// </summary>
public enum TokenType
{
    /// <summary>Stateless token - no server-side state tracking</summary>
    Stateless,

    /// <summary>Stateful token - tracked on server with usage history</summary>
    Stateful
}
