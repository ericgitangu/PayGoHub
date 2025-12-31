namespace PayGoHub.Domain.Enums;

/// <summary>
/// Status of an M2M device command through its lifecycle
/// </summary>
public enum CommandStatus
{
    /// <summary>Command created, waiting to be sent to device</summary>
    Pending,

    /// <summary>Command sent to device via gateway</summary>
    Sent,

    /// <summary>Device acknowledged receipt of command</summary>
    Acknowledged,

    /// <summary>Command executed successfully</summary>
    Completed,

    /// <summary>Command execution failed</summary>
    Failed,

    /// <summary>Command timed out waiting for response</summary>
    TimedOut
}
